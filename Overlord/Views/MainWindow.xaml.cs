using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using M = System.Windows.Media;
using NotifyIcon = System.Windows.Forms.NotifyIcon;
using WindowsFormsContextMenu = System.Windows.Forms.ContextMenu;

namespace Overlord
{
    class LoadingAdorner: Adorner
    {
        public LoadingAdorner(UIElement element) :
            base(element)
        { }

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawRectangle(M.Brushes.AliceBlue, null, new Rect(0, 0, this.ActualWidth, this.ActualHeight));

        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, System.Windows.Forms.IWin32Window
    {
        NotifyIcon _trayIcon;
        WindowsFormsContextMenu _trayMenu;
        LoadingAdorner _loadingAdorner;

        public MainWindow()
        {
            InitializeComponent();
            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            this.ShowInTaskbar = false;
            this.Visibility = System.Windows.Visibility.Hidden;

            _trayMenu = new WindowsFormsContextMenu();
            _trayMenu.MenuItems.Add("&Settings", OnSettingsSelected);
            _trayMenu.MenuItems.Add("E&xit", OnExitSelected);

            _trayIcon = new NotifyIcon();
            _trayIcon.Icon = new Icon(SystemIcons.Application, 40, 40);
            _trayIcon.ContextMenu = _trayMenu;
            _trayIcon.Visible = true;
            _loadingAdorner = new LoadingAdorner(this);
            this.MainTextBox.Focus();
        }

        public void ShowSavingAnimation()
        {
            Canvas.SetLeft(savingAnimation, (savingCanvas.ActualWidth - savingAnimation.ActualWidth) / 2);
            Canvas.SetTop(savingAnimation, (savingCanvas.ActualHeight - savingAnimation.ActualHeight) / 2);
            savingBorder.Visibility = System.Windows.Visibility.Visible;
        }

        public void HideSavingAnimation()
        {
            savingBorder.Visibility = System.Windows.Visibility.Hidden;
        }

        private void OnExitSelected(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OnSettingsSelected(object sender, EventArgs e)
        {
            App.Controller.OnSettingsSelected();
        }

        void TrayIcon_OnClick(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Hide();
            }
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    this.MainTextBox.Text += "\n";
                    this.MainTextBox.CaretIndex = this.MainTextBox.Text.Length;
                }
                else
                {
                    App.Controller.SendText();
                }
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            var adorner = AdornerLayer.GetAdornerLayer(MainRectangle);
            adorner.Add(_loadingAdorner);
            // App.Controller.SendText();
        }

        public IntPtr Handle
        {
            get 
            {
                return (new WindowInteropHelper(this)).Handle;
            }
        }
    }
}
