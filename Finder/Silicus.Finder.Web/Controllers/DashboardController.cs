﻿using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using Silicus.Finder.IdentityWrapper;
using Silicus.Finder.Services.Interfaces;

namespace Silicus.Finder.Web.Controllers
{
    
    public class DashboardController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly IProjectService _projectService;
        private readonly ISkillSetService _skillsetservice;

        public DashboardController(IEmployeeService employeeservice, IProjectService projectService, ISkillSetService skillsetservice)
        {
            _projectService = projectService;
            _employeeService = employeeservice;
            _skillsetservice = skillsetservice;
        }

        private ApplicationRoleManager _roleManager;
        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }

        [Authorize(Roles = "Admin, Manager,User")]
        public ActionResult Dashboard()
        {
            ViewBag.UserRoles = RoleManager.Roles.Select(r => new SelectListItem { Text = r.Name, Value = r.Name }).ToList();
            Session["uname"] = User.Identity.Name;
            @ViewBag.NumberOfEmployee = _employeeService.GetAllEmployees().Count();
            @ViewBag.NumberOfProjects = _projectService.GetProjects().Count();
            @ViewBag.NumberOfSkills = _skillsetservice.GetAllSkills().Count();

            return View();
        }
    }
}