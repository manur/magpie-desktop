This project is organized based on an MVC model.

There are 3 windows:
- MainWindow
- SettingsWindow
- HotkeyWindow

The Main and Settings windows are WPF-based windows. Their layout and controls are described using XAML, and their codebehind
files are simply a thin layer over the AppController's methods. 

The Hotkey window is an old-style WinForms window. This is because we're simply using it to register a global keyboard handler with 
that window, and when the keyboard shortcut is pressed, Window's will send a message to that window. The window itself is 
never displayed.

There is one Model class- the Settings model. This has the logic to save to and read from the settings file, which is stored in the
user's LocalAppData directory. If that file doesn't exist or is corrupted, the class will simply generate defaults.

The AppController class is the one responsible for changing the model based on events being fired in the view.

The Saving Control is stolen from http://elegantcode.com/2009/08/21/a-simple-wpf-loading-animation/

TODO: We need to write a setup application for this app. Sadly, the VS Express edition doesn't have support for 
setup type projects. The best option is probably to use WiX (http://wixtoolset.org/)