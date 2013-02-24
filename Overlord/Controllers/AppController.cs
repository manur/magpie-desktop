using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Overlord.Models;
using System.Windows.Input;
using System.Threading;
using System.Windows.Threading;
using System.ComponentModel;
using System.Windows.Forms;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;
using System.Security.Principal;

namespace Overlord
{
    public class AppController
    {
        // Indicate to .NET that this class can be serialized to JSON
        [DataContract]
        class SendParameters
        {
            [DataMember]
            public string Text;

            [DataMember]
            public string Email;

            [IgnoreDataMember]
            public string RemoteServerAddress;
        };

        internal SettingsWindow SettingsWindow
        {
            get;
            private set;
        }

        internal MainWindow MainWindow
        {
            get;
            private set;
        }

        internal SettingsModel SettingsModel
        {
            get;
            private set;
        }

        internal HotkeyWindow HotkeyWindow
        {
            get;
            private set;
        }

        internal AppController()
        {
            this.SettingsWindow = new SettingsWindow();
            this.MainWindow = new MainWindow();
            this.HotkeyWindow = new HotkeyWindow();
            this.SettingsModel = new SettingsModel();
        }

        // The logic here is a little complicated but basically it's as follows:
        //  First we check if the app has permission to write to the user's registry keys
        //  If it does, we then check if the expected state is different from the current state
        //  if it is, then we make the required change
        //  If no changes are needed, it's a no-op
        //  However, if the app doesn't have permissions to write to the registry, we
        //  relaunch the current app with admin privileges and pass in magic params. These
        //  params simply write the required setting to the registry and then quit
        internal static void SetLaunchAtStartup(bool launchAtStartup)
        {
            bool canWrite = true;
            RegistryKey key;

            try
            {
                key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            }
            catch (UnauthorizedAccessException)
            {
                canWrite = false;
                key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run");
            }

            object value = key.GetValue(StringResources.ApplicationName, null);
            bool changeNeeded = ((value != null && !launchAtStartup) || (value == null && launchAtStartup));

            if (changeNeeded)
            {
                if (canWrite)
                {
                    if (value != null && !launchAtStartup)
                    {
                        key.DeleteValue(StringResources.ApplicationName);
                    }
                    else if (value == null && launchAtStartup)
                    {
                        string applicationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Application.ExecutablePath);
                        key.SetValue(StringResources.ApplicationName, applicationPath, RegistryValueKind.String);
                    }
                }
                else
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.Verb = "runas";
                    psi.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    psi.FileName = Application.ExecutablePath;
                    psi.UseShellExecute = true;
                    psi.Arguments = (launchAtStartup ? "enableLaunchAtStartup" : "disableLaunchAtStartup");

                    Process elevatedProcess = Process.Start(psi);
                    elevatedProcess.EnableRaisingEvents = true;
                    elevatedProcess.WaitForExit();
                }
            }
        }

        internal void OnSettingsSelected()
        {
            SettingsWindow.txtEmailAddress.Text = SettingsModel.UserEmailAddress;
            SettingsWindow.txtRemoteServer.Text = SettingsModel.RemoteServer;
            SettingsWindow.txtKeyboardShortcut.Text = SettingsModel.KeyboardShortcut;
            SettingsWindow.btnLaunchAtStartup.IsChecked = SettingsModel.LaunchAtStartup;

            this.SettingsWindow.Show();
        }

        internal void OnSave()
        {
            if (!SettingsModel.ValidateEmail(SettingsWindow.txtEmailAddress.Text))
            {
                SettingsWindow.ShowPopup(SettingsWindow.txtEmailAddress, StringResources.EmailInvalidError);
                return;
            }

            if (!SettingsModel.ValidateRemoteServer(SettingsWindow.txtRemoteServer.Text))
            {
                SettingsWindow.ShowPopup(SettingsWindow.txtRemoteServer, StringResources.RemoteServerInvalidError);
                return;
            }

            if (!SettingsModel.ValidateKey(_lastKey, _lastModifiers))
            {
                SettingsWindow.ShowPopup(SettingsWindow.txtKeyboardShortcut, StringResources.KeyShortcutInvalid);
                return;
            }

            SettingsModel.UserEmailAddress = SettingsWindow.txtEmailAddress.Text;
            SettingsModel.RemoteServer = SettingsWindow.txtRemoteServer.Text;

            bool oldLaunchAtStartup = SettingsModel.LaunchAtStartup;
            SettingsModel.LaunchAtStartup = SettingsWindow.btnLaunchAtStartup.IsChecked ?? false;

            if (oldLaunchAtStartup != SettingsModel.LaunchAtStartup)
            {
                AppController.SetLaunchAtStartup(SettingsModel.LaunchAtStartup);
            }

            if (_lastKey != null)
            {
                SettingsModel.KeyboardShortcutKey = _lastKey.Value;
            }

            if (_lastModifiers != null)
            {
                SettingsModel.KeyboardShortcutModifiers = _lastModifiers.Value;
            }

            SettingsModel.Save();

            this.SettingsWindow.Hide();
        }

        internal void OnCancel()
        {
            this.SettingsWindow.Hide();
        }

        internal void OnShortcutKeyUp(Key key, ModifierKeys modifierKeys)
        {
            ModifierKeys currentModifiers = Keyboard.Modifiers;
            // Scope to letters and digits now, might want to broaden
            // this. This was done because the keyup event gets fired
            // for system/modifier keys too (so like if you just pressed
            // alt)- we don't want the shortcut to be simply alt...
            if ((key >= Key.A && key <= Key.Z) ||
                (key >= Key.D0 && key <= Key.D9))
            {
                _lastKey = key;
                _lastModifiers = modifierKeys;
                SettingsWindow.txtKeyboardShortcut.Text = SettingsModel.KeyToString(_lastKey.Value, _lastModifiers.Value);
            }
        }

        internal void OnAppStartup()
        {
            _hotkeyId = Utils.SystemUtils.RegisterHotkey(this.HotkeyWindow, 
                this.SettingsModel.KeyboardShortcutKey, this.SettingsModel.KeyboardShortcutModifiers);
        }

        internal void OnAppShutdown()
        {
            Utils.SystemUtils.UnregisterHotkey(this.HotkeyWindow, _hotkeyId);
            this.SettingsModel.Save();
        }

        internal void OnHotkey()
        {
            MainWindow.Show();
        }

        internal void SendText()
        {
            SendParameters parameters = new SendParameters();
            parameters.Text = MainWindow.MainTextBox.Text;
            parameters.Email = SettingsModel.UserEmailAddress;
            parameters.RemoteServerAddress = SettingsModel.RemoteServer;

            MainWindow.ShowSavingAnimation();
            
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += MakeBackgroundRequest;
            worker.RunWorkerCompleted += OnRequestCompleted;
            worker.RunWorkerAsync(parameters);

        }

        // Run on the main thread
        private void OnRequestCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MainWindow.HideSavingAnimation();

            if (e.Error != null)
            {
                MessageBox.Show(MainWindow, StringResources.ServerError, StringResources.ApplicationName);
            }
            else
            {
                MainWindow.MainTextBox.Text = "";
                MainWindow.Hide();
            }
        }

        // Run on a background thread
        private void MakeBackgroundRequest(object sender, DoWorkEventArgs e)
        {
            SendParameters parameters = (SendParameters) e.Argument;
            HttpWebRequest webRequest = (HttpWebRequest) HttpWebRequest.Create(parameters.RemoteServerAddress);
            webRequest.Method = "POST";
            webRequest.ContentType = "application/json; charset=utf-8";

            using (MemoryStream memoryStream = new MemoryStream()) 
            {
                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(SendParameters));
                jsonSerializer.WriteObject(memoryStream, parameters);
                String jsonString = Encoding.UTF8.GetString(memoryStream.ToArray());

                using (StreamWriter streamWriter = new StreamWriter(webRequest.GetRequestStream()))
                {
                    streamWriter.Write(jsonString);
                    streamWriter.Close();
                }
            }
        }

        private Key? _lastKey;
        private ModifierKeys? _lastModifiers;
        private int _hotkeyId;
    }
}
