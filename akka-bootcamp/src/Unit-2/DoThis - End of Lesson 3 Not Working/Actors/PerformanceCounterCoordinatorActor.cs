
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;
using Akka.Actor;
using ChartApp.Messages;
namespace ChartApp.Actors
{
    public class PerformanceCounterCoordinatorActor : ReceiveActor
    {
        private Dictionary<CounterType, IActorRef> counterActors;
        private IActorRef chartingActor;

        /// <summary>
        /// Methods for generating new instances of all <see cref="PerformanceCounter"/>s
        /// we want to monitor
        /// </summary>
        private static readonly Dictionary<CounterType, Func<PerformanceCounter>> CounterGenerators =
            new Dictionary<CounterType, Func<PerformanceCounter>>()
            {
                {CounterType.Cpu, () => new PerformanceCounter("Processor", "% Processor Time", "_Total", true)},
                {CounterType.Memory, () => new PerformanceCounter("Memory", "% Committed Bytes in Use", true)},
                {CounterType.Disk, () => new PerformanceCounter("LogicalDisk", "% Disk Time", "_Total", true)}
            };

        /// <summary>
        /// Methods for creating new <see cref="Series"/> with distinct colors and names
        /// corresponding to each <see cref="PerformanceCounter"/>
        /// </summary>
        private static readonly Dictionary<CounterType, Func<Series>> CounterSeries = new Dictionary<CounterType, Func<Series>>()
            {
                {CounterType.Cpu, () => new Series(CounterType.Cpu.ToString()) {ChartType = SeriesChartType.SplineArea,Color = Color.DarkGreen }},
                {CounterType.Memory, () => new Series(CounterType.Memory.ToString()){ChartType = SeriesChartType.FastLine,Color = Color.MediumBlue }},
                {CounterType.Memory, () => new Series(CounterType.Disk.ToString()){ChartType = SeriesChartType.Spline,Color = Color.DarkRed}}
            };

        public PerformanceCounterCoordinatorActor(IActorRef chartingActor) : this(chartingActor, new Dictionary<CounterType, IActorRef>())
        { }

        public PerformanceCounterCoordinatorActor(IActorRef chartingActor, Dictionary<CounterType, IActorRef> counterActors)
        {
            this.chartingActor = chartingActor;
            this.counterActors = counterActors;

            Receive<WatchMessage>(watchMessage =>
            {
                if (!counterActors.ContainsKey(watchMessage.CounterType))
                {
                    // create a child actor to monitor this counter if cone doesn't exist already
                    var counterActor = Context.ActorOf(Props.Create(
                                () => new PerformanceCounterActor(watchMessage.CounterType.ToString(), CounterGenerators[watchMessage.CounterType])));

                    counterActors[watchMessage.CounterType] = counterActor;
                }

                // register this series with the ChartingActor
                chartingActor.Tell(new ChartingActor.AddSeriesMessage(CounterSeries[watchMessage.CounterType]()));

                // tell the counter actor to begin publishing its statistics to the _chartingActor
                counterActors[watchMessage.CounterType].Tell(new PerformanceCounterActor.SubscribeCounterMessage(watchMessage.CounterType, chartingActor));
            });

            Receive<UnwatchMessage>(unwatchMessage =>
            {
                if (!counterActors.ContainsKey(unwatchMessage.CounterType)) { return; }

                counterActors[unwatchMessage.CounterType].Tell(new PerformanceCounterActor.UnSubscribeCounterMessage(unwatchMessage.CounterType, chartingActor));

                chartingActor.Tell(new ChartingActor.RemoveSeriesMessage(unwatchMessage.CounterType.ToString()));
            });

        }



        #region Messages

        public class UnwatchMessage
        {
            public CounterType CounterType { get; private set; }

            public UnwatchMessage(CounterType counterType) { this.CounterType = counterType; }
        }

        public class WatchMessage
        {
            public CounterType CounterType { get; private set; }

            public WatchMessage(CounterType counterType)
            {
                this.CounterType = counterType;
            }
        }
        #endregion
    }
}
