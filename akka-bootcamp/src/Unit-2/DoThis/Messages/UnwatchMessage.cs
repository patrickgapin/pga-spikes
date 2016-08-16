
namespace ChartApp.Messages
{
    public class UnwatchMessage
    {
        public CounterType CounterType { get; private set; }

        public UnwatchMessage(CounterType counterType) { this.CounterType = counterType; }
    }
}
