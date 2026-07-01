namespace Nexo.Client.Models
{
    /// <summary>One module tile in the "Todo conectado" grid (Proyectos, Artistas, etc.).</summary>
    public record ModuleItem(
        string Icon,
        string Title,
        string Description,
        string Href,
        string AccentFrom,
        string AccentTo);
}
