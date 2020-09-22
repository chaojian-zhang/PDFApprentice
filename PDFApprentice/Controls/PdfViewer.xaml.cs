using PDFApprentice.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
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
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;
using YamlDotNet.Serialization;

namespace PDFApprentice.Controls
{
    /// <summary>
    /// A tag object for PDF rendered page images containing identifying information for the page
    /// </summary>
    public class ImageTag
    {
        /// <summary>
        /// Reference to the PDF viewer container
        /// </summary>
        public PdfViewer Viewer;
        /// <summary>
        /// Reference to the presenting canvas
        /// </summary>
        public Canvas Canvas;
        /// <summary>
        /// Page number
        /// </summary>
        public uint PageID;

        public ImageTag(PdfViewer viewer, uint pageID, Canvas canvas)
        {
            Viewer = viewer;
            PageID = pageID;
            Canvas = canvas;
        }
    }

    /// <summary>
    /// Interaction logic for PdfViewer.xaml
    /// </summary>
    public partial class PdfViewer : UserControl, INotifyPropertyChanged
    {
        #region Bindable Dependency Properties
        /// <summary>
        /// Path of the PDF file being opened
        /// </summary>
        public string PdfPath
        {
            get { return (string)GetValue(PdfPathProperty); }
            set { SetValue(PdfPathProperty, value); }
        }

        /// <remarks>Using a DependencyProperty as the backing store for PdfPath.  This enables animation, styling, binding, etc...</remarks>
        public static readonly DependencyProperty PdfPathProperty =
            DependencyProperty.Register("PdfPath", typeof(string), typeof(PdfViewer), new PropertyMetadata(null, propertyChangedCallback: OnPdfPathChanged));
        /// <summary>
        /// Callback when pdf path is updated
        /// </summary>
        private static void OnPdfPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pdfDrawer = (PdfViewer)d;

            if (!string.IsNullOrEmpty(pdfDrawer.PdfPath))
            {
                // Making sure it's an absolute path
                var path = System.IO.Path.GetFullPath(pdfDrawer.PdfPath);

                // Clear annotations list
                pdfDrawer.Annotations = new List<Annotation>();

                // Get file
                StorageFile.GetFileFromPathAsync(path).AsTask()
                  // Load pdf document on background thread
                  .ContinueWith(t => PdfDocument.LoadFromFileAsync(t.Result).AsTask()).Unwrap()
                  // Display on UI Thread
                  .ContinueWith(t2 => PdfToImages(pdfDrawer, t2.Result), TaskScheduler.FromCurrentSynchronizationContext());
            }
        }
        #endregion

        #region Constructor
        public PdfViewer()
            => InitializeComponent();
        public AnnotationProperty PropertyWindow { get; set; }
        #endregion

        #region View Properties
        private const double MinScale = 1.0;
        private const double MaxScale = 3.0;
        private double _Scale = 1.0;
        public double Scale { get => _Scale; set => SetField(ref _Scale, value); }
        #endregion

        #region Annotation Collection
        public List<Annotation> Annotations { get; set; }
        #endregion

        #region Private Properties
        /// <summary>
        /// A reference to the current image being clicked on
        /// </summary>
        ImageTag CurrentImage { get; set; }
        #endregion

        #region Interface
        private const string PDFApprenticeFileSuffix = ".papr";
        public void Save()
        {
            // Generate path
            string savePath = GetSavePath();
            // Preprocess line ending
            foreach (Annotation annoation in Annotations)
            {
                Entity entity = annoation.GetEntity();
                entity.Content = entity.Content.Replace(Environment.NewLine, "\n");
            }
            // Sort in page order
            List<Entity> entitiesSorted = Annotations.Select(a => a.GetEntity())
                .OrderBy(e => e.OwnerPage).ToList();
            // Generate serialized text
            Serializer serializer = new YamlDotNet.Serialization.Serializer();
            string serialization = serializer.Serialize(entitiesSorted);
            // Save 
            File.WriteAllText(savePath, serialization);
        }
        internal void DeleteAnnotation(Annotation annotation)
        {
            // Delete from canvas
            annotation.Canvas.Children.Remove(annotation);
            // Delete from collection
            Annotations.Remove(annotation);
        }
        internal void Navigate(uint page)
        {
            double offset = 0;
            for (int i = 0; i < page && i < PagesContainer.Items.Count; i++)
            {
                double itemHeight = (PagesContainer.Items[i] as Canvas).ActualHeight + 4;
                offset += itemHeight;
            }
            Scroll.ScrollToVerticalOffset(offset);
        }
        #endregion

