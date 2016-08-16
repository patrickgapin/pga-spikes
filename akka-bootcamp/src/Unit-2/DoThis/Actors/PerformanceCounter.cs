using System;
using System.Collections.Generic;
using System.Diagnostics;
using Akka.Actor;
using ChartApp.Messages;

namespace ChartApp.Actors
{
    public class PerformanceCounterActor : UntypedActor
    {
        private readonly string seriesName;
        private readonly Func<PerformanceCounter> performanceCounterGenerator;
        private PerformanceCounter performanceCounter;
        private readonly HashSet<IActorRef> subscriptions;
        private readonly ICancelable cancelPublishing;

        public PerformanceCounterActor(string seriesName, Func<PerformanceCounter> performanceCounterGenerator)
        {
            this.seriesName = seriesName;
            this.performanceCounterGenerator = performanceCounterGenerator;
            subscriptions = new HashSet<IActorRef>();
            cancelPublishing = new Cancelable(Context.System.Scheduler);
        }

        #region Actor Lifecyle methods

        protected override void PreStart()
        {
            performanceCounter = performanceCounterGenerator();
            Context.System.Scheduler.ScheduleTellRepeatedly(
                TimeSpan.FromMilliseconds(250), TimeSpan.FromMilliseconds(250),
                Self, new Messages.GatherMetricsMessage(), Self, cancelPublishing
                );
        }

        protected override void PostStop()
        {
            try
            {
                cancelPublishing.Cancel(false);
                performanceCounter.Dispose();
            }
            catch (Exception) {/*dont care about additional ObjectDisposed Exceptions*/}
            finally { base.PostStop(); }
        }

        #endregion

        protected override void OnReceive(object message)
        {
            if (message is Messages.GatherMetricsMessage)
            {
                var metric = new ChartingActor.Messages.MetricMessage(seriesName, performanceCounter.NextValue());
                foreach (var subscription in subscriptions) { subscription.Tell(metric); }
            }
            else if (message is Messages.SubscribeCounterMessage) { subscriptions.Add((message as Messages.SubscribeCounterMessage).Subscriber); }
            else if (message is Messages.UnSubscribeCounterMessage) { subscriptions.Remove((message as Messages.UnSubscribeCounterMessage).Subscriber); }
        }

        #region Messages

        public class Messages
        {
            public class GatherMetricsMessage
            {
            }

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

        #endregion
    }
}
