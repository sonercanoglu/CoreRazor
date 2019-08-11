using CoreRazor.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace KobiMuhasebe.Models
{
    public class BasePageModel<T> : PageModel
    {
        public CoreRazorDbContext _context;
        public ILogger<T> _log;
        public BasePageModel(CoreRazorDbContext context, ILogger<T> log)
        {
            _context = context;
            _log = log;
        }

        public IActionResult NotFound()
        {
            return RedirectToPage("NotFound");
        }
    }
}
