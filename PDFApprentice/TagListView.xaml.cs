using PDFApprentice.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace PDFApprentice
{
    /// <summary>
    /// Interaction logic for TagListView.xaml
    /// </summary>
    public partial class TagListView : Window, INotifyPropertyChanged
    {
        public TagListView()
        {
            InitializeComponent();
        }

        #region View Properties
        private ObservableCollection<Entity> _Entities;
        public ObservableCollection<Entity> Entities { get => _Entities; set { SetField(ref _Entities, value); NotifyPropertyChanged("Entities"); } }
        #endregion

        #region Data Binding
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        protected bool SetField<type>(ref type field, type value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<type>.Default.Equals(field, value)) return false;
            field = value;
            NotifyPropertyChanged(propertyName);
            return true;
        }

        #endregion

        #region Events
        private void ListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = (sender as ListViewItem).DataContext as Entity;
            (Owner as MainWindow).Navigate(item.OwnerPage);
        }
        #endregion
    }
}
