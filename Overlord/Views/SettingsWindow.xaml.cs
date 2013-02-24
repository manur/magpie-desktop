using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace Overlord
{
    public class TextInputToVisibilityConverter : System.Windows.Data.IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Always test MultiValueConverter inputs for non-null
            // (to avoid crash bugs for views in the designer)
            if (values[0] is bool && values[1] is bool)
            {
                bool hasText = !(bool)values[0];
                bool hasFocus = (bool)values[1];

                if (hasFocus || hasText)
                    return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            this.ShowInTaskbar = false;
            this.popupInfo.LostFocus += PopupInfo_LostFocus;
            this.popupInfo.MouseUp += PopupInfo_MouseUp;
        }

        private void PopupInfo_LostFocus(object sender, RoutedEventArgs e)
        {
            this.popupInfo.IsOpen = false;
        }

        void PopupInfo_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.popupInfo.IsOpen = false;
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            App.Controller.OnSave();
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            App.Controller.OnCancel();
        }

        private void ShortcutText_OnKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            App.Controller.OnShortcutKeyUp(e.Key, Keyboard.Modifiers);
        }

        internal void ShowPopup(System.Windows.Controls.TextBox textBox, string p)
        {
            Popup popup = this.popupInfo;
            popup.PlacementTarget = textBox;
            popup.HorizontalOffset = -10;
            popup.VerticalOffset = (textBox.ActualHeight - popupCanvas.Height) / 2;
            txtBlockPopup.Text = p;
            popup.IsOpen = true;
            popup.StaysOpen = false;
            popup.Focus();
        }
    }
}
