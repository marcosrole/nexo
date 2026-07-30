using System;
using System.ComponentModel.DataAnnotations;

namespace Nexo.Shared.Models
{
    public class Tarifa
    {
        public int Id { get; set; }

        public int ProyectoId { get; set; }

        [Required]
        public ModalidadTarifa Modalidad { get; set; }

        [Required(ErrorMessage = "El valor de la tarifa es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El valor debe ser mayor a cero.")]
        public decimal Valor { get; set; }

        [Required(ErrorMessage = "La fecha de acuerdo es obligatoria.")]
        public DateTime FechaAcuerdo { get; set; } = DateTime.UtcNow.Date;

        public DateTime VigenteHasta => FechaAcuerdo.AddMonths(3);

        public bool Vencida => DateTime.UtcNow.Date > VigenteHasta;
    }
}
