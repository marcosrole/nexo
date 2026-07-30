using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Nexo.Server.Data;
using Nexo.Shared.Models;

namespace Nexo.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsuariosController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // Usuarios de staff (Administrador/Operador), candidatos a "Productor responsable" de un proyecto.
        [HttpGet("staff")]
        public async Task<ActionResult<IEnumerable<UsuarioResumen>>> GetStaff()
        {
            var administradores = await _userManager.GetUsersInRoleAsync("Administrador");
            var operadores = await _userManager.GetUsersInRoleAsync("Operador");

            var staff = administradores.Concat(operadores)
                .Where(u => u.Activo)
                .GroupBy(u => u.Id)
                .Select(g => g.First())
                .OrderBy(u => u.NombreCompleto)
                .Select(u => new UsuarioResumen { Id = u.Id, NombreCompleto = u.NombreCompleto ?? u.UserName })
                .ToList();

            return Ok(staff);
        }
    }
}
