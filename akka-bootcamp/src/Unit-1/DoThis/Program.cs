using System;
﻿using Akka.Actor;
using WinTail.Actors;

namespace WinTail
{
    #region Program
    class Program
    {
        public static ActorSystem MyActorSystem;

        static void Main(string[] args)
        {
            MyActorSystem = ActorSystem.Create("MyActorSystem");

            var consoleWriterProps = Props.Create<ConsoleWriterActor>();
            var consoleWriterActor = MyActorSystem.ActorOf(consoleWriterProps, "consoleWriterActor");

            var tailCoordinatorActorProps = Props.Create(() => new TailCoordinatorActor());
            var tailCoordinatorActor = MyActorSystem.ActorOf(tailCoordinatorActorProps, "tailCoordinatorActor");            

            var fileValidationActorProps = Props.Create(() => new FileValidationActor(consoleWriterActor));
            var validationActor = MyActorSystem.ActorOf(fileValidationActorProps, "validationActor");

            var consoleReaderProps = Props.Create<ConsoleReaderActor>();
            var consoleReaderActor = MyActorSystem.ActorOf(consoleReaderProps, "consoleReaderActor");

            consoleReaderActor.Tell(ConsoleReaderActor.StartCommand);

            // blocks the main thread from exiting until the actor system is shut down
            MyActorSystem.AwaitTermination();
        }
    }
    #endregion
}
