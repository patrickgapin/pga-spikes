
using System.Windows.Forms.DataVisualization.Charting;

namespace ChartApp.Messages
{
    public class AddSeriesMessage
    {
        public Series Series { get; private set; }

        public AddSeriesMessage(Series series)
        {
            this.Series = series;
        }
    }
}
