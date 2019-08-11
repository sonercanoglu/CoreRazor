using CoreRazor.Data;
using CoreRazor.Models;
using CoreRazor.Services;
using KobiMuhasebe.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreRazor.Pages.User
{
    [Authorize(Roles = Constants.StockRole)]
    public class CreateModel : BasePageModel<CreateModel>
    {
        public CreateModel(CoreRazorDbContext context, ILogger<CreateModel> log) : base(context, log)
        {
        }

        [BindProperty]
        public UserView userView { get; set; }

        public IActionResult OnGet()
        {
            FillList();

            userView = new UserView();
            userView = AddDefaultUserViewRoles(userView);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Models.User user = new Models.User();
                    user.Employee_Id = userView.Employee_Id;
                    user.Password = userView.Password;
                    user.Username = userView.Username;

                    user.UserRoles = new List<UserRole>();

                    foreach (UserRoleView userRoleView in userView.UserRoleViews)
                    {
                        if (userRoleView.Selected)
                            user.UserRoles.Add(new UserRole { Role_Id = userRoleView.Id, User = user });
                    }

                    _context.Users.Add(user);

                    //With this code, we save the entity history.
                    _context.EnsureAutoHistory();
                    await _context.SaveChangesAsync();

                    //With this code, we save in log files what we want.
                    _log.LogInformation("New Record Added");
                    return RedirectToPage("Index", new { messageType = MessageType.Add });
                }
                catch (Exception ex)
                {
                    //With this code, we save in log files what we want.
                    _log.LogError(ex.Message);
                    TempData["ErrorMessage"] = ex.Message;

                    return RedirectToPage("Error");
                }
            }

            FillList();
            userView.Employee_Id = 0;

            return Page();
        }

        /// <summary>
        /// Get All Employee List, who is not an User
        /// Get Role List
        /// </summary>
        public void FillList()
        {
            var list = _context.Employees.Where(m => m.User == null).Select(m => new { Id = m.Id, Text = m.Name + " " + m.Surname }).ToList();

            ViewData["EmployeeList"] = new SelectList(list, "Id", "Text");
            ViewData["RoleList"] = _context.Roles.ToList();
        }

        /// <summary>
        /// Fill Role List in to the UserRoleViews
        /// </summary>
        /// <param name="userView"></param>
        /// <returns></returns>
        public UserView AddDefaultUserViewRoles(UserView userView)
        {
            userView.UserRoleViews = new List<UserRoleView>();
            foreach (Role r in (List<Role>)ViewData["RoleList"])
                userView.UserRoleViews.Add(new UserRoleView { Id = r.Id, Name = r.Name, Selected = false, UserRole_Id = 0 });

            return userView;
        }
    }
}