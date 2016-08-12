
using System.IO;
using System.Text;
using Akka.Actor;
using WinTail.Helpers;

namespace WinTail.Actors
{
    public class TailActor : UntypedActor
    {
        private readonly string filePath;
        private readonly IActorRef reporterActor;
        private readonly FileObserver observer;
        private readonly Stream fileStream;
        private readonly StreamReader fileStreamReader;

        #region Message Types

        public class FileWrite
        {
            public string FileName { get; private set; }

            public FileWrite(string fileName) { FileName = fileName; }
        }

        public class FileError
        {
            public string FileName { get; private set; }
            public string Reason { get; private set; }

            public FileError(string fileName, string reason) { FileName = fileName; Reason = reason; }
        }

        public class InitialRead
        {
            public string FileName { get; private set; }
            public string Text { get; private set; }

            public InitialRead(string fileName, string text) { FileName = fileName; Text = text; }
        }

        #endregion

        public TailActor(IActorRef reporterActor, string filePath)
        {
            this.reporterActor = reporterActor;
            this.filePath = filePath;

            observer = new FileObserver(Self, filePath);
            observer.Start();

            fileStream = new FileStream(Path.GetFullPath(filePath), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            fileStreamReader = new StreamReader(fileStream, Encoding.UTF8);

            var text = fileStreamReader.ReadToEnd();
            Self.Tell(new InitialRead(filePath, text));
        }

        protected override void OnReceive(object message)
        {
            if (message is FileWrite)
            {
                var text = fileStreamReader.ReadToEnd();
                if (!string.IsNullOrEmpty(text)) { reporterActor.Tell(text); }

            }
            else if (message is FileError) { reporterActor.Tell($"Tail error: {(message as FileError).Reason}"); }
            else if (message is InitialRead){ reporterActor.Tell((message as InitialRead).Text);}
        }
    }
}
