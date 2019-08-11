using CoreRazor.Data;
using CoreRazor.Services;
using KobiMuhasebe.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CoreRazor.Pages.Employee
{
    [Authorize(Roles = Constants.HumanResourceRole)]
    public class EditModel : BasePageModel<EditModel>
    {
        public EditModel(CoreRazorDbContext context, ILogger<EditModel> log) : base(context, log)
        {
        }

        [BindProperty]
        public Models.Employee employee { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                employee = await _context.Employees.Where(m => m.Id == id).FirstOrDefaultAsync();
                if (employee == null)
                    return NotFound();

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
                    _context.Attach(employee).State = EntityState.Modified;

                    //With this code, we save the entity history.
                    _context.EnsureAutoHistory();
                    await _context.SaveChangesAsync();

                    //With this code, we save in log files what we want.
                    _log.LogInformation("ID = {0}, Record Updated", employee.Id);

                    return RedirectToPage("Index", new { messageType = MessageType.Update });
                }
                catch (Exception ex)
                {
                    //With this code, we save in log files what we want.
                    _log.LogError(ex.Message + ". ID : {0}", employee.Id);
                    TempData["ErrorMessage"] = ex.Message;

                    return RedirectToPage("Error");
                }
            }

            return Page();
        }
    }
}