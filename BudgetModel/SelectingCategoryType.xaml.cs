 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BudgetModel
{
    /// <summary>
    /// Interaction logic for SelectingCategoryType.xaml
    /// </summary>
    public partial class SelectingCategoryType : Window
    {
        public string SelectedCategoryType { get; private set; }

        public SelectingCategoryType()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Checks if any radio button within the CategoryTypeRadioPanel is selected.
        /// </summary>
        /// <returns>
        /// True if at least one radio button is checked; otherwise, false.
        /// </returns>
        private bool ChoosenCategoryType()
        {
            foreach (var categoryType in CategoryTypeRadioPanel.Children)
            {
                if (categoryType is RadioButton radioButton && radioButton.IsChecked == true)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Event handler for the Cancel button.
        /// Closes the dialog without saving any selection.
        /// </summary>
        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        /// <summary>
        /// Event handler for the add category button.
        /// Validates selection, sets the selected category type, and closes the dialog.
        /// </summary>
        private void AddCategoryType(object sender, RoutedEventArgs e)
        {
            if (ChoosenCategoryType())
            {
                foreach (var categoryType in CategoryTypeRadioPanel.Children)
                {
                    if (categoryType is RadioButton radioButton && radioButton.IsChecked == true)
                    {
                        SelectedCategoryType = radioButton.Content.ToString();
                        this.DialogResult = true; // Close the window with selection
                        return;
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a category type.");
            }
        }

        /// <summary>
        /// Event handler for checking a category type radio button (Income, Expense, etc.).
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments associated with the button click.</param>
        private void CategoryTypeRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton)
            {
                int categoryType = Convert.ToInt32(radioButton.Tag);        
            }
        }

    }
}
