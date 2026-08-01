namespace Nexo.Shared.Models
{
    public class SesionTarea
    {
        public int Id { get; set; }

        public int SesionId { get; set; }

        public int TareaCatalogoId { get; set; }

        public TareaCatalogo TareaCatalogo { get; set; }
    }
}
