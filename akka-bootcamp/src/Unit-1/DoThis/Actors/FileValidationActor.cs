
using Akka.Actor;
using WinTail.Messages;
using System.IO;

namespace WinTail.Actors
{
    public class FileValidationActor : UntypedActor
    {
        private readonly IActorRef consoleWriterActor;
        private readonly IActorRef tailCoordinatorActor;

        public FileValidationActor(IActorRef consoleWriterActor, IActorRef tailCoordinatorActor)
        {
            this.consoleWriterActor = consoleWriterActor;
            this.tailCoordinatorActor = tailCoordinatorActor;
        }

        protected override void OnReceive(object message)
        {
            var msg = message as string;
            if (string.IsNullOrEmpty((msg)))
            {
                consoleWriterActor.Tell(new NullInputErrorMessage("Input was blank. Please try again.\n"));                
            }
            else if (IsFileUri(msg))
            {
                consoleWriterActor.Tell(
                    new InputSuccessMessage($"Starting processing for {msg}"));

                tailCoordinatorActor.Tell(new TailCoordinatorActor.StartTail(msg, consoleWriterActor));
                return;
            }
            else
            {
                consoleWriterActor.Tell(new ValidationErrorMessage($"{msg} is not an existing URI on disk."));                
            }


            Sender.Tell(new ContinueProcessingMessage());
        }

        private static bool IsFileUri(string path)
        {
            return File.Exists(path);
        }
    }
}