        #region Events
        private void UserControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Scroll scale
            double scaleSpeed = 1.5;
            if (PdfPath != null && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                int direction = e.Delta > 0 ? 1 : -1;
                if (direction == 1)
                {
                    double portion = (double)(MaxScale - Scale) / (MaxScale - MinScale);
                    Scale += Math.Pow(portion, scaleSpeed);
                }
                else
                {
                    double portion = (double)(Scale - MinScale) / (MaxScale - MinScale);
                    Scale -= Math.Pow(portion, scaleSpeed);
                }
                // Clamp
                if (Scale > MaxScale)
                    Scale = MaxScale;
                else if (Scale < MinScale)
                    Scale = MinScale;
                e.Handled = true;
            }
        }
        /// <summary>
        /// Image object callback, registers self to the viewer as current image being clicked on
        /// </summary>
        private static void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Image image = sender as Image;
            if(image != null)
            {
                ImageTag tag = image.Tag as ImageTag;
                tag.Viewer.CurrentImage = tag;
            }
        }
        /// <summary>
        /// Handles creating new notes
        /// </summary>
        private void PdfViewer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(CurrentImage != null)
            {
                // Get potision
                Canvas canvas = CurrentImage.Canvas;
                // Create annotation
                var position = e.GetPosition(canvas);
                Entity entity = new Entity()
                {
                    Content = "New Note",
                    Location = new Location()
                    {
                        X = (int)position.X,
                        Y = (int)position.Y
                    },
                    OwnerPage = CurrentImage.PageID
                };
                Annotation annotation = CreateAnnotation(canvas, entity);
                // Show annotation property
                ShowAnnotationProperty(annotation, null);
                // Indicate file unsaved
                (Window.GetWindow(this) as MainWindow).UpdateSaveStatus(MainWindow.SaveStatus.Unsaved);
            }
        }
        private void ShowAnnotationProperty(object sender, MouseButtonEventArgs e)
        {
            if(PropertyWindow != null && sender is Annotation)
            {
                // Format old annotation
                if(PropertyWindow.GetAnnotation() != null)
                {
                    var annotation = PropertyWindow.GetAnnotation();
                    SimpleFormatText(annotation.NoteText.Inlines, annotation.Note);
                }
                // Show new annotation
                PropertyWindow.SetAnnotation(sender as Annotation);
                PropertyWindow.Show();
                PropertyWindow.Owner = Window.GetWindow(this);
            }
        }
        #endregion

        #region Subroutine
        enum ParseState
        {
            NormalText,
            BoldStart,
            ItalicStart
        }
        /// <summary>
        /// Simple format Markdown text for TextBlock
        /// </summary>
        private void SimpleFormatText(InlineCollection inlines, string note)
        {
            inlines.Clear();
            // Only bold and italic is supported for now
            StringBuilder builder = new StringBuilder();
            ParseState state = ParseState.NormalText;
            for (int i = 0; i < note.Length; i++)
            {
                char c = note[i];
                switch (state)
                {
                    case ParseState.NormalText:
                        {
                            if (c != '*')
                                builder.Append(c);
                            // Make sure we have something to come
                            else if (i < note.Length - 1)
                            {
                                // Bold
                                if (note[i + 1] == '*')
                                {
                                    i++; 
                                    state = ParseState.BoldStart;
                                }
                                else
                                    state = ParseState.ItalicStart;
                                // Save current
                                inlines.Add(builder.ToString());
                                builder.Clear();
                            }
                        }
                        break;
                    case ParseState.BoldStart:
                        {
                            if (c != '*')
                                builder.Append(c);
                            else
                            {
                                i++;    // Assume complete
                                inlines.Add(new Bold(new Run(builder.ToString())));
                                builder.Clear();
                                state = ParseState.NormalText;
                            }
                        }
                        break;
                    case ParseState.ItalicStart:
                        {
                            if (c != '*')
                                builder.Append(c);
                            else
                            {
                                inlines.Add(new Italic(new Run(builder.ToString())));
                                builder.Clear();
                                state = ParseState.NormalText;
                            }
                        }
                        break;
                }
            }
            // Add remaining text
            if(builder.Length != 0)
                inlines.Add(builder.ToString());            
        }
        private async static Task PdfToImages(PdfViewer pdfViewer, PdfDocument pdfDoc)
        {
            ItemCollection items = pdfViewer.PagesContainer.Items;
            items.Clear();

            if (pdfDoc == null) return;

            for (uint i = 0; i < pdfDoc.PageCount; i++)
            {
                using (var page = pdfDoc.GetPage(i))
                {
                    var bitmap = await PageToBitmapAsync(page);
                    // Create containing canvas element
                    var canvas = new Canvas()
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 4, 0, 4),
                        MaxWidth = 800,
                        Width = bitmap.PixelWidth,
                        Height = bitmap.PixelHeight
                    };
                    // Create image elements
                    var image = new Image
                    {
                        Source = bitmap,
                        Tag = new ImageTag(pdfViewer, i, canvas), // Tag page number
                    };
                    image.MouseDown += Image_MouseDown;
                    // Add image to canvas
                    canvas.Children.Add(image);
                    // Add canvas
                    items.Add(canvas);
                }
            }

            // Automaticaly load annotations
            LoadExistingAnnotations(pdfViewer);
        }
        private static void LoadExistingAnnotations(PdfViewer pdfViewer)
        {
            if(pdfViewer.PdfPath != null)
            {
                string savePath = pdfViewer.GetSavePath();
                if (File.Exists(savePath))
                {
                    string yaml = File.ReadAllText(savePath);
                    List<Entity> entities = new Deserializer().Deserialize<List<Entity>>(yaml);

                    // Create annotation for each entity
                    foreach (var entity in entities)
                    {
                        Canvas canvas = pdfViewer.PagesContainer.Items.GetItemAt((int)(entity.OwnerPage)) as Canvas;
                        var annotation = pdfViewer.CreateAnnotation(canvas, entity);
                        // Format
                        pdfViewer.SimpleFormatText(annotation.NoteText.Inlines, annotation.Note);
                    }
                }
            }
        }
        private static async Task<BitmapImage> PageToBitmapAsync(PdfPage page)
        {
            BitmapImage image = new BitmapImage();

            using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
            {
                await page.RenderToStreamAsync(stream);

                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream.AsStream();
                image.EndInit();
            }

            return image;
        }
        private string GetSavePath()
        {
            string folderPath = System.IO.Path.GetDirectoryName(PdfPath);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(PdfPath) + PDFApprenticeFileSuffix;
            string savePath = System.IO.Path.Combine(folderPath, fileName);
            return savePath;
        }
        /// <summary>
        /// Create a new annotation and add it to the viewer
        /// </summary>
        /// <param name="canvas">The canvas the annotation belongs to</param>
        private Annotation CreateAnnotation(Canvas canvas, Entity entity)
        {
            Annotation annotation = new Annotation(canvas, entity);
            annotation.MouseDown += ShowAnnotationProperty;
            // Add annotation to canvas
            annotation.SetValue(Canvas.LeftProperty, (double)entity.Location.X);
            annotation.SetValue(Canvas.TopProperty, (double)entity.Location.Y);
            canvas.Children.Add(annotation);
            // Add annotation to collection
            Annotations.Add(annotation);
            return annotation;
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
