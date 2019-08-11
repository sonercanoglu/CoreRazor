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

namespace CoreRazor.Pages.Product
{
    [Authorize(Roles = Constants.StockRole)]
    public class CreateModel : BasePageModel<CreateModel>
    {
        public CreateModel(CoreRazorDbContext context, ILogger<CreateModel> log) : base(context, log)
        {
        }

        [BindProperty]
        public Models.Product product { get; set; }

        public IActionResult OnGet()
        {
            ViewData["BrandList"] = new SelectList(_context.Brands, "Id", "Name");

            product = new Models.Product();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Products.Add(product);

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

            ViewData["BrandList"] = new SelectList(_context.Brands, "Id", "Name");
            product.Brand_Id = 0;
            product.BrandModel_Id = 0;

            return Page();
        }

        public JsonResult OnGetModelList(int brand_id)
        {
            var result = new SelectList(_context.BrandModels.Where(m => m.Brand_Id == brand_id), "Id", "Name");
            return new JsonResult(result);
        }
    }
}