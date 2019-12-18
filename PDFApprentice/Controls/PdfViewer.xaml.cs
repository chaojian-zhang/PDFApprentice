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
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;

namespace PDFApprentice.Controls
{
    public class ImageTag
    {
        public PdfViewer Viewer;
        public Canvas Canvas;
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
    public partial class PdfViewer : UserControl
    {
        #region Bindable Properties
        public string PdfPath
        {
            get { return (string)GetValue(PdfPathProperty); }
            set { SetValue(PdfPathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PdfPath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PdfPathProperty =
            DependencyProperty.Register("PdfPath", typeof(string), typeof(PdfViewer), new PropertyMetadata(null, propertyChangedCallback: OnPdfPathChanged));

        private static void OnPdfPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pdfDrawer = (PdfViewer)d;

            if (!string.IsNullOrEmpty(pdfDrawer.PdfPath))
            {
                // Making sure it's an absolute path
                var path = System.IO.Path.GetFullPath(pdfDrawer.PdfPath);

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
        #endregion

        #region Private Properties
        ImageTag LastImage { get; set; }
        #endregion

        #region Events
        private static void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Image image = sender as Image;
            if(image != null)
            {
                ImageTag tag = image.Tag as ImageTag;
                tag.Viewer.LastImage = tag;
            }
        }
        private void PdfViewer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(LastImage != null)
            {
                // Get potision
                Canvas canvas = LastImage.Canvas;
                // Create entity
                Entity entity = new Entity()
                {
                    Content = "Test"
                };
                Annotation annotation = new Annotation(entity);
                // Add annotation to canvas
                Point position = e.GetPosition(canvas);
                annotation.SetValue(Canvas.LeftProperty, position.X);
                annotation.SetValue(Canvas.TopProperty, position.Y);
                canvas.Children.Add(annotation);
            }
        }
        #endregion

        #region Subroutine
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
        #endregion
    }
}
