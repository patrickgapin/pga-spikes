
namespace ChartApp.Messages
{
    public class WatchMessage
    {
        public CounterType CounterType { get; private set; }

        public WatchMessage(CounterType counterType)
        {
            this.CounterType = counterType;
        }
    }

}
