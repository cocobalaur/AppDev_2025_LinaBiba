using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
        /// Sets and displays the pie chart using grouped expense data.
        /// </summary>
        /// <param name="groupedData">A list of dictionaries where each dictionary represents a month's data with category keys.</param>
        /// <param name="allCategories">All available category names, used to ensure $0 categories are still shown.</param>
        public void SetData(List<Dictionary<string, object>> groupedData, List<string> allCategories)
        {
            if (groupedData == null || groupedData.Count == 0)
            {
                chPie.Series.Clear();
                return;
            }

            // Get the last month by default
            var latestMonth = groupedData.LastOrDefault();
            if (latestMonth == null || !latestMonth.ContainsKey("Month")) return;

            string selectedMonth = latestMonth["Month"].ToString();

            var pieData = new List<KeyValuePair<string, double>>();

            foreach (var pair in latestMonth)
            {
                if (pair.Key == "Month") continue;

                string category = pair.Key;
                double value = 0;
                double.TryParse(pair.Value?.ToString(), out value);

                // Always show the category, even if zero (for chart balance)
                if (allCategories.Contains(category))
                {
                    pieData.Add(new KeyValuePair<string, double>(category, Math.Abs(value)));
                }
            }

            chPie.Series.Clear();

            var pieSeries = new LabeledPieSeries
            {
                IndependentValueBinding = new System.Windows.Data.Binding("Key"),
                DependentValueBinding = new System.Windows.Data.Binding("Value"),
                ItemsSource = pieData,
                Title = $"Expenses for {selectedMonth}"
            };

            chPie.Series.Add(pieSeries);
        }
    }
}
