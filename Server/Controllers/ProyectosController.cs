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
    public class ProyectosController : ControllerBase
    {
        private readonly NexoDbContext _db;

        public ProyectosController(NexoDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Proyecto>>> Get()
        {
            var proyectos = await _db.Proyectos
                .Include(p => p.Cliente)
                .Include(p => p.Tarifa)
                .OrderByDescending(p => p.FechaInicio)
                .ToListAsync();

            var proyectosConSesionAbierta = await _db.Sesiones
                .Where(s => s.FechaFin == null)
                .Select(s => s.ProyectoId)
                .Distinct()
                .ToListAsync();

            var cantidadSesionesPorProyecto = await _db.Sesiones
                .GroupBy(s => s.ProyectoId)
                .Select(g => new { ProyectoId = g.Key, Cantidad = g.Count() })
                .ToDictionaryAsync(g => g.ProyectoId, g => g.Cantidad);

            foreach (var proyecto in proyectos)
            {
                proyecto.TieneSesionAbierta = proyectosConSesionAbierta.Contains(proyecto.Id);
                proyecto.CantidadSesiones = cantidadSesionesPorProyecto.TryGetValue(proyecto.Id, out var cantidad) ? cantidad : 0;
            }

            return Ok(proyectos);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Proyecto>> Get(int id)
        {
            var proyecto = await _db.Proyectos
                .Include(p => p.Cliente)
                .Include(p => p.Tarifa)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (proyecto == null) return NotFound();

            return Ok(proyecto);
        }

        [HttpPost]
        public async Task<ActionResult<Proyecto>> Post(Proyecto proyecto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            if (!await _db.Clientes.AnyAsync(c => c.Id == proyecto.ClienteId))
                return ValidationProblem("El cliente indicado no existe.");

            proyecto.Id = 0;
            proyecto.Cliente = null;
            proyecto.Estado = EstadoProyecto.Presupuesto;
            if (proyecto.Tarifa != null) proyecto.Tarifa.Id = 0;

            _db.Proyectos.Add(proyecto);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = proyecto.Id }, proyecto);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, Proyecto proyecto)
        {
            if (id != proyecto.Id) return BadRequest();
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var existente = await _db.Proyectos.Include(p => p.Tarifa).FirstOrDefaultAsync(p => p.Id == id);
            if (existente == null) return NotFound();

            existente.Nombre = proyecto.Nombre;
            existente.Referencia = proyecto.Referencia;
            existente.ClienteId = proyecto.ClienteId;
            existente.FechaInicio = proyecto.FechaInicio;
            existente.ProductorResponsableId = proyecto.ProductorResponsableId;
            existente.HorasContratadas = proyecto.HorasContratadas;

            if (existente.Tarifa == null)
            {
                proyecto.Tarifa.Id = 0;
                proyecto.Tarifa.ProyectoId = existente.Id;
                existente.Tarifa = proyecto.Tarifa;
            }
            else
            {
                existente.Tarifa.Modalidad = proyecto.Tarifa.Modalidad;
                existente.Tarifa.Valor = proyecto.Tarifa.Valor;
                existente.Tarifa.FechaAcuerdo = proyecto.Tarifa.FechaAcuerdo;
            }

            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id:int}/finalizar")]
        public async Task<IActionResult> Finalizar(int id)
        {
            var proyecto = await _db.Proyectos.FindAsync(id);
            if (proyecto == null) return NotFound();

            proyecto.Estado = EstadoProyecto.Finalizado;
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id:int}/cancelar")]
        public async Task<IActionResult> Cancelar(int id)
        {
            var proyecto = await _db.Proyectos.FindAsync(id);
            if (proyecto == null) return NotFound();

            proyecto.Estado = EstadoProyecto.Cancelado;
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var proyecto = await _db.Proyectos.FindAsync(id);
            if (proyecto == null) return NotFound();

            _db.Proyectos.Remove(proyecto);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
