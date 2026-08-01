using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexo.Server.Data;
using Nexo.Shared.Models;

namespace Nexo.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SesionesController : ControllerBase
    {
        private readonly NexoDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public SesionesController(NexoDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Sesion>>> GetByProyecto([FromQuery] int proyectoId)
        {
            var sesiones = await _db.Sesiones
                .Include(s => s.Estudio)
                .Include(s => s.Tareas).ThenInclude(t => t.TareaCatalogo)
                .Where(s => s.ProyectoId == proyectoId)
                .OrderByDescending(s => s.FechaCreacion)
                .ToListAsync();

            return Ok(sesiones);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Sesion>> Get(int id)
        {
            var sesion = await _db.Sesiones
                .Include(s => s.Estudio)
                .Include(s => s.Tareas).ThenInclude(t => t.TareaCatalogo)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sesion == null) return NotFound();

            return Ok(sesion);
        }

        [HttpGet("resumen/{proyectoId:int}")]
        public async Task<ActionResult<ResumenHorasProyecto>> Resumen(int proyectoId)
        {
            var proyecto = await _db.Proyectos.FindAsync(proyectoId);
            if (proyecto == null) return NotFound();

            var horasUsadas = await _db.Sesiones
                .Where(s => s.ProyectoId == proyectoId && s.CantidadHoras != null)
                .SumAsync(s => s.CantidadHoras.Value);

            return Ok(new ResumenHorasProyecto
            {
                HorasContratadas = proyecto.HorasContratadas,
                HorasUsadas = horasUsadas,
                HorasDisponibles = proyecto.HorasContratadas.HasValue
                    ? proyecto.HorasContratadas.Value - horasUsadas
                    : null
            });
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Sesion>> Post(Sesion sesion)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var proyecto = await _db.Proyectos.FirstOrDefaultAsync(p => p.Id == sesion.ProyectoId);
            if (proyecto == null)
                return ValidationProblem("El proyecto indicado no existe.");

            var usuarioActual = await _userManager.GetUserAsync(User);

            sesion.Id = 0;
            sesion.Estudio = null;
            sesion.UsuarioQueCargoId = usuarioActual?.Id;

            var tareaIds = sesion.TareaCatalogoIds ?? new List<int>();
            sesion.Tareas = tareaIds.Select(tid => new SesionTarea { TareaCatalogoId = tid }).ToList();

            if (proyecto.Estado == EstadoProyecto.Presupuesto)
                proyecto.Estado = EstadoProyecto.EnCurso;

            _db.Sesiones.Add(sesion);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = sesion.Id }, sesion);
        }

        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Put(int id, Sesion sesion)
        {
            if (id != sesion.Id) return BadRequest();
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var existente = await _db.Sesiones
                .Include(s => s.Tareas)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (existente == null) return NotFound();

            existente.FechaInicio = sesion.FechaInicio;
            existente.FechaFin = sesion.FechaFin;
            existente.CantidadHoras = sesion.CantidadHoras;
            existente.ResponsableId = sesion.ResponsableId;
            existente.Descripcion = sesion.Descripcion;
            existente.EstudioId = sesion.EstudioId;
            existente.Observaciones = sesion.Observaciones;

            var tareaIdsNuevas = sesion.TareaCatalogoIds ?? new List<int>();
            _db.SesionTareas.RemoveRange(existente.Tareas);
            existente.Tareas = tareaIdsNuevas.Select(tid => new SesionTarea { TareaCatalogoId = tid }).ToList();

            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var sesion = await _db.Sesiones.FindAsync(id);
            if (sesion == null) return NotFound();

            _db.Sesiones.Remove(sesion);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
