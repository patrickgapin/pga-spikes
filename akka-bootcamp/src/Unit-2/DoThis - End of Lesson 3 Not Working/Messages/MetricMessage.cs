
namespace ChartApp.Messages
{
    /// <summary>
    /// Metric data at the time of sample
    /// </summary>
    public class MetricMessage
    {
        public string Series { get; private set; }
        public float CounterValue { get; private set; }

        public MetricMessage(string series, float counterValue)
        {
            this.Series = series;
            this.CounterValue = counterValue;
        }
    }
}
