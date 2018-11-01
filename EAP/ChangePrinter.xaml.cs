using System.Collections.Generic;
using System.Drawing.Printing;
using System.Windows;
using System.Windows.Controls;

namespace EAP
{
    /// <summary>
    /// Logique d'interaction pour ChangePrinter.xaml
    /// </summary>
    public partial class ChangePrinter : Window
    {
        public string SelectedPrinter { get; set; }
        public bool DialogOK { get; set; } = false;

        public ChangePrinter(PrintDocument pd)
        {
            InitializeComponent();
            printers.ItemsSource = PrinterSettings.InstalledPrinters;
            SelectedPrinter = pd.PrinterSettings.PrinterName;
            printers.SelectedItem = SelectedPrinter;
        }

        private void printers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedPrinter = printers.SelectedItem.ToString();
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            DialogOK = true;
            this.Close();
        }
    }
}
