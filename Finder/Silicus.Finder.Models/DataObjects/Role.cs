﻿using System.ComponentModel.DataAnnotations;

namespace Silicus.Finder.Models.DataObjects
{
    public class Role
    {
        public int RoleId { get; set; }

        [Required]
        public string RoleName { get; set; }
        //[Required]
        public string Description { get; set; }
    }
}
