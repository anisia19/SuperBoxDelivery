using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Username is required")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; private set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }
        public bool IsAdmin { get; set; }
        public List<Order> Orders { get; set; }
        public User() { }
        public User(string userName, string password, string mail)
        {
            UserName = userName;
            SetPassword(password);
            Email = mail;
            IsAdmin = false;
        }

        // Method to hash the password
        public void SetPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password), "Password cannot be null or empty");
            }
            PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
            Password = passwordHasher.HashPassword(this, password);
        }
        public bool VerifyPassword(string password)
        {
            var passwordHasher = new PasswordHasher<User>();
            var result = passwordHasher.VerifyHashedPassword(this, Password, password);
            return result == PasswordVerificationResult.Success;
        }
    }
}