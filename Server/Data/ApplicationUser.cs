using Microsoft.AspNetCore.Identity;

namespace Nexo.Server.Data
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string NombreCompleto { get; set; }

        public int? ClienteId { get; set; }

        public bool Activo { get; set; } = true;
    }
}
