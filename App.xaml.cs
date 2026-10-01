using System.Configuration;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EuroPromotionProject
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly CultureInfo Greek = new CultureInfo("el-GR");
        private static readonly CultureInfo English = new CultureInfo("en-US");

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this.DispatcherUnhandledException += (s, args) =>
            {
                MessageBox.Show(
                    "Σφάλμα κατά την εκκίνηση:\n\n" + args.Exception.ToString(),
                    "Unhandled Exception",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                args.Handled = true;
            };

            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                var ex = args.ExceptionObject as Exception;
                MessageBox.Show(
                    "Κρίσιμο σφάλμα:\n\n" + ex?.ToString(),
                    "Fatal Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            };

            EventManager.RegisterClassHandler(typeof(TextBox), UIElement.GotKeyboardFocusEvent,
                new KeyboardFocusChangedEventHandler(OnTextBoxGotFocus));
        }

        private static void OnTextBoxGotFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                InputLanguageManager.Current.CurrentInputLanguage =
                    tb.Name == "TxtEmail" ? English : Greek;
            }
        }
    }
}