using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PDFApprentice
{
    public static class PDFApprenticeCommands
    {
        #region UI Commands
        public static readonly RoutedUICommand Export = new RoutedUICommand("Export note data into YAML", "Export", typeof(PDFApprenticeCommands), new InputGestureCollection { new KeyGesture(Key.E, ModifierKeys.Control, "Ctrl+E") });
        public static readonly RoutedUICommand Statistics = new RoutedUICommand("Show PDF statistics", "Statistics", typeof(PDFApprenticeCommands), new InputGestureCollection { new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift, "Ctrl+Shift+S") });
        #endregion
    }
}
