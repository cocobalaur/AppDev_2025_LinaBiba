using Budget;
using System.Text;
using System.Windows;
using System.Windows.Controls;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Load default Light theme on startup
            var lightTheme = new ResourceDictionary
            {
                Source = new Uri("Themes/LightTheme.xaml", UriKind.Relative)
            };
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(lightTheme);

            // Apply light gradient on startup
            SetWindowGradient("Light", false);
        }

        private void ApplyTheme_Click(object sender, RoutedEventArgs e)
        {
            string baseTheme = ((ComboBoxItem)ColorComboBox.SelectedItem)?.Content?.ToString();
            bool isDark = DarkModeCheckBox.IsChecked == true;

            string themeFile;

            if (baseTheme == "Blush")
                themeFile = isDark ? "Themes/DarkBlushTheme.xaml" : "Themes/BlushTheme.xaml";
            else if (baseTheme == "Ocean")
                themeFile = isDark ? "Themes/DarkOceanTheme.xaml" : "Themes/OceanTheme.xaml";
            else if (baseTheme == "Lavender")
                themeFile = isDark ? "Themes/DarkLavenderTheme.xaml" : "Themes/LavenderTheme.xaml";
            else
                themeFile = isDark ? "Themes/DarkNeutralTheme.xaml" : "Themes/LightTheme.xaml";

            // Load new theme
            var newTheme = new ResourceDictionary { Source = new Uri(themeFile, UriKind.Relative) };
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(newTheme);

            // Update gradient background manually
            SetWindowGradient(baseTheme, isDark);
        }

        private void SetWindowGradient(string themeName, bool isDark)
        {
            LinearGradientBrush gradient = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };

            if (themeName == "Light")
            {
                if (isDark)
                {
                    // Dark Neutral fallback
                    gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FF1E1B1A"), 0));
                    gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FF2A2421"), 1));
                }
                else
                {
                    // Soft vanilla peach gradient
                    gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FFFFFAF3"), 0));
                    gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FFFFEDD7"), 1));
                }
            }
            else if (themeName == "Blush")
            {
                gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(isDark ? "#FF2D1E2A" : "#FFFCEFFB"), 0));
                gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(isDark ? "#FF3E2C38" : "#FFFFDDEE"), 1));
            }
            else if (themeName == "Ocean")
            {
                gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(isDark ? "#FF122529" : "#FFE6F9FA"), 0));
                gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(isDark ? "#FF1B3238" : "#FFD6F0F3"), 1));
            }
            else if (themeName == "Lavender")
            {
                gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(isDark ? "#FF2B2135" : "#FFF4E7FF"), 0));
                gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(isDark ? "#FF3A2E45" : "#FFE3D3FF"), 1));
            }
            else
            {
                // Fallback for unknown theme
                gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FF1E1B1A"), 0));
                gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FF2A2421"), 1));
            }

            this.Background = gradient;
        }

    }
}