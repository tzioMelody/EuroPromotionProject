using System.Configuration;
using System.Data;
using System.Windows;

namespace EuroPromotionProject
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
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
        }
    }
}


