using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Nexo.Shared.Models
{
    public class Sesion : IValidatableObject
    {
        public int Id { get; set; }

        public int ProyectoId { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; } = DateTime.Now;

        public DateTime? FechaFin { get; set; }

        public decimal? CantidadHoras { get; set; }

        public int ResponsableId { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(500)]
        public string Descripcion { get; set; }

        public int EstudioId { get; set; }

        public Estudio Estudio { get; set; }

        [StringLength(1000)]
        public string Observaciones { get; set; }

        public int? UsuarioQueCargoId { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // Para alta/edición: ids de tareas elegidas del catálogo.
        public List<int> TareaCatalogoIds { get; set; } = new();

        // Para lectura: detalle de tareas ya asociadas.
        public List<SesionTarea> Tareas { get; set; } = new();

        public bool Abierta => FechaFin is null;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EstudioId <= 0)
                yield return new ValidationResult("El estudio/locación es obligatorio.", new[] { nameof(EstudioId) });

            if (ResponsableId <= 0)
                yield return new ValidationResult("El responsable es obligatorio.", new[] { nameof(ResponsableId) });

            if (FechaFin.HasValue && FechaFin.Value < FechaInicio)
                yield return new ValidationResult("La fecha de fin no puede ser anterior a la de inicio.", new[] { nameof(FechaFin) });

            if (FechaFin.HasValue && (!CantidadHoras.HasValue || CantidadHoras.Value <= 0))
                yield return new ValidationResult("La cantidad de horas es obligatoria al cerrar la sesión.", new[] { nameof(CantidadHoras) });
        }
    }
}
