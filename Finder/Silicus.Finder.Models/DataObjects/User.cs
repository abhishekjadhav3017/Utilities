﻿using System;
using System.ComponentModel.DataAnnotations;

namespace Silicus.Finder.Models.DataObjects
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Name is required!")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [CompareAttribute("NewPassword", ErrorMessage = "Passwords don't match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
        
        [Required]
        public string Address { get; set; }
        
        [Required]
        public bool isActive { get; set; }
        
        [Required]
        public string Role { get; set; }

        [Required]
        public string Email { get; set; }
        
        public Guid IdentityUserId { get; set; }
    }
}
