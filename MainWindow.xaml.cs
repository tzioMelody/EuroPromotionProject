using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static EuroPromotionProject.ClientInformationWin;

namespace EuroPromotionProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ClientInformationWin clientInformationWin = new ClientInformationWin();

        public MainWindow()
        {
            InitializeComponent();
            LoadPdfFiles();
            clientInformationWin.InitWindow(this);
        }


        private void AsyncBtnAddNewStatement_Click(object sender, RoutedEventArgs e)
        {
            ClientInformationWin clientInfWin = new ClientInformationWin();
            clientInfWin.Show();
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (clientInformationWin.allFiles == null) return;

            string searchText = TxtSearch.Text.ToLower();
            var filteredList = clientInformationWin.allFiles.Where(f => f.FileName.ToLower().Contains(searchText)).ToList();
            DtgFiles.ItemsSource = filteredList;
        }

        private void DtgFiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DtgFiles.SelectedItem is StatementFile selected)
            {
                OpenFile(selected.FullPath);
            }
        }

        private void OpenPdf_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is StatementFile selected)
            {
                OpenFile(selected.FullPath);
            }
        }
        private void OpenFile(string path)
        {
            if (System.IO.File.Exists(path))
            {
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            }
            else
            {
                MessageBox.Show("Το αρχείο δεν βρέθηκε πλέον στον φάκελο.");
            }
        }

        public void LoadPdfFiles()
        {
            try
            {
                // Φάκελος EuroStatementPdf στο directory της εφαρμογής
                string folderPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EuroStatementPdf");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                // Διάβασμα των PDF και αποθήκευση στη λίστα allFiles
                clientInformationWin.allFiles = Directory.GetFiles(folderPath, "*.pdf")
                    .Select(f => new StatementFile
                    {
                        FileName = System.IO.Path.GetFileName(f),
                        FullPath = f,
                        DateCreated = File.GetCreationTime(f).ToString("dd/MM/yyyy HH:mm")
                    })
                    .OrderByDescending(x => x.DateCreated)
                    .ToList();

                // Εμφάνιση στο DataGrid
                DtgFiles.ItemsSource = clientInformationWin.allFiles;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Σφάλμα φόρτωσης: {ex.Message}");
            }
        }

        private void EditPdf_Click(object sender, RoutedEventArgs e) // ή EditPdf_Click
        {
            var button = sender as Button;
            if (button?.DataContext is StatementFile selected)
            {
                if (System.IO.File.Exists(selected.FullPath))
                {
                    // Ανοίγουμε το παράθυρο περνώντας του το αρχείο προς επεξεργασία
                    ClientInformationWin clientInfWin = new ClientInformationWin(selected);
                    clientInfWin.Show();
                }
                else
                {
                    MessageBox.Show("Το αρχείο δεν βρέθηκε.", "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void showEditedPdf_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closing(object sender, EventArgs e)
        {

            Environment.Exit(Environment.ExitCode);
        }
    }
}