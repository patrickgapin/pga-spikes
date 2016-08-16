using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Akka.Actor;
using Akka.Util.Internal;
using ChartApp.Actors;
using ChartApp.Messages;

namespace ChartApp
{
    public partial class Main : Form
    {
        private IActorRef chartActor;
        private readonly AtomicCounter seriesCounter = new AtomicCounter(1);
        private IActorRef coordinatorActor;
        private Dictionary<CounterType, IActorRef> toggleActors = new Dictionary<CounterType, IActorRef>();

        public Main()
        {
            InitializeComponent();
        }

        #region Initialization

        private void Main_Load(object sender, EventArgs e)
        {
            chartActor = Program.ChartActors.ActorOf(Props.Create(() => new ChartingActor(sysChart)), "charting");
            chartActor.Tell(new ChartingActor.InitializeChartMessage(null)); //no initial series

            coordinatorActor = Program.ChartActors.ActorOf(Props.Create(() => new PerformanceCounterCoordinatorActor(chartActor)), "counters");

            // CPU button toggle actor
            toggleActors[CounterType.Cpu] = Program.ChartActors.ActorOf(
                Props.Create(() => new ButtonToggleActor(coordinatorActor, buttonCpu, CounterType.Cpu, false))
                .WithDispatcher("akka.actor.synchronized-dispatcher"));

            // MEMORY button toggle actor
            toggleActors[CounterType.Memory] = Program.ChartActors.ActorOf(
               Props.Create(() => new ButtonToggleActor(coordinatorActor, buttonMemory, CounterType.Memory, false))
                .WithDispatcher("akka.actor.synchronized-dispatcher"));

            // DISK button toggle actor
            toggleActors[CounterType.Disk] = Program.ChartActors.ActorOf(
               Props.Create(() => new ButtonToggleActor(coordinatorActor, buttonDisk, CounterType.Disk, false))
               .WithDispatcher("akka.actor.synchronized-dispatcher"));

            // Set the CPU toggle to ON so we start getting some data
            toggleActors[CounterType.Cpu].Tell(new ButtonToggleActor.ToggleMessage());
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            //shut down the charting actor
            chartActor.Tell(PoisonPill.Instance);

            //shut down the ActorSystem
            Program.ChartActors.Shutdown();
        }

        #endregion

        private void buttonCpu_Click(object sender, EventArgs e)
        {
            toggleActors[CounterType.Cpu].Tell(new ButtonToggleActor.ToggleMessage());
        }

        private void buttonMemory_Click(object sender, EventArgs e)
        {
            toggleActors[CounterType.Memory].Tell(new ButtonToggleActor.ToggleMessage());
        }

        private void buttonDisk_Click(object sender, EventArgs e)
        {
            toggleActors[CounterType.Disk].Tell(new ButtonToggleActor.ToggleMessage());
        }
    }
}
