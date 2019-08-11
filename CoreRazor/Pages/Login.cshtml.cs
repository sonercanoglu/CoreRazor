using CoreRazor.Data;
using CoreRazor.Models;
using KobiMuhasebe.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CoreRazor.Pages
{
    [AllowAnonymous]
    public class LoginModel : BasePageModel<LoginModel>
    {
        public LoginModel(CoreRazorDbContext context, ILogger<LoginModel> log) : base(context, log)
        {
        }

        [BindProperty]
        [Required]
        public string username { get; set; }

        [BindProperty]
        [Required]
        public string password { get; set; }

        public IActionResult OnGet()
        {
            //We are logging out the User
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Models.User user = _context.Users.Where(m => m.Username == username && m.Password == password).FirstOrDefault();
            if (user != null)
            {
                //We are geting Username and it's Roles
                ClaimsIdentity claimsIdentity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                List<Claim> claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.Name, user.Username));
                foreach (UserRole userRole in user.UserRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));
                }

                claimsIdentity.AddClaims(claims);

                var principal = new ClaimsPrincipal(claimsIdentity);

                var login = HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                //Just redirect to our index after logging in. 
                return RedirectToPage("Home/Index");
            }

            return Page();
        }
    }
}