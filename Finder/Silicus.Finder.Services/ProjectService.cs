﻿using Aspose.Cells;
using Silicus.Finder.Entities;
using Silicus.Finder.Models.DataObjects;
using Silicus.Finder.Models.Models;
using Silicus.Finder.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Silicus.Finder.Services
{
    public class ProjectService : IProjectService
    {


        private readonly IDataContext _context;
        IEmployeeService _employeeService;

        public ProjectService(IDataContextFactory dataContextFactory, IEmployeeService employeeService)
        {

            _context = dataContextFactory.Create(ConnectionType.Ip);
            _employeeService = employeeService;
        }

        public int AddProject(Project project)
        {
            _context.Add(project);
            return project.ProjectId;
        }

        public int UpdateProject(Project project)
        {
            _context.Update<Project>(project);
            return project.ProjectId;
        }

        public void ArchiveProject(int projectId)
        {
            var project = _context.Query<Project>().FirstOrDefault(p => p.ProjectId == projectId);
            project.ArchiveDate = DateTime.Now.Date;
            _context.Update<Project>(project);
        }

        public IEnumerable<Project> GetProjects()
        {
            var projectList = _context.Query<Project>().Where(p => p.ArchiveDate == null).ToList();
            return projectList;
        }

        public Project GetProjectById(int? projectId)
        {
            var project = _context.Query<Project>().Where(model => model.ProjectId == projectId).FirstOrDefault();
            return project;
        }

        public List<Project> GetProjectsByCriteria(ProjectSearchCriteriaModel criteria)
        {
            var projectList = GetProjects().ToList();

            if (projectList.Count() != 0)
            {
                projectList = projectList.Where(p => p.Status.ToString().Equals(string.IsNullOrEmpty(criteria.Status) ? p.Status.ToString() : criteria.Status.Trim())
                    && p.ProjectType.ToString().Equals(string.IsNullOrEmpty(criteria.ProjectType) ? p.ProjectType.ToString() : criteria.ProjectType.Trim())
                    && p.SkillSets.Any(s => s.Name.Equals(string.IsNullOrEmpty(criteria.SkillSet) ? s.Name : criteria.SkillSet.Trim()))).ToList();

                if (!string.IsNullOrEmpty(criteria.EngagementManager) && projectList.Count() != 0)
                {
                    var employeeIds = _employeeService.GetEmployeeByName(criteria.EngagementManager).Select(pm => pm.EmployeeId).ToList();
                    projectList = projectList.Where(p => employeeIds.Contains(Convert.ToInt32(p.EngagementManagerId))).ToList();
                }

                if (!string.IsNullOrEmpty(criteria.ProjectManager) && projectList.Count() != 0)
                {
                    var employeeIds = _employeeService.GetEmployeeByName(criteria.ProjectManager).Select(pm => pm.EmployeeId).ToList();
                    projectList = projectList.Where(p => employeeIds.Contains(Convert.ToInt32(p.ProjectManagerId))).ToList();
                }
            }

            return projectList;
        }

        public IEnumerable<Project> GetProjectsByName(string projectName)
        {
            var projectList = _context.Query<Project>().Where(e => e.ProjectName.Contains(projectName.Trim())).ToList();

            if (projectList.Count == 0)
            {
                return _context.Query<Project>().Where(e => e.ProjectCode.Equals(projectName.Trim())).ToList();
            }

            return projectList;
        }

        public Project GetProjectDetails(int projectId)
        {
            var project = _context.Query<Project>().FirstOrDefault(p => p.ProjectId == projectId);
            return project;
        }

        public List<SkillSet> GetAllSkills()
        {
            var skills = _context.Query<SkillSet>().ToList();
            return skills;
        }

        public SkillSet GetSkillSetById(int? projectId)
        {
            var skillset = _context.Query<SkillSet>().Where(model => model.SkillSetId == projectId).FirstOrDefault();
            return skillset;
        }

        public List<Employee> GetAllEmployees()
        {
            var allEmployeeList = _context.Query<Employee>().ToList();
            foreach (Employee emp in allEmployeeList)
            {
                if (emp.EmployeeTitles.Count > 0)
                {
                    var empTitle = emp.EmployeeTitles.Where(title => title.IsCurrent == true).SingleOrDefault().Title;
                    emp.Title = empTitle.Name;
                    emp.TitleId = empTitle.TitleId;
                }
            }

            return allEmployeeList;
        }

        public List<Employee> GetAllManagers()
        {
            var allEmployeeList = GetAllEmployees();
            var managers = allEmployeeList.Where(emp => emp.TitleId == 1).ToList();
            return managers;
        }


        public Employee GetEmployeeById(int? projectId)
        {
            var employee = _context.Query<Employee>().Where(model => model.EmployeeId == projectId).FirstOrDefault();
            return employee;
        }

        public IEnumerable<Employee> GetEmployeesAssignedToProject(int projectId)
        {
            var employeesOnProject = _context.Query<Project>().Where(model => model.ProjectId == projectId).First().Employees;
            return employeesOnProject;
        }

        public int AllocateEmployeesToProject(int projectId, int[] employeeIds)
        {
            var project = _context.Query<Project>().FirstOrDefault(p => p.ProjectId == projectId);
            _context.Attach<Project>(project);

            foreach (int empId in employeeIds)
            {
                var employeeToAllocate = _context.Query<Employee>().Where(model => model.EmployeeId == empId).FirstOrDefault();
                _context.Attach<Employee>(employeeToAllocate);
                project.Employees.Add(employeeToAllocate);
            }

            var updatedProjectId = _context.Update<Project>(project);
            return updatedProjectId;
        }

        public int DeallocateEmployeeFromProject(int empId, int projectId)
        {
            Project project = _context.Query<Project>().Where(model => model.ProjectId == projectId).FirstOrDefault();
            Employee employeeToRemove = project.Employees.Where(model => model.EmployeeId == empId).FirstOrDefault();

            _context.Attach<Project>(project);
            _context.Attach<Employee>(employeeToRemove);
            var isEmployeeRemoved = project.Employees.Remove(employeeToRemove);

            var updatedProjectId = _context.Update<Project>(project);
            return updatedProjectId;
        }

        public int AddSkillToProject(int[] skillIds, int projectID)
        {
            var project = _context.Query<Project>().FirstOrDefault(p => p.ProjectId == projectID);
            _context.Attach<Project>(project);

            foreach (int skillId in skillIds)
            {
                var skillToAdd = _context.Query<SkillSet>().Where(model => model.SkillSetId == skillId).FirstOrDefault();
                _context.Attach<SkillSet>(skillToAdd);
                project.SkillSets.Add(skillToAdd);
            }
            var updatedProjectId = _context.Update<Project>(project);
            return updatedProjectId;
        }

        public int RemoveSkillFromProject(int skillId, int projectId)
        {
            var project = _context.Query<Project>().Where(model => model.ProjectId == projectId).FirstOrDefault();
            var skillToRemove = _context.Query<SkillSet>().Where(model => model.SkillSetId == skillId).FirstOrDefault();

            _context.Attach<Project>(project);
            _context.Attach<SkillSet>(skillToRemove);
            var isRemoved = project.SkillSets.Remove(skillToRemove);

            var updatedProjectId = _context.Update<Project>(project);
            return updatedProjectId;
        }

        public int GetProjectCountBySkill(int skillSetId)
        {
            var projectCount = _context.Query<Project>().Where(model => model.SkillSets.Any(r => r.SkillSetId == skillSetId)).Count();
            return projectCount;
        }


        //Import Projects..
        public List<Project> ImportProjectsFromExcel(string path)
        {
            LoadOptions loadOptionForXlsx = new LoadOptions(LoadFormat.Xlsx);
            Workbook workbook = new Workbook(path, loadOptionForXlsx);
            char column = 'A';
            var project = new Project();
            var projects = new List<Project>();
            var dbProjects = GetProjects();


            var rowcount = workbook.Worksheets["Sheet1"].Cells.MaxDataRow;



            for (int index = 2; index <= rowcount + 1; )
            {

            NewRow:
                var cell = workbook.Worksheets["Sheet1"].Cells[column + index.ToString()];
                switch (column)
                {

                    case 'A':
                        project.ProjectName = cell.StringValue;
                        ++column;
                        break;
                    case 'B':
                        project.ProjectCode = cell.StringValue;
                        ++column;
                        break;
                    case 'C':
                        project.Description = cell.StringValue;
                        ++column;
                        break;
                    case 'D':
                        project.ProjectType = ParseEnum<ProjectType>(cell.StringValue);
                        ++column;
                        break;
                    case 'E':
                        project.EngagementType = ParseEnum<EngagementType>(cell.StringValue);
                        ++column;
                        break;

                    case 'F':
                        project.EngagementManagerId = cell.IntValue;
                        ++column;
                        break;

                    case 'G':
                        project.ProjectManagerId = cell.IntValue;
                        ++column;
                        break;
                    case 'H':
                        project.Status = ParseEnum<Status>(cell.StringValue);
                        ++column;
                        break;
                    case 'I':
                        project.StartDate = cell.DateTimeValue;
                        ++column;
                        break;
                    case 'J':
                        project.ExpectedEndDate = cell.DateTimeValue;
                        ++column;
                        break;
                    case 'K': project.ActualEndDate = cell.DateTimeValue;
                        ++column;
                        break;


                    case 'L':
                        project.AdditionalNotes = cell.StringValue;
                        ++column;
                        break;



                    case 'M':

                        foreach (var dbproject in dbProjects)
                        {
                            if (dbproject.ProjectCode == project.ProjectCode)
                            {

                                ErrorLog(@"D:\Error.txt", "Problem while updating row " + index + " in Project Code");
                                project = new Project();
                                ++index;
                                column = 'A';
                                goto NewRow;
                            }


                        }

                        AddProject(project);
                        project = AddSkills(project, cell.StringValue);
                        project = new Project();
                        ++index;
                        column = 'A';
                        break;




                }
            }
            return projects;
        }

        private Project AddSkills(Project targetProject, string skills)
        {
            _context.Attach<Project>(targetProject);
            var skillList = SeparateAllSkills(skills);

            var skillSet = new List<int>();
            var dbSkills = GetAllSkills();

            foreach (var skillName in skillList)
            {
                foreach (var skill in dbSkills)
                {
                    if (skill.Name.ToLower() == skillName.ToLower())
                    {

                        skillSet.Add(skill.SkillSetId);
                        //  skillSet[i] = skill.SkillSetId;
                        // i++;
                    }
                }

            }

            //  AddSkillToProject(skillSet.ToArray(), targetProject.ProjectId);

            // AttachEntity(targetProject, skillSet);
            foreach (int skillId in skillSet)
            {
                if (skillId >= 1)
                {
                    var skillToAdd = GetSkillSetById(skillId);

                    try
                    {
                        _context.Attach<SkillSet>(skillToAdd);
                        targetProject.SkillSets.Add(skillToAdd);
                        _context.Update<Project>(targetProject);

                    }
                    catch (Exception ex)
                    {
                        ErrorLog(@"D:\Error.txt", "Problem while updating the skills for project with ProjectId " +targetProject.ProjectId  );

                    }


                }
            }
            return targetProject;
        }


        private List<string> SeparateAllSkills(string line)
        {
            var skillList = line.Split(',').ToList();
            var AllSkills = new List<string>();
            foreach (var skill in skillList)
            {
                if (skill.Contains('/'))
                {

                    var version = skill.Split(' ');
                    string name = "";
                    int i = 0;
                    while (version[i].Contains('/') == false)
                    {
                        name = name + ' ' + version[i];
                        i++;
                    }
                    var x = version[i].Split('/');
                    for (int j = 0; j < x.Length; j++)
                    {
                        AllSkills.Add(name + ' ' + x[j]);
                    }

                }
                else
                {
                    AllSkills.Add(skill);
                }

            }
            return AllSkills;
        }


        public void ErrorLog(string pathName, string errorMessage)
        {
            StreamWriter streamwriter = new StreamWriter(pathName, true);
            streamwriter.WriteLine(errorMessage);
            streamwriter.Flush();
            streamwriter.Close();
        }



        public List<string> AddAllProjects(List<Project> projects)
        {
            var count = 0;
            var projectcodefailedtoadd = new List<string>();

            foreach (Project project in projects)
            {
                try
                {

                    _context.Add<Project>(project);
                    count++;
                }
                catch (Exception ex)
                {
                    projectcodefailedtoadd.Add(project.ProjectCode);
                }
            }

            return projectcodefailedtoadd;
        }

        public static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

    }
}