using System.ComponentModel.DataAnnotations;

namespace Nexo.Shared.Models
{
    public class TareaCatalogo
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required]
        public TipoTrabajo TipoTrabajo { get; set; }

        public bool Activo { get; set; } = true;
    }
}
