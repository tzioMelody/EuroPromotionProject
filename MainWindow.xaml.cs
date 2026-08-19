using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
        private bool isGridVisible = false;

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
            TxtSearchPlaceholder.Visibility = string.IsNullOrEmpty(TxtSearch.Text) ? Visibility.Visible : Visibility.Collapsed;

            if (clientInformationWin.allFiles == null) return;

            string searchText = TxtSearch.Text.ToLower();
            var filteredList = clientInformationWin.allFiles.Where(f => (f.FileName?.ToLower().Contains(searchText) ?? false) || (f.DateCreated?.ToLower().Contains(searchText) ?? false) || (f.ImportantNotes?.ToLower().Contains(searchText) ?? false)).ToList();
            DtgFiles.ItemsSource = filteredList;
        }


        private void TxtSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            TxtSearchPlaceholder.Visibility = Visibility.Collapsed;
        }

        private void TxtSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TxtSearch.Text))
            {
                TxtSearchPlaceholder.Visibility = Visibility.Visible;
            }
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

        private string GetImportantNotesFromJson(string pdfPath) //για να μπορεί να κάνει load μόνο αυτό το πεδίο την ώρα που κάνει load όλα τα pdf αρχεία
        {
            try
            {
                string jsonPath = System.IO.Path.ChangeExtension(pdfPath, ".json");
                if (File.Exists(jsonPath))
                {
                    string jsonString = File.ReadAllText(jsonPath);
                    var data = JsonSerializer.Deserialize<StatementData>(jsonString);
                    return data?.ImportantNotes;
                }
            }
            catch
            {  }
            return null;
        }

        public void LoadPdfFiles()
        {
            try
            {
                string folderPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EuroStatementPdf");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                clientInformationWin.allFiles = Directory.GetFiles(folderPath, "*.pdf")
                    .Select(f => new StatementFile
                    {
                        FileName = System.IO.Path.GetFileName(f),
                        FullPath = f,
                        DateCreated = File.GetCreationTime(f).ToString("dd/MM/yyyy HH:mm"),
                        ImportantNotes = GetImportantNotesFromJson(f) // για να γεμίσει η στήλη Σημαντικά και να μπορεί να γίνει και αναζήτηση με βάση αυτά
                    })
                    .OrderByDescending(x => x.DateCreated)
                    .ToList();

                DtgFiles.ItemsSource = clientInformationWin.allFiles;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Σφάλμα φόρτωσης: {ex.Message}");
            }
        }

        private void EditPdf_Click(object sender, RoutedEventArgs e) 
        {
            var button = sender as Button;
            if (button?.DataContext is StatementFile selected)
            {
                if (System.IO.File.Exists(selected.FullPath))
                {
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

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation();
            animation.Duration = TimeSpan.FromSeconds(0.4); 

            if (isGridVisible)
            {
                animation.From = 1.0;
                animation.To = 0.0;
                OverlayGrid.IsHitTestVisible = false; 
            }
            else
            {
                animation.From = 0.0;
                animation.To = 1.0;
                OverlayGrid.IsHitTestVisible = true; 
            }

            OverlayGrid.BeginAnimation(OpacityProperty, animation);
            isGridVisible = !isGridVisible;
        }
    }
}
