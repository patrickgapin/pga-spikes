using System;
using System.Collections.Generic;
using System.Windows.Forms.DataVisualization.Charting;

namespace ChartApp.Messages
{
    public class InitializeChartMessage
    {
        public InitializeChartMessage(Dictionary<string, Series> initialSeries)
        {
            InitialSeries = initialSeries;
        }

        public Dictionary<string, Series> InitialSeries { get; private set; }
    }
}
