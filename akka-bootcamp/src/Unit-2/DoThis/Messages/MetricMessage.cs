
namespace ChartApp.Messages
{
    public class MetricMessage
    {
        public string Series { get; set; }
        public float CounterValue { get; set; }

        public MetricMessage(string series, float counterValue)
        {
            this.Series = series;
            this.CounterValue = counterValue;
        }
    }
}
