/*
  Easy Automatic Printer  - The Printer Utility
  Copyright (C) 2016 SeniuuS.

  This program is free software; you can redistribute it and/or modify
  it under the terms of the GNU General Public License version 3 as published by
  the Free Software Foundation.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program; if not, write to the Free Software
  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Printing;
using System;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Threading;
using System.Windows.Input;
using System.IO;
using System.Linq;
using System.Drawing;
using EAP.Config;
using log4net;
using EAP.Engine;
using System.Threading.Tasks;

namespace EAP
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class PrintWindow : Window
    {
        private static ILog log = LogManager.GetLogger(typeof(PrintWindow));

        /// <summary>
        /// This ObservableCollection will be used to store every Document object
        /// </summary>
        public ObservableCollection<Document> Documents { get; } = new ObservableCollection<Document>();

        public IList<Document> DocumentsInQueue { get; } = new List<Document>();

        /// <summary>
        /// Used to get every information of the current printer
        /// </summary>
        public PrintDocument PrintInformation { get; } = new PrintDocument();

        /// <summary>
        /// Configuration of the software
        /// </summary>
        public Configuration Configuration { get; } = new Configuration();
        
        List<Document> documentInQueueList = new List<Document>();

        public PrintWindow()
        {
            InitializeComponent();
            
            files.ItemsSource = Documents;
            printerName.Text = PrintInformation.PrinterSettings.PrinterName;
            
            LoadSettings();
        }

        private void SetNumberDocument()
        {
            NbDoc.Text = Documents.Count.ToString();
        }

        /// <summary>
        /// Method called by clicking on the "Open Files" MenuItem
        /// It opens an OpenFileDialog and create an object Document of all the documents you selected
        /// And it stores them in the ObservableCollection documentList
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnuSelectFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            string filterall = "All Printable File|";
            string filter = "|All Printable Document Types|";
            foreach (string extension in Configuration.DocumentExtension)
            {
                filter += "*." + extension;
                filterall += "*." + extension + ";";
                if (!extension.Equals(Configuration.DocumentExtension.Last()))
                    filter += ";";
            }
            string filterIMG = "|All Printable Image Types|";
            foreach (string extension in Configuration.ImageExtension)
            {
                filterIMG += "*." + extension;
                filterall += "*." + extension;
                if (!extension.Equals(Configuration.DocumentExtension.Last()))
                {
                    filterIMG += ";";
                    filterall += ";";
                }
            }

            ofd.Filter = filterall + filter + filterIMG;
            ofd.FilterIndex = 1;

            ofd.Multiselect = true;

            bool? userClickedOK = ofd.ShowDialog();

            if (userClickedOK == true)
                AddDocuments(ofd.FileNames);
            SetNumberDocument();
        }

        /// <summary>
        /// Calls the same method of Open Files when you click on the button Open File from the toolbar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            mnuSelectFile_Click(sender, e);
        }

        private void files_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
        }

        /// <summary>
        /// same as the method of Open Files, but it's from dragging the files into the ListView
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void files_ItemDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                AddDocuments((string[])e.Data.GetData(DataFormats.FileDrop));
            SetNumberDocument();
        }

        private void AddDocuments(string[] names)
        {
            foreach (string name in names)
                AddDocument(name);
        }

        private void AddDocument(string name)
        {
            string[] splitName = name.Split('\\');
            Document doc = new Document(splitName[splitName.Length - 1], name, splitName[splitName.Length - 1].Split('.')[splitName[splitName.Length - 1].Split('.').Length - 1].ToLower());
            if (Configuration.DocumentExtension.Contains(doc.Type) || Configuration.ImageExtension.Contains(doc.Type))
                Documents.Add(doc);
        }

        /// <summary>
        /// This method is called when clicking on "Drop selected files" MenuItem
        /// It removes every selected document of the ObservableCollection documentList
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnuDropFile_Click(object sender, RoutedEventArgs e)
        {
            if (files.SelectedItem != null)
            {
                List<object> list = new List<object>();
                foreach (var doc in files.SelectedItems)
                    list.Add(doc);
                foreach(var doc in list)
                    Documents.Remove(doc as Document);
            }
            SetNumberDocument();
        }

        /// <summary>
        /// Calls the method of "Drop Selected Files" MenuItem by clicking on the "Drop Files" button of the toolbar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDropFile_Click(object sender, RoutedEventArgs e)
        {
            mnuDropFile_Click(sender, e);
        }

        /// <summary>
        /// Method of the "Clear selected files" MenuItem, it drops every Document object of the ObservableCollection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnuDropAllFile_Click(object sender, RoutedEventArgs e)
        {
            Documents.Clear();
            SetNumberDocument();
        }

        /// <summary>
        /// Clicking on the "delete" key will call the DropFile method and drop every selected files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void files_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
                mnuDropFile_Click(sender, e);
        }

        /// <summary>
        /// Show-Hide the toolbar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnuShowToolBar_Click(object sender, RoutedEventArgs e)
        {
            if (!ShowToolBar.IsChecked)
                ToolBar.Visibility = Visibility.Collapsed;
            else
                ToolBar.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Quit the program
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnuExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        //Loads all the settings of settings.ini and set all the settings variables (saveLogs, autoRetry, ShowToolBar) to true or false on program start
        private void LoadSettings()
        {
            RetryAuto.IsChecked = Configuration.AutoRetry;
            ShowToolBar.IsChecked = Configuration.ShowToolbar;
            ToolBar.Visibility = Configuration.ShowToolbar ? Visibility.Visible : Visibility.Collapsed;
        }
        
        private void mnuRetryAuto_Click(object sender, RoutedEventArgs e)
        {
            Configuration.AutoRetry = RetryAuto.IsChecked;
        }

        private void mnuImageMargin_Click(object sender, RoutedEventArgs e)
        {
            Configuration.ImageMarginBool = ImageMargin.IsChecked;
        }

        private void mnuDonation_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=D34RXXY3MUSFA");
        }

        private void mnuAbout_Click(object sender, RoutedEventArgs e)
        {
            About newWindowAbout = new About();
            newWindowAbout.Left = this.Left + this.Width / 4;
            newWindowAbout.Top = this.Top + this.Height / 4;
            newWindowAbout.ShowDialog();
        }

        private void mnuChangePrint_Click(object sender, RoutedEventArgs e)
        {
            ChangePrinter choicePrinter = new ChangePrinter(PrintInformation);
            choicePrinter.Left = this.Left + this.Width / 4;
            choicePrinter.Top = this.Top + this.Height / 4;
            choicePrinter.ShowDialog();
            if (choicePrinter.DialogOK)
            {
                PrintInformation.PrinterSettings.PrinterName = choicePrinter.SelectedPrinter;
                printerName.Text = PrintInformation.PrinterSettings.PrinterName;
            }
        }
        private void btnChangePrint_Click(object sender, RoutedEventArgs e)
        {
            mnuChangePrint_Click(sender, e);
        }

        private void mnuPrint_Click(object sender, RoutedEventArgs e)
        {
            ManualResetEvent wait = new ManualResetEvent(false);
            wait.Set();

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;

            Stop stopdialog = new Stop(Documents, tokenSource);
            stopdialog.Left = this.Left + this.Width / 4;
            stopdialog.Top = this.Top + this.Height / 4;
            stopdialog.Pause.Set();

            var context = TaskScheduler.FromCurrentSynchronizationContext();

            PrinterEngine engine = new PrinterEngine(this, stopdialog, wait, token, context);

            Task.Factory.StartNew(() => { engine.Start(); }, tokenSource.Token).ContinueWith((task) => { engine.Finished(); }, context);

            if (Documents.Count != 0)
            {
                stopdialog.ShowDialog();
            }
        }
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            mnuPrint_Click(sender, e);
        }

        public void RefreshList(Document document)
        {
            files.SelectedItem = document;
            files.ScrollIntoView(files.SelectedItem);
        }
        
        protected override void OnClosing(CancelEventArgs e)
        {
            Configuration.SaveSettings();
            base.OnClosing(e);
        }
    }
}