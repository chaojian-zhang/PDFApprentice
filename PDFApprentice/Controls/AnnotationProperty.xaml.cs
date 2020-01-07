using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace PDFApprentice.Controls
{
    /// <summary>
    /// Interaction logic for AnnotationProperty.xaml
    /// </summary>
    public partial class AnnotationProperty : Window, INotifyPropertyChanged
    {
        #region Constructor
        public AnnotationProperty(PdfViewer viewer)
        {
            InitializeComponent();
            Viewer = viewer;
        }
        private PdfViewer Viewer { get; set; }
        #endregion

        #region Method
        private Annotation Annotation { get; set; }
        public void SetAnnotation(Annotation annotation)
        {
            Annotation = annotation;
            NotifyPropertyChanged(nameof(IsAnnotationAvailable));
            NotifyPropertyChanged(nameof(X));
            NotifyPropertyChanged(nameof(Y));
            NotifyPropertyChanged(nameof(Note));
            NotifyPropertyChanged(nameof(Tags));
        }
        #endregion

        #region View Properties
        public bool IsAnnotationAvailable { get => Annotation != null;  }
        public int X { get => Annotation?.GetLocation().X ?? 0; set { Annotation.UpdateLocation(value, Y); NotifyPropertyChanged(); } }
        public int Y { get => Annotation?.GetLocation().Y ?? 0; set { Annotation.UpdateLocation(X, value); NotifyPropertyChanged(); } }
        public string Note { get => Annotation?.GetContent() ?? string.Empty; set { Annotation.UpdateNote(value); NotifyPropertyChanged(); } }
        public string Tags { get => Annotation?.GetTags() ?? string.Empty; set { Annotation.UpdateTags(value); NotifyPropertyChanged(); } }
        #endregion

        #region Events
        public bool ShouldReallyClose = false;
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // If we are not really closing (only truly close when main window is closed), just hide ourselves
            if (!ShouldReallyClose)
            {
                Hide();
                e.Cancel = true;
            }
        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if(Annotation != null)
            {
                Viewer.DeleteAnnotation(Annotation);
                SetAnnotation(null);
            }
        }
        #endregion

        #region Data Binding
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void NotifyPropertyChanged([CallerMemberName]string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        protected bool SetField<type>(ref type field, type value, [CallerMemberName]string propertyName = null)
        {
            if (EqualityComparer<type>.Default.Equals(field, value)) return false;
            field = value;
            NotifyPropertyChanged(propertyName);
            return true;
        }
        #endregion
    }
}
