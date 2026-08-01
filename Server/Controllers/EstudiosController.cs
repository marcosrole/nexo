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
    public class EstudiosController : ControllerBase
    {
        private readonly NexoDbContext _db;

        public EstudiosController(NexoDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Estudio>>> Get()
        {
            var estudios = await _db.Estudios.OrderBy(e => e.Nombre).ToListAsync();
            return Ok(estudios);
        }

        [HttpPost]
        public async Task<ActionResult<Estudio>> Post(Estudio estudio)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            estudio.Id = 0;
            _db.Estudios.Add(estudio);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = estudio.Id }, estudio);
        }
    }
}
