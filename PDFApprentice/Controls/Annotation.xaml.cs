using PDFApprentice.Data;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PDFApprentice.Controls
{
    /// <summary>
    /// Interaction logic for Annotation.xaml
    /// </summary>
    public partial class Annotation : UserControl, INotifyPropertyChanged
    {
        #region Constructor
        public Annotation(Canvas canvas, Entity entity)
        {
            Entity = entity;
            Canvas = canvas;
            InitializeComponent();
        }
        /// <summary>
        /// Containing canvas, which hosts the pdf page image and annotations on that page
        /// </summary>
        public Canvas Canvas;
        /// <summary>
        /// Contained note entity in this annotation representation
        /// </summary>
        private Entity Entity { get; }
        #endregion

        #region View Properties
        public string Note { get => Entity.Content; set { Entity.Content = value; NotifyPropertyChanged(); } }
        public string Tags { get => Entity.Tags; set { Entity.Tags = value; NotifyPropertyChanged(); } }
        #endregion

        #region Methods
        /// <summary>
        /// Used for serializatoin
        /// </summary>
        internal Entity GetEntity()
            => Entity;
        internal Location GetLocation()
            => Entity.Location;
        internal void UpdateLocation(int x, int y)
        {
            Entity.Location = new Location() { X = x, Y = y };
            Canvas.SetLeft(this, x);
            Canvas.SetTop(this, y);
        }
        internal string GetContent()
            => Entity.Content;
        internal void UpdateTags(string tags)
            => Tags = tags;
        internal string GetTags()
            => Entity.Tags;
        internal void UpdateNote(string note)
            => Note = note;
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
