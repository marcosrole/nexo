using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Nexo.Shared.Models
{
    public class Proyecto : IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150)]
        public string Nombre { get; set; }

        [StringLength(60)]
        public string Referencia { get; set; }

        public int ClienteId { get; set; }

        public Cliente Cliente { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; } = DateTime.UtcNow.Date;

        [Required]
        public EstadoProyecto Estado { get; set; } = EstadoProyecto.Presupuesto;

        public int ProductorResponsableId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Las horas contratadas deben ser mayores a cero.")]
        public decimal? HorasContratadas { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public bool TieneSesionAbierta { get; set; }

        [Required]
        public Tarifa Tarifa { get; set; } = new Tarifa();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ClienteId <= 0)
                yield return new ValidationResult("El cliente es obligatorio.", new[] { nameof(ClienteId) });

            if (ProductorResponsableId <= 0)
                yield return new ValidationResult("El productor responsable es obligatorio.", new[] { nameof(ProductorResponsableId) });

            if (Tarifa is null)
            {
                yield return new ValidationResult("La tarifa es obligatoria.", new[] { nameof(Tarifa) });
                yield break;
            }

            if (Tarifa.Valor <= 0)
                yield return new ValidationResult("El valor de la tarifa debe ser mayor a cero.", new[] { nameof(Tarifa) });

            if (Tarifa.FechaAcuerdo == default)
                yield return new ValidationResult("La fecha de acuerdo de la tarifa es obligatoria.", new[] { nameof(Tarifa) });
        }
    }
}
