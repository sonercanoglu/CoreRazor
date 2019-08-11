using CoreRazor.Data;
using KobiMuhasebe.Models;
using Microsoft.Extensions.Logging;

namespace CoreRazor.Pages.Home
{
    public class IndexModel : BasePageModel<IndexModel>
    {
        public IndexModel(CoreRazorDbContext context, ILogger<IndexModel> log) : base(context, log)
        {
        }
        public void OnGet()
        {

        }
    }
}