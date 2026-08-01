using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexo.Server.Data;
using Nexo.Shared.Models;

namespace Nexo.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TareasController : ControllerBase
    {
        private readonly NexoDbContext _db;

        public TareasController(NexoDbContext db)
        {
            _db = db;
        }

        [HttpGet("catalogo")]
        public async Task<ActionResult<IEnumerable<TareaCatalogo>>> Catalogo()
        {
            var tareas = await _db.TareasCatalogo
                .Where(t => t.Activo)
                .OrderBy(t => t.Nombre)
                .ToListAsync();

            return Ok(tareas);
        }
    }
}
