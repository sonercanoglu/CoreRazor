using CoreRazor.Data;
using CoreRazor.Services;
using KobiMuhasebe.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CoreRazor.Pages.BrandModel
{
    [Authorize(Roles = Constants.StockRole)]
    public class CreateModel : BasePageModel<CreateModel>
    {
        public CreateModel(CoreRazorDbContext context, ILogger<CreateModel> log) : base(context, log)
        {
        }

        [BindProperty]
        public Models.BrandModel brandModel { get; set; }

        public IActionResult OnGet()
        {
            ViewData["BrandList"] = new SelectList(_context.Brands, "Id", "Name");

            brandModel = new Models.BrandModel();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.BrandModels.Add(brandModel);

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

            return Page();
        }
    }
}