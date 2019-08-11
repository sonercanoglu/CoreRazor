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
using System.Linq;
using System.Threading.Tasks;

namespace CoreRazor.Pages.Product
{
    [Authorize(Roles = Constants.StockRole)]
    public class IndexModel : BasePageModel<IndexModel>
    {
        public IndexModel(CoreRazorDbContext context, ILogger<IndexModel> log) : base(context, log)
        {
        }

        [BindProperty]
        public PaginatedList<Models.Product> list { get; set; }

        [HttpGet]
        public async Task<IActionResult> OnGetAsync(MessageType messageType, int p_brand_Id, int p_brandModel_Id, int p = 1)
        {
            ViewData["BrandList"] = new SelectList(_context.Brands, "Id", "Name");

            //With this code, we get the message from Create,Edit,Delete functions and then if message is not empty, we show the message with Modal Dialog Popup
            //This Modal Dialog is in _Layout_Core_Mvc.cshtml Page. So i can use this Modal Dialog from all Pages.
            string message = Utils.GetMessage(messageType, "Product");
            if (message != "")
                ViewData["ModalMessage"] = message;

            ViewData["CurrentFilterBrandId"] = p_brand_Id;
            ViewData["CurrentFilterBrandModelId"] = p_brandModel_Id;

            list = new PaginatedList<Models.Product>();
            list._items = await _context.Products.Where(m => (m.Brand_Id == p_brand_Id || p_brand_Id == 0) && (m.BrandModel_Id == p_brandModel_Id || p_brandModel_Id == 0)).ToListAsync();

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
                Models.Product product = await _context.Products.Where(m => m.Id == id).FirstOrDefaultAsync();
                if (product == null)
                    return NotFound();

                _context.Products.Remove(product);

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

        public JsonResult OnGetModelList(int brand_id)
        {
            var result = new SelectList(_context.BrandModels.Where(m => m.Brand_Id == brand_id), "Id", "Name");
            return new JsonResult(result);
        }
    }
}