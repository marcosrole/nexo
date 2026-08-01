using System.ComponentModel.DataAnnotations;

namespace Nexo.Shared.Models
{
    public class Estudio
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100)]
        public string Nombre { get; set; }

        public bool EsLocacionExterna { get; set; }

        [StringLength(150)]
        public string Direccion { get; set; }
    }
}
