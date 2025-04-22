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

namespace BudgetWPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
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

        var newTheme = new ResourceDictionary { Source = new Uri(themeFile, UriKind.Relative) };
        Application.Current.Resources.MergedDictionaries.Clear();
        Application.Current.Resources.MergedDictionaries.Add(newTheme);
    }

}
