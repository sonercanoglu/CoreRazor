using CoreRazor.Data;
using CoreRazor.Models;
using CoreRazor.Services;
using KobiMuhasebe.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CoreRazor.Pages.Brand
{
    [Authorize(Roles = Constants.StockRole)]
    public class IndexModel : BasePageModel<IndexModel>
    {
        public IndexModel(CoreRazorDbContext context, ILogger<IndexModel> log) : base(context, log)
        {
        }

        [BindProperty]
        public PaginatedList<Models.Brand> list { get; set; }

        [HttpGet]
        public async Task<IActionResult> OnGetAsync(MessageType messageType, string name, int p = 1)
        {
            //With this code, we get the message from Create,Edit,Delete functions and then if message is not empty, we show the message with Modal Dialog Popup
            //This Modal Dialog is in _Layout_Core_Mvc.cshtml Page. So i can use this Modal Dialog from all Pages.
            string message = Utils.GetMessage(messageType, "Brand");
            if (message != "")
                ViewData["ModalMessage"] = message;

            name = name == null ? "" : name;
            ViewData["CurrentFilter"] = name;

            list = new PaginatedList<Models.Brand>();
            list._items = await _context.Brands.Where(m => m.Name.Contains(name)).ToListAsync();
            list._TotalRecords = list._items.Count;

            if (list._TotalRecords >= ((p - 1) * list._PageSize))
                list._PageIndex = p;
            else
                list._PageIndex = 1;

            list._items = list._items.OrderBy(m => m.Id).Skip((list._PageIndex - 1) * list._PageSize).Take(list._PageSize).ToList();

            return Page();
        }

        [HttpPost]
        public async Task<IActionResult> OnPostDeleteAsync(string deleteId)
        {
            int id = 0;
            if (!int.TryParse(deleteId, out id))
                throw new Exception("Wrong Id Information.");

            try
            {
                Models.Brand brand = await _context.Brands.Where(m => m.Id == id).FirstOrDefaultAsync();
                if (brand== null)
                    return NotFound();

                if ((brand.Products != null && brand.Products.Count != 0) || (brand.BrandModels != null && brand.BrandModels.Count != 0))
                    throw new Exception("There are related Records with this Brand. First You have to delete them!");

                _context.Brands.Remove(brand);

                //With this code, we save the entity history.
                _context.EnsureAutoHistory();
                await _context.SaveChangesAsync();

                //With this code, we save in log files what we want.
                _log.LogInformation("Record Deleted");

                return RedirectToPage("Index", new { messageType = MessageType.Delete });
            }
            catch (Exception ex)
            {
                //With this code, we save in log files what we want.
                _log.LogError(ex.Message + ". ID : {0}", id);
                TempData["ErrorMessage"] = ex.Message;

                return RedirectToPage("Error");
            }
        }
    }
}