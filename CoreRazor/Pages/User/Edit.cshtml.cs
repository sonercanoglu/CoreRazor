using CoreRazor.Data;
using CoreRazor.Models;
using CoreRazor.Services;
using KobiMuhasebe.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreRazor.Pages.User
{
    [Authorize(Roles = Constants.StockRole)]
    public class EditModel : BasePageModel<EditModel>
    {
        public EditModel(CoreRazorDbContext context, ILogger<EditModel> log) : base(context, log)
        {
        }

        [BindProperty]
        public UserView userView { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {

            if (id == null)
                return NotFound();

            try
            {
                Models.User user = await _context.Users.Where(m => m.Id == id).FirstOrDefaultAsync();
                if (user == null)
                    return NotFound();

                ViewData["RoleList"] = _context.Roles.ToList();

                userView = new UserView();
                userView = AddDefaultUserViewRoles(userView);
                userView.Id = user.Id;
                userView.Username = user.Username;
                userView.Password = user.Password;
                userView.Employee_Id = user.Employee_Id;
                userView.EmployeeName = user.Employee.Name + " " + user.Employee.Surname;

                UserRoleView userRoleView;
                foreach (UserRole ur in user.UserRoles)
                {
                    userRoleView = userView.UserRoleViews.Where(m => m.Id == ur.Role_Id).FirstOrDefault();
                    if (userRoleView != null)
                    {
                        userRoleView.Selected = true;
                        userRoleView.UserRole_Id = ur.Id;
                    }
                }

                return Page();
            }
            catch (Exception ex)
            {
                //With this code, we save in log files what we want.
                _log.LogError(ex.Message + ". ID : {0}", id);
                TempData["ErrorMessage"] = ex.Message;

                return RedirectToPage("Error");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Models.User user = await _context.Users.Where(m => m.Id == userView.Id).FirstOrDefaultAsync();
                    if (user == null)
                        return NotFound();

                    user.Password = userView.Password;
                    user.Username = userView.Username;

                    user.UserRoles.Where(m => userView.UserRoleViews.Any(o => o.UserRole_Id == m.Id && o.Selected == false)).ToList()
                        .ForEach(d => _context.UserRoles.Remove(d));

                    userView.UserRoleViews.Where(m => m.UserRole_Id == 0 && m.Selected).ToList()
                        .ForEach(a => user.UserRoles.Add(new UserRole { Role_Id = a.Id, User = user }));

                    _context.Attach(user).State = EntityState.Modified;

                    //With this code, we save the entity history.
                    _context.EnsureAutoHistory();
                    await _context.SaveChangesAsync();

                    //With this code, we save in log files what we want.
                    _log.LogInformation("ID = {0}, Record Updated", user.Id);

                    return RedirectToPage("Index", new { messageType = MessageType.Update });
                }
                catch (Exception ex)
                {
                    //With this code, we save in log files what we want.
                    _log.LogError(ex.Message + ". ID : {0}", userView.Id);
                    TempData["ErrorMessage"] = ex.Message;

                    return RedirectToPage("Error");
                }
            }

            return Page();
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