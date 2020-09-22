using Microsoft.Win32;
using PDFApprentice.Controls;
using PDFApprentice.Data;
using System;
using System.Collections.Generic;
using System.IO;
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
        private SaveStatus CurrentSaveStatus { get; set; }
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
                    Title = GetPDFName();
                    break;
                case SaveStatus.Unsaved:
                    Title = System.IO.Path.GetFileNameWithoutExtension(PDF.PdfPath) + "*";
                    break;
                default:
                    break;
            }
            CurrentSaveStatus = status;
        }
        internal void Navigate(uint page)
            => PDF.Navigate(page);
        #endregion

        #region Events
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Show warning of unsaved progress
            if (CurrentSaveStatus == SaveStatus.Unsaved)
            {
                var result = MessageBox.Show("There are unsaved annotations. Would you like to make a save first?\n" +
                    "Click Yes to save before exit, \n" +
                    "Click No to discard unsaved annotations, \n" +
                    "Click Cancel to cancel exiting.",
                    "Confirmation", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);
                if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                else if (result == MessageBoxResult.Yes)
                    SaveCommand_Executed(null, null);
            }
            // Close all windows
            PropertyWindow.ShouldReallyClose = true;
            PropertyWindow.Close();
        }
        private void Window_Drop(object sender, DragEventArgs e)
        {
            // If the DataObject contains string data, extract it.
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string filePath = (e.Data.GetData(DataFormats.FileDrop) as IEnumerable<string>).First();

                // If the string is a file, open it
                if(File.Exists(filePath))
                    TryReopenFile(filePath);
            }
        }
        private void OpenCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
            => e.CanExecute = true;
        private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Select and open file
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*";
            if (dialog.ShowDialog() == true)
            {
                string filePath = dialog.FileName;
                // Open file
                TryReopenFile(filePath);
            }
        }
        private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
            => e.CanExecute = IsPDFAvailable();

        private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            PDF.Save();
            UpdateSaveStatus(SaveStatus.Saved);
        }
        private void ExitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
            => this.Close();
        private void ExitCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
            => e.CanExecute = true;

        private void StatisticsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show(
                $"Title: {GetPDFName()}\n" +
                $"Pages: {PDF.PagesContainer.Items.Count}\n" +
                $"Annotations: {PDF.Annotations.Count}",
                "Annotation Statistics", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void StatisticsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
            => e.CanExecute = IsPDFAvailable();
        private void ExportCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
            => e.CanExecute = IsPDFAvailable();
        private void ExportCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Generate file path
            var folderPath = System.IO.Path.GetDirectoryName(PDF.PdfPath);
            var exportName = GetPDFName() + ".md"; // It's markdown format plain text
            var exportPath = System.IO.Path.Combine(folderPath, exportName);
            // Generate content
            StringBuilder builder = new StringBuilder($"# {GetPDFName()}\n\n");
            foreach (Entity entity in PDF.Annotations
                .Select(a => a.GetEntity())
                .OrderBy(en => en.OwnerPage))
            {
                // Concatenate lines with a single space
                string content = entity.Content.Replace(Environment.NewLine, " ")
                    .Replace("\n", " ");    // For saved then loaded entities the line ending is already normalized
                builder.AppendLine($"* {(string.IsNullOrWhiteSpace(entity.Tags) ? string.Empty : $"({entity.Tags}) ")}{content}");
            }
            // Save
            File.WriteAllText(exportPath, builder.ToString());
            // Open export file
            System.Diagnostics.Process.Start(exportPath);
        }
        private void SummaryCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        => e.CanExecute = IsPDFAvailable();

        private void SummaryCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var popUp = new TagListView() { Owner = this };
            popUp.Entities = new System.Collections.ObjectModel.ObservableCollection<Entity>(PDF.Annotations.Select(a => a.GetEntity()));
            popUp.Show();
        }
        #endregion

        #region Subroutine
        private void TryReopenFile(string filePath)
        {
            // Show warning of unsaved progress
            if (CurrentSaveStatus == SaveStatus.Unsaved)
            {
                var result = MessageBox.Show("There are unsaved annotations. Would you like to make a save first?\n" +
                    "Click Yes to save before open new file, \n" +
                    "Click No to discard unsaved annotations, \n" +
                    "Click Cancel to cancel opening.",
                    "Confirmation", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);
                if (result == MessageBoxResult.Cancel)
                    return;
                else if (result == MessageBoxResult.Yes)
                    SaveCommand_Executed(null, null);
            }

            // Update path
            PDF.PdfPath = filePath;

            // Update title through save status (as a side effect)
            UpdateSaveStatus(SaveStatus.Saved);
            // Reset scale
            PDF.Scale = 1.0;
            // Reset annotation property window
            PropertyWindow.SetAnnotation(null);
        }
        private bool IsPDFAvailable()
            => PDF.PdfPath != null;
        private string GetPDFName()
            => System.IO.Path.GetFileNameWithoutExtension(PDF.PdfPath);
        #endregion
    }
}