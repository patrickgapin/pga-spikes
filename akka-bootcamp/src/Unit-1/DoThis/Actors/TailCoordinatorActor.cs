
using System;
using Akka.Actor;

namespace WinTail.Actors
{
    public class TailCoordinatorActor : UntypedActor
    {
        public class StartTail
        {
            public string FilePath { get; private set; }
            public IActorRef ReporterActor { get; private set; }

            public StartTail(string filePath, IActorRef reporterActor)
            {
                ReporterActor = reporterActor;
                FilePath = filePath;
            }
        }

        public class StopTail
        {
            public StopTail(string filePath) { FilePath = filePath; }

            public string FilePath { get; private set; }
        }

        protected override void OnReceive(object message)
        {
            if (message is StartTail)
            {
                var msg = message as StartTail;
                var tailActorProps = Props.Create(() => new TailActor(msg.ReporterActor, msg.FilePath));
                Context.ActorOf(tailActorProps, "tailActor");
            }
        }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            var strategy = new OneForOneStrategy(
                10, TimeSpan.FromSeconds(30), exception =>
                {
                    if (exception is ArithmeticException) return Directive.Resume;

                    if (exception is NotSupportedException) return Directive.Stop;

                    return Directive.Restart;
                });

            return strategy;
        }
    }
}