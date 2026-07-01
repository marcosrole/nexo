namespace Nexo.Client.Models
{
    /// <summary>One stage of the horizontal production timeline (Idea → Lanzamiento).</summary>
    public record FlowStage(int Order, string Name, string Icon);
}
