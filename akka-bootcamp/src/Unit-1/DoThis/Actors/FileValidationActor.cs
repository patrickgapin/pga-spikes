
using Akka.Actor;
using WinTail.Messages;
using System.IO;
using WinTail.Helpers;

namespace WinTail.Actors
{
    public class FileValidationActor : UntypedActor
    {
        private readonly IActorRef consoleWriterActor;

        public FileValidationActor(IActorRef consoleWriterActor)
        {
            this.consoleWriterActor = consoleWriterActor;
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

                Context.ActorSelection(Constants.FilePaths.TailCoordinatorActorPath).Tell(
                    new TailCoordinatorActor.StartTail(msg, consoleWriterActor));

                return;
            }
            else { consoleWriterActor.Tell(new ValidationErrorMessage($"{msg} is not an existing URI on disk.")); }

            Sender.Tell(new ContinueProcessingMessage());
        }

        private static bool IsFileUri(string path)
        {
            return File.Exists(path);
        }
    }
}
