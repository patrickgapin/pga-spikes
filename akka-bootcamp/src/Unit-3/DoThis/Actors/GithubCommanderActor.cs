using System;
using System.Linq;
using System.Windows.Forms;
using Akka.Actor;
using Akka.Routing;

namespace GithubActors.Actors
{
    /// <summary>
    /// Top-level actor responsible for coordinating and launching repo-processing jobs
    /// </summary>
    public class GithubCommanderActor : ReceiveActor, IWithUnboundedStash
    {
        #region Message classes

        public class CanAcceptJob
        {
            public CanAcceptJob(RepoKey repo)
            {
                Repo = repo;
            }

            public RepoKey Repo { get; private set; }
        }

        public class AbleToAcceptJob
        {
            public AbleToAcceptJob(RepoKey repo)
            {
                Repo = repo;
            }

            public RepoKey Repo { get; private set; }
        }

        public class UnableToAcceptJob
        {
            public UnableToAcceptJob(RepoKey repo)
            {
                Repo = repo;
            }

            public RepoKey Repo { get; private set; }
        }

        #endregion

        private IActorRef _coordinator;
        private IActorRef _canAcceptJobSender;
        private int pendingJobReplies;
        private RepoKey repoJob;

        public GithubCommanderActor() { Ready(); }

        private void Ready()
        {
            Receive<CanAcceptJob>(job =>
            {
                _coordinator.Tell(job);
                repoJob = job.Repo;
                BecomeAsking();
            });
        }

        private void BecomeAsking()
        {
            _canAcceptJobSender = Sender;
            pendingJobReplies = _coordinator.Ask<Routees>(new GetRoutees())
                .Result.Members.Count(); // 3; // number of routees
            Become(Asking);

            // send ourselves a ReceiveTimeout message if no message within 3 seconds
            Context.SetReceiveTimeout(TimeSpan.FromSeconds(3));
        }

        private void Asking()
        {
            Receive<ReceiveTimeout>(timeout =>
            {
                _canAcceptJobSender.Tell(new UnableToAcceptJob(repoJob));
                BecomeReady();
            });

            Receive<CanAcceptJob>(job => Stash.Stash());

            Receive<UnableToAcceptJob>(job =>
            {
                pendingJobReplies--;
                if (pendingJobReplies == 0)
                {
                    _canAcceptJobSender.Tell(job);
                    BecomeReady();
                }
            });

            Receive<AbleToAcceptJob>(job =>
            {
                _canAcceptJobSender.Tell(job);

                // processes messages
                Sender.Tell(new GithubCoordinatorActor.BeginJob(job.Repo));

                // launch new window to view results of the processing
                Context.ActorSelection(ActorPaths.MainFormActor.Path).Tell(
                    new MainFormActor.LaunchRepoResultsWindow(job.Repo, Sender));

                BecomeReady();
            });
        }

        private void BecomeReady()
        {
            Become(Ready);
            Stash.UnstashAll();

            //cancel ReceiveTimeout
            Context.SetReceiveTimeout(null);
        }

        protected override void PreStart()
        {
            /*
            // create three GithubCoordinatorActor instances
            var c1 = Context.ActorOf(Props.Create(() => new GithubCoordinatorActor()), ActorPaths.GithubCoordinatorActor.Name + "1");
            var c2 = Context.ActorOf(Props.Create(() => new GithubCoordinatorActor()), ActorPaths.GithubCoordinatorActor.Name + "2");
            var c3 = Context.ActorOf(Props.Create(() => new GithubCoordinatorActor()), ActorPaths.GithubCoordinatorActor.Name + "3");

            // create a broadcast router who will ask all of them 
            // if they're available for work
            _coordinator =
                Context.ActorOf(Props.Empty.WithRouter(
                    new BroadcastGroup(ActorPaths.GithubCoordinatorActor.Path + "1",
                    ActorPaths.GithubCoordinatorActor.Path + "2",
                    ActorPaths.GithubCoordinatorActor.Path + "3")));
            */

            _coordinator = Context.ActorOf(Props.Create(() => new GithubCoordinatorActor())
                .WithRouter(FromConfig.Instance),
                ActorPaths.GithubCoordinatorActor.Name);
            base.PreStart();
        }

        protected override void PreRestart(Exception reason, object message)
        {
            //kill off the old coordinator so we can recreate it from scratch
            _coordinator.Tell(PoisonPill.Instance);
            base.PreRestart(reason, message);
        }

        public IStash Stash { get; set; }
    }
}
