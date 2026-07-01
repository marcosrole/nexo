namespace Nexo.Client.Models
{
    /// <summary>
    /// A floating card in the hero's connected-ecosystem illustration, positioned as a
    /// percentage of its container so the SVG connector lines stay aligned at any size.
    /// </summary>
    public record ConnectedNode(
        string Label,
        string Icon,
        double Top,
        double Left,
        double AnimationDelaySeconds);
}
