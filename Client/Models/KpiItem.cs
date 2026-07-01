namespace Nexo.Client.Models
{
    /// <summary>A single KPI card shown on the Home dashboard summary row.</summary>
    public record KpiItem(
        string Icon,
        string Value,
        string Label,
        string TrendLabel,
        bool IsPositiveTrend);
}
