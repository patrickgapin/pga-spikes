using System;
using Akka.Actor;
using WinTail.Helpers;
using WinTail.Messages;

namespace WinTail.Actors
{
    /// <summary>
    /// Actor responsible for reading FROM the console. 
    /// Also responsible for calling <see cref="ActorSystem.Shutdown"/>.
    /// </summary>
    class ConsoleReaderActor : UntypedActor
    {
        public const string ExitCommand = "exit";
        public const string StartCommand = "start";

        protected override void OnReceive(object message)
        {
            if (message.Equals(StartCommand)) { DoPrintInstructions(); }

            GetAndValidateInput();
        }

        #region Internal Methods

        private void DoPrintInstructions()
        {
            Console.WriteLine("Please provide the URI of a log file on disk.\n");
        }

        private void GetAndValidateInput()
        {
            var message = Console.ReadLine();
            if (!string.IsNullOrEmpty(message) &&
                string.Equals(message, ExitCommand, StringComparison.OrdinalIgnoreCase))
            {
                Context.System.Shutdown();
                return;
            }

            //validationActor.Tell(message);
            Context.ActorSelection(Constants.FilePaths.ValidationActorPath).Tell(message);
        }
        #endregion

    }
}