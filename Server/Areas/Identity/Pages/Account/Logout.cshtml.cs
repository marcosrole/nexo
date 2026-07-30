using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nexo.Server.Data;

namespace Nexo.Server.Areas.Identity.Pages.Account
{
    // El botón "Salir" se renderiza como HTML estático dentro de un componente Blazor,
    // no como una vista Razor Pages — no puede incluir el token antifalsificación habitual.
    [IgnoreAntiforgeryToken]
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public LogoutModel(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _signInManager.SignOutAsync();
            return LocalRedirect("~/");
        }
    }
}
