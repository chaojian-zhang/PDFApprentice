using Microsoft.Win32;
using PDFApprentice.Controls;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PDFApprentice
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Constructor
        public MainWindow()
        {
            InitializeComponent();
            PropertyWindow = new AnnotationProperty(PDF);
            PDF.PropertyWindow = PropertyWindow;
        }
        private AnnotationProperty PropertyWindow { get; }
        #endregion

        #region Methods
        public enum SaveStatus
        {
            Saved,
            Unsaved
        }
        internal void UpdateSaveStatus(SaveStatus status)
        {
            if (PDF.PdfPath == null)
                return;
            switch (status)
            {
                case SaveStatus.Saved:
                    Title = System.IO.Path.GetFileNameWithoutExtension(PDF.PdfPath);
                    break;
                case SaveStatus.Unsaved:
                    Title = System.IO.Path.GetFileNameWithoutExtension(PDF.PdfPath) + "*";
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region Events
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            PropertyWindow.ShouldReallyClose = true;
            PropertyWindow.Close();
        }
        private void OpenCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
            => e.CanExecute = true;
        private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Select and open file
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*";
            if(dialog.ShowDialog() == true)
            {
                string filePath = dialog.FileName;
                PDF.PdfPath = filePath;
            }
            // Update title
            UpdateSaveStatus(SaveStatus.Saved);
        }
        private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
            => e.CanExecute = PDF.PdfPath != null;

        private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
            => PDF.Save();

        private void ExitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
            => this.Close();
        private void ExitCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
            => e.CanExecute = true;

        private void StatisticsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void StatisticsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
            => e.CanExecute = PDF.PdfPath != null;
        #endregion
    }
}