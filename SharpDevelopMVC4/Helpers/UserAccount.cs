using SharpDevelopMVC4.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

[Table("Users")]
public partial class UserAccount
{
    // Change this to your desired default admin login and password
    public const string DEFAULT_ADMIN_LOGIN = "admin";
    // Change this to your DbContext class
    private static SdMvc4DbContext _db = new SdMvc4DbContext();

    #region UserAccountRepository
    public int Id { get; set; }
    //Login info
    [Required]
    [StringLength(254)]
    public string UserName { get; set; }
    public byte[] PasswordHash { get; set; }
    public byte[] PasswordSalt { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? LastLogin { get; set; }
    public bool IsActive { get; set; }
    public string Roles { get; set; } // comma-separated


    private static UserAccount CurrentUser = null;

    public static bool Authenticate(string userName, string userPassword)
    {
        CreateAdmin(); // Comment out this line if you already have admin account

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(userPassword))
            return false;

        var user = _db.Users.Where(x => x.UserName == userName.Trim().ToLower()).FirstOrDefault();
        if (user == null)
            return false;
        if (!user.IsActive)
            return false;

        bool valid = VerifyPasswordHash(userPassword, user.PasswordSalt, user.PasswordHash);
        if (valid)
        {
            user.LastLogin = DateTime.Now;
            _db.Entry(user).State = System.Data.Entity.EntityState.Modified;
            _db.SaveChanges();
            CurrentUser = user; // Set current user
            return true;
        }

        return false;
    }

    private static bool VerifyPasswordHash(string userPassword, byte[] passwordSalt, byte[] passwordHash)
    {
        // Verify PasswordHash
        using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
        {
            var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(userPassword));
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != passwordHash[i])
                    return false;
            }
        }

        return true;
    }

    public static int? Create(string userName, string userPassword, string userRoles = "", bool requiresActivation = false)
    {
        if (string.IsNullOrWhiteSpace(userPassword))
            return null;
        if (string.IsNullOrWhiteSpace(userName))
            return null;

        var user = new UserAccount();
        user.UserName = userName.Trim().ToLower();

        var userExists = _db.Users.Where(x => x.UserName == user.UserName).Count() > 0;
        if (userExists)
            return null;

        // Create PasswordHash
        using (var hmac = new System.Security.Cryptography.HMACSHA512())
        {
            user.PasswordSalt = hmac.Key;
            user.PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(userPassword));
        }

        user.Roles = userRoles;
        user.CreatedOn = DateTime.Now;
        user.IsActive = !requiresActivation;

        _db.Users.Add(user);
        _db.SaveChanges();

        return user.Id;
    }

    private static void CreateAdmin()
    {
        var hasAdmin = _db.Users.Where(x => x.Roles == DEFAULT_ADMIN_LOGIN).Any();
        if (!hasAdmin)
        {
            Create(DEFAULT_ADMIN_LOGIN, DEFAULT_ADMIN_LOGIN, DEFAULT_ADMIN_LOGIN);
        }
    }

    public static bool ChangePassword(string userName, string userPassword = "", string newPassword = "", bool forceChange = false)
    {
        if (string.IsNullOrWhiteSpace(newPassword))
            return false;

        if (forceChange == false && string.IsNullOrWhiteSpace(userPassword))
            return false;

        var user = _db.Users.Where(x => x.UserName == userName.Trim()).FirstOrDefault();
        if (user == null)
            return false;


        var validPassword = !forceChange ? VerifyPasswordHash(userPassword, user.PasswordSalt, user.PasswordHash) : true;
        if (validPassword)
        {
            // Overwrite with new PasswordHash
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                user.PasswordSalt = hmac.Key;
                user.PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(newPassword));
            }
            
	        _db.Entry(user).State = System.Data.Entity.EntityState.Modified;
	        _db.SaveChanges();
	        return true;            
        }
		else
			return false;

    }

    public static List<UserAccount> GetAll()
    {
        var users = _db.Users.ToList();
        return users;
    }

    public static List<UserAccount> GetAllUsersInRole(string role)
    {
        var users = _db.Users.ToList().Where(x => x.Roles.Split(',').Contains(role)).ToList();
        return users;
    }

    public static UserAccount GetUserById(int userId)
    {
        var user = _db.Users.Find(userId);
        return user;
    }

    public static UserAccount GetUserByUserName(string userName)
    {
    	var user = _db.Users.Where(x => x.UserName.ToLower() == userName.ToLower()).FirstOrDefault();
        return user;
    }

    public static UserAccount GetCurrentUser()
    {
        return CurrentUser;
    }

    public static string[] GetUserRoles(int userId)
    {
        var user = GetUserById(userId);
        if (user != null)
            return user.Roles.Split(',');
        else
            return new string[] { string.Empty };
    }

    public static string[] GetUserRoles(string userName)
    {
        var user = GetUserByUserName(userName);
        return GetUserRoles(user.Id);
    }

    public static UserAccount Deactivate(string userName)
    {
        var user = _db.Users.Where(x => x.UserName == userName).FirstOrDefault();
        if (user != null)
        {
            user.IsActive = false;
            _db.Entry(user).State = System.Data.Entity.EntityState.Modified;
            _db.SaveChanges();
            return user;
        }
        else
            return null;

    }

    public static UserAccount Activate(string userName)
    {
        var user = _db.Users.Where(x => x.UserName == userName).FirstOrDefault();
        if (user != null)
        {
            user.IsActive = true;
            _db.Entry(user).State = System.Data.Entity.EntityState.Modified;
            _db.SaveChanges();
            return user;
        }
        else
            return null;

    }
    #endregion

}


