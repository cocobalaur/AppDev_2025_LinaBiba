using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;

namespace BudgetModel
{
    /// <summary> 
    /// Interaction logic for ChartView.xaml
    /// </summary>
    public partial class ChartView : UserControl
    {
        public ChartView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Sets the chart data source using summary data and category list.
        /// Groups values by month and category, displaying them in a pie chart.
        /// </summary>
        public void SetData(List<object> summaryData, List<string> categories)
        {
            if (summaryData == null || summaryData.Count == 0 || categories == null || categories.Count == 0)
            {
                MessageBox.Show("NO DATA");
                return;
            }

            var pieData = new List<KeyValuePair<string, double>>();

            foreach (var rowObj in summaryData)
            {
                if (rowObj is Dictionary<string, object> row && row.ContainsKey("Month"))
                {
                    string month = row["Month"]?.ToString();
                    if (month == "TOTALS") continue;

                    foreach (var cat in categories)
                    {
                        if (row.TryGetValue(cat, out object valObj) &&
                            double.TryParse(valObj?.ToString(), out double value) &&
                            value != 0)
                        {
                            pieData.Add(new KeyValuePair<string, double>($"{month} - {cat}", Math.Abs(value)));
                        }
                    }
                }
            }

            // Bind data to the PieSeries inside the Chart
            if (chPie.Series.Count > 0 && chPie.Series[0] is PieSeries pieSeries)
            {
                pieSeries.ItemsSource = pieData;
                this.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Makes the current control or window visible.
        /// Typically used to display UI elements like the chart or a panel.
        /// </summary>
        public void Show()
        {
            this.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Hides the current control or window by collapsing it from the layout.
        /// Typically used to hide UI elements like the chart or a panel when not needed.
        /// </summary>
        public void Hide()
        {
            this.Visibility = Visibility.Collapsed;
        }


    }
}
