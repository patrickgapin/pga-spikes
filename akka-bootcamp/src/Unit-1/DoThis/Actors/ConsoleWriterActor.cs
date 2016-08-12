using System;
using Akka.Actor;
using WinTail.Messages;

namespace WinTail.Actors
{
    /// <summary>
    /// Actor responsible for serializing message writes to the console.
    /// (write one message at a time, champ :)
    /// </summary>
    class ConsoleWriterActor : UntypedActor
    {
        protected override void OnReceive(object message)
        {
            if (message is InputErrorMessage)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine((message as InputErrorMessage).Reason);
            }
            else if (message is InputSuccessMessage)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine((message as InputSuccessMessage).Reason);
            }
            else { Console.WriteLine(message); }

            Console.ResetColor();
        }
    }
}
