
using Akka.Actor;
using WinTail.Messages;

namespace WinTail.Actors
{
    public class ValidationActor : UntypedActor
    {
        private readonly IActorRef consoleWriterActor;

        public ValidationActor(IActorRef consoleWriterActor) { this.consoleWriterActor = consoleWriterActor; }

        protected override void OnReceive(object message)
        {
            var msg = message as string;
            if (string.IsNullOrEmpty((msg))) { consoleWriterActor.Tell(new NullInputErrorMessage("No input received.")); }
            else if (IsValid(msg)) { consoleWriterActor.Tell(new InputSuccessMessage("Thank you! Message was valid.")); }
            else { consoleWriterActor.Tell(new ValidationErrorMessage("Invalid: input had off numnber of characters.")); }

            Sender.Tell(new ContinueProcessingMessage());
        }

        private static bool IsValid(string message)
        {
            return message.Length % 2 == 0;
        }
    }
}
