using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace Overlord.Models
{
    public class SettingsModel
    {
        public SettingsModel()
        {
            string appSettingsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                StringResources.ApplicationName);

            this.SettingsFilePath = Path.Combine(appSettingsFolder, "Settings.txt");
            SetDefaultSettings();

            if (!Directory.Exists(appSettingsFolder))
            {
                Directory.CreateDirectory(appSettingsFolder);
            }
            else
            {
                TryLoadSettingsFromFile();
            }
        }

        private bool IsLaunchAtStartup()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run");
            object value = key.GetValue(StringResources.ApplicationName, null);

            return (value != null);
        }

        private void SetDefaultSettings()
        {
            this.RemoteServer = "http://magpie.io";
            this.UserEmailAddress = "";
            this.LaunchAtStartup = IsLaunchAtStartup();
            this.KeyboardShortcutKey = Key.O;
            this.KeyboardShortcutModifiers = ModifierKeys.Windows | ModifierKeys.Shift;
        }

        private void TryLoadSettingsFromFile()
        {
            if (File.Exists(this.SettingsFilePath))
            {
                using (FileStream fs = File.Open(this.SettingsFilePath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(fs))
                    {
                        try
                        {
                            this.RemoteServer = reader.ReadLine();
                            this.UserEmailAddress = reader.ReadLine();
                            this.KeyboardShortcutKey = (Key) Enum.Parse(typeof(Key), reader.ReadLine());
                            this.KeyboardShortcutModifiers = (ModifierKeys)Enum.Parse(typeof(ModifierKeys), reader.ReadLine());
                            this.ValidateParams();
                        }
                        catch (Exception)
                        {
                            // If the file format is corrupted, simply switch back to the default settings
                            SetDefaultSettings();
                        }
                    }
                }
            }
        }

        private void ValidateParams()
        {
            bool valid = ValidateRemoteServer(this.RemoteServer);
            valid &= ValidateEmail(this.UserEmailAddress);
            valid &= ValidateLaunchAtStartup(this.LaunchAtStartup);
            if (!valid)
            {
                throw new System.ApplicationException("Config file is corrupted");
            }
        }

        private bool ValidateLaunchAtStartup(bool launchAtStartup)
        {
            return true;
        }

        // TODO: use a better regex here which actually complies with the pertinent RFC
        private static readonly Regex EmailRegex = new Regex("^.*\\@.*\\..?.?.?.?$", RegexOptions.Compiled);
        public static bool ValidateEmail(string email)        
        {            
            return EmailRegex.IsMatch(email);
        }

        public bool ValidateRemoteServer(string remoteServer)
        {
            // TODO
            return true;
        }

        private string SettingsFilePath { get; set; }
        public string RemoteServer { get; set; }
        public string UserEmailAddress { get; set; }
        public bool LaunchAtStartup { get; set; }

        public static string KeyToString(Key key, ModifierKeys modifiers)
        {
            List<string> sb = new List<string>();
            if ((modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                sb.Add("ALT");
            }
            if ((modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                sb.Add("CTRL");
            }
            if ((modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                sb.Add("SHIFT");
            }
            if ((modifiers & ModifierKeys.Windows) == ModifierKeys.Windows)
            {
                sb.Add("WIN");
            }
            sb.Add(key.ToString());

            return String.Join("+", sb.ToArray());
        }

        public string KeyboardShortcut
        {
            get
            {
                return KeyToString(KeyboardShortcutKey, KeyboardShortcutModifiers);
            }
        }

        public Key KeyboardShortcutKey { get; set; }
        public ModifierKeys KeyboardShortcutModifiers { get; set; }

        public void Save()
        {
            using (FileStream fs = File.Open(this.SettingsFilePath, FileMode.OpenOrCreate))
            {
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    try
                    {
                        writer.WriteLine(this.RemoteServer);
                        writer.WriteLine(this.UserEmailAddress);
                        writer.WriteLine(this.KeyboardShortcutKey.ToString());
                        writer.WriteLine(this.KeyboardShortcutModifiers.ToString());
                        this.ValidateParams();
                    }
                    catch (Exception)
                    {
                        // If the file format is corrupted, simply switch back to the default settings
                        SetDefaultSettings();
                    }
                }
            }
        }

        internal static bool ValidateKey(Key? _lastKey, ModifierKeys? _lastModifiers)
        {
            if (_lastKey != null && (_lastModifiers == null || _lastModifiers.Value == ModifierKeys.None))
                return false;

            return true;
        }
    }
}
