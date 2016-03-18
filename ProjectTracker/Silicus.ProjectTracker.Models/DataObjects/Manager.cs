﻿using System.ComponentModel.DataAnnotations;

namespace Silicus.ProjectTracker.Models.DataObjects
{
    public class Manager
    {
        public int ManagerId { get; set; }

        [Required]
        public string ManagerName { get; set; }
    }
}
