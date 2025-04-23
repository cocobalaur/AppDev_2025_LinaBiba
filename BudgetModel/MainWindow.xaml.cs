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
    /// Interaction logic for MainWindow.xaml.
    /// This window allows the user to apply different themes (Blush, Ocean, Lavender, Light)
    /// and toggle between light and dark mode.
    /// The background gradient is set programmatically to match the selected theme.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Constructor: Initializes the main window and applies the default Light theme on startup.
        /// </summary>
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

        /// <summary>
        /// Event handler for the Apply Theme button.
        /// Applies the selected theme and dark mode preference,
        /// and manually sets the matching background gradient.
        /// </summary>
        private void ApplyTheme_Click(object sender, RoutedEventArgs e)
        {
            // Get selected theme from ComboBox
            string baseTheme = ((ComboBoxItem)ColorComboBox.SelectedItem)?.Content?.ToString();

            // Determine if dark mode is checked
            bool isDark = DarkModeCheckBox.IsChecked == true;

            // Determine correct theme file path
            string themeFile;
            if (baseTheme == "Blush")
                themeFile = isDark ? "Themes/DarkBlushTheme.xaml" : "Themes/BlushTheme.xaml";
            else if (baseTheme == "Ocean")
                themeFile = isDark ? "Themes/DarkOceanTheme.xaml" : "Themes/OceanTheme.xaml";
            else if (baseTheme == "Lavender")
                themeFile = isDark ? "Themes/DarkLavenderTheme.xaml" : "Themes/LavenderTheme.xaml";
            else // Default to Light/DarkNeutral if unrecognized
                themeFile = isDark ? "Themes/DarkNeutralTheme.xaml" : "Themes/LightTheme.xaml";

            // Load and apply the selected theme resource dictionary
            var newTheme = new ResourceDictionary { Source = new Uri(themeFile, UriKind.Relative) };
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(newTheme);

            // Manually apply the matching gradient background
            SetWindowGradient(baseTheme, isDark);
        }

        /// <summary>
        /// Sets the gradient background of the window based on the selected theme and dark mode setting.
        /// </summary>
        /// <param name="themeName">The name of the base theme (e.g., Light, Blush, Ocean, Lavender).</param>
        /// <param name="isDark">True if dark mode is enabled; otherwise, false.</param>
        private void SetWindowGradient(string themeName, bool isDark)
        {
            // Create the gradient brush to be applied to the window background
            LinearGradientBrush gradient = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };

            // Apply color stops based on theme and mode
            if (themeName == "Light")
            {
                if (isDark)
                {
                    // Dark Neutral fallback for Light + DarkMode
                    gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FF1E1B1A"), 0));
                    gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FF2A2421"), 1));
                }
                else
                {
                    // Light theme vanilla peach gradient
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
                // Fallback neutral gradient for unknown theme
                gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FF1E1B1A"), 0));
                gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FF2A2421"), 1));
            }

            // Apply the gradient to the window
            this.Background = gradient;
        }

    }
}