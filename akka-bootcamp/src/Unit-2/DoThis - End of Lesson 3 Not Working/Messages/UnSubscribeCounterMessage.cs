
using Akka.Actor;

namespace ChartApp.Messages
{

    /// <summary>
    /// Unsubscribes <see cref="Subscriber"/> from receiving updates 
    /// for a given counter
    /// </summary>
    public class UnSubscribeCounterMessage
    {
        public CounterType CounterType { get; private set; }
        public IActorRef Subscriber { get; private set; }

        public UnSubscribeCounterMessage(CounterType counterType, IActorRef subscriber)
        {
            this.CounterType = counterType;
            this.Subscriber = subscriber;
        }
    }
}
