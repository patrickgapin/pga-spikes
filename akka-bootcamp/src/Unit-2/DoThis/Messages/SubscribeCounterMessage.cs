
using Akka.Actor;

namespace ChartApp.Messages
{
    public class SubscribeCounterMessage
    {
        public CounterType CounterType { get; private set; }
        public IActorRef Subscriber { get; private set; }

        public SubscribeCounterMessage(CounterType counterType, IActorRef subscriber)
        {
            this.CounterType = counterType;
            this.Subscriber = subscriber;
        }
    }
}
