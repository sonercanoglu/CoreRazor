using CoreRazor.Data;
using CoreRazor.Services;
using KobiMuhasebe.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CoreRazor.Pages.BrandModel
{
    [Authorize(Roles = Constants.StockRole)]
    public class EditModel : BasePageModel<EditModel>
    {
        public EditModel(CoreRazorDbContext context, ILogger<EditModel> log) : base(context, log)
        {
        }

        [BindProperty]
        public Models.BrandModel brandModel { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                brandModel = await _context.BrandModels.Where(m => m.Id == id).FirstOrDefaultAsync();
                if (brandModel == null)
                    return NotFound();

                ViewData["BrandList"] = new SelectList(_context.Brands, "Id", "Name");

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
                    _context.Attach(brandModel).State = EntityState.Modified;

                    //With this code, we save the entity history.
                    _context.EnsureAutoHistory();
                    await _context.SaveChangesAsync();

                    //With this code, we save in log files what we want.
                    _log.LogInformation("ID = {0}, Record Updated", brandModel.Id);

                    return RedirectToPage("Index", new { messageType = MessageType.Update });
                }
                catch (Exception ex)
                {
                    //With this code, we save in log files what we want.
                    _log.LogError(ex.Message + ". ID : {0}", brandModel.Id);
                    TempData["ErrorMessage"] = ex.Message;

                    return RedirectToPage("Error");
                }
            }

            return Page();
        }
    }
}