
namespace ChartApp.Messages
{
    /// <summary>
    /// Remove an existing <see cref="Series"/> from the chart
    /// </summary>
    public class RemoveSeriesMessage
    {
        public RemoveSeriesMessage(string seriesName)
        {
            SeriesName = seriesName;
        }

        public string SeriesName { get; private set; }
    }
}
