using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;
using Akka.Actor;
using ChartApp.Messages;

namespace ChartApp.Actors
{
    public class ChartingActor : ReceiveActor
    {
        /// <summary>
        /// Maximum number of points we will allow in a series
        /// </summary>
        public const int MaxPoints = 250;

        /// <summary>
        /// Incrementing counter we use to plot along the X-axis
        /// </summary>
        private int xPosCounter = 0;

        private readonly Chart chart;
        private Dictionary<string, Series> seriesIndex;

        public ChartingActor(Chart chart) : this(chart, new Dictionary<string, Series>())
        {
        }

        public ChartingActor(Chart chart, Dictionary<string, Series> seriesIndex)
        {
            this.chart = chart;
            this.seriesIndex = seriesIndex;

            Receive<InitializeChartMessage>(message => HandleInitialize(message));
            Receive<AddSeriesMessage>(message => HandleAddSeriesMessage(message));
            Receive<RemoveSeriesMessage>(message => HandleRemoveSeriesMessage(message));
            Receive<MetricMessage>(message => HandleMetricsMessage(message));
        }

        #region Individual Message Type Handlers

        private void HandleInitialize(InitializeChartMessage ic)
        {
            if (ic.InitialSeries != null)
            {
                //swap the two series out
                seriesIndex = ic.InitialSeries;
            }

            //delete any existing series
            chart.Series.Clear();

            // set axes up
            var area = chart.ChartAreas[0];
            area.AxisX.IntervalType = DateTimeIntervalType.Number;
            area.AxisY.IntervalType = DateTimeIntervalType.Number;

            SetChartBoundaries();

            //attempt to render the initial chart
            if (seriesIndex.Any())
            {
                foreach (var series in seriesIndex)
                {
                    //force both the chart and the internal index to use the same names
                    series.Value.Name = series.Key;
                    chart.Series.Add(series.Value);
                }
            }

            SetChartBoundaries();
        }

        private void HandleAddSeriesMessage(AddSeriesMessage series)
        {
            if (!string.IsNullOrEmpty(series.Series.Name) && !seriesIndex.ContainsKey(series.Series.Name))
            {
                seriesIndex.Add(series.Series.Name, series.Series);
                chart.Series.Add(series.Series);
                SetChartBoundaries();
            }
        }

        private void HandleRemoveSeriesMessage(RemoveSeriesMessage message)
        {
            if (!string.IsNullOrEmpty(message.SeriesName) && seriesIndex.ContainsKey(message.SeriesName))
            {
                var seriesToRemove = seriesIndex[message.SeriesName];
                seriesIndex.Remove(message.SeriesName);
                chart.Series.Remove(seriesToRemove);
                SetChartBoundaries();
            }
        }

        private void HandleMetricsMessage(MetricMessage message)
        {
            if (!string.IsNullOrEmpty(message.Series) && seriesIndex.ContainsKey(message.Series))
            {
                var series = seriesIndex[message.Series];
                series.Points.AddXY(xPosCounter++, message.CounterValue);

                while (series.Points.Count > MaxPoints) series.Points.RemoveAt(0);

                SetChartBoundaries();
            }
        }

        #endregion

        private void SetChartBoundaries()
        {
            //double minAxisY = 0.seriesIndex;
            var allPoints = seriesIndex.Values.SelectMany(series => series.Points).ToList();
            var yValues = allPoints.SelectMany(point => point.YValues).ToList();
            double maxAxisX = xPosCounter;
            double minAxisX = xPosCounter - MaxPoints;
            double maxAxisY = yValues.Count > 0 ? Math.Ceiling(yValues.Max()) : 1.0d;
            double minAxisY = yValues.Count > 0 ? Math.Floor(yValues.Max()) : 0.0d;

            if (allPoints.Count > 2)
            {
                var area = chart.ChartAreas[0];
                area.AxisX.Minimum = minAxisX;
                area.AxisX.Maximum = maxAxisX;
                area.AxisX.Minimum = minAxisY;
                area.AxisX.Maximum = maxAxisY;
            }
        }
        #region Messages

        public class InitializeChartMessage
        {
            public InitializeChartMessage(Dictionary<string, Series> initialSeries)
            {
                InitialSeries = initialSeries;
            }

            public Dictionary<string, Series> InitialSeries { get; private set; }
        }

        public class AddSeriesMessage
        {
            public Series Series { get; private set; }

            public AddSeriesMessage(Series series)
            {
                this.Series = series;
            }
        }

        public class RemoveSeriesMessage
        {
            public RemoveSeriesMessage(string seriesName)
            {
                SeriesName = seriesName;
            }

            public string SeriesName { get; private set; }
        }

        public class MetricMessage
        {
            public string Series { get; private set; }
            public float CounterValue { get; private set; }

            public MetricMessage(string series, float counterValue)
            {
                this.Series = series;
                this.CounterValue = counterValue;
            }
        }

        #endregion
    }
}
