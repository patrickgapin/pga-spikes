using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using WinTail.Actors;

namespace WinTail.Helpers
{
    public class FileObserver
    {
        private readonly IActorRef tailActor;
        private readonly string absoluteFilePath;
        private FileSystemWatcher watcher;
        private readonly string fileDir;
        private readonly string fileNameOnly;

        public FileObserver(IActorRef tailActor, string absoluteFilePath)
        {
            this.tailActor = tailActor;
            this.absoluteFilePath = absoluteFilePath;
            fileDir = Path.GetDirectoryName(absoluteFilePath);
            fileNameOnly = Path.GetFileName(absoluteFilePath);
        }

        public void Start()
        {
            watcher = new FileSystemWatcher(fileDir, fileNameOnly)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            };

            watcher.Changed += OnFileChanged;
            watcher.Error += OnFileError;
        }

        private void OnFileError(object sender, ErrorEventArgs e)
        {
            tailActor.Tell(new TailActor.FileError(fileNameOnly, e.GetException().Message), ActorRefs.NoSender);
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                tailActor.Tell(new TailActor.FileWrite(e.Name), ActorRefs.NoSender);
            }
        }

        public void Dispose() { watcher.Dispose(); }
    }
}
