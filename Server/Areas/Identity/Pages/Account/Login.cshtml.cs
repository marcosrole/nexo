using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nexo.Server.Data;

namespace Nexo.Server.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public LoginModel(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ErrorMessage { get; set; }

        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "El DNI es obligatorio.")]
            [Display(Name = "DNI")]
            public string Dni { get; set; }

            [Required(ErrorMessage = "La contraseña es obligatoria.")]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var result = await _signInManager.PasswordSignInAsync(
                Input.Dni, Input.Password, isPersistent: true, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                return LocalRedirect(string.IsNullOrEmpty(ReturnUrl) ? "~/dashboard" : ReturnUrl);
            }

            ErrorMessage = result.IsLockedOut
                ? "La cuenta quedó bloqueada temporalmente por varios intentos fallidos."
                : "DNI o contraseña incorrectos.";

            return Page();
        }
    }
}
