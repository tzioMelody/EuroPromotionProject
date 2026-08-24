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
        private bool syncPopUpIsClosed = false;


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

        private string GetImportantNotesFromJson(string pdfPath)
        {
            try
            {
                string folder = System.IO.Path.GetDirectoryName(pdfPath);
                string fileNameNoExt = System.IO.Path.GetFileNameWithoutExtension(pdfPath);
                string jsonPath = System.IO.Path.Combine(folder, "JsonData", fileNameNoExt + ".json");

                if (File.Exists(jsonPath))
                {
                    string jsonString = File.ReadAllText(jsonPath);
                    var data = JsonSerializer.Deserialize<StatementData>(jsonString);
                    return data?.ImportantNotes;
                }
            }
            catch
            {}
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
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await CheckSyncStatusAsync();
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

        private void BtnToggleStatus_Click(object sender, RoutedEventArgs e)
        {
            if (syncPopUpIsClosed)
            {
                syncPopUpIsClosed = false;
                return;
            }

            StatusPopup.IsOpen = true;
        }

        private string GetOneDrivePath()
        {
            string oneDrivePath = Environment.GetEnvironmentVariable("OneDrive") ?? Environment.GetEnvironmentVariable("OneDriveCommercial");
            if (string.IsNullOrEmpty(oneDrivePath)) return null;
            return System.IO.Path.Combine(oneDrivePath, "EuroStatementsPdf");
        }

        public async Task CheckSyncStatusAsync()
        {
            string localPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EuroStatementPdf");
            string localJsonPath = System.IO.Path.Combine(localPath, "JsonData");
            string drivePath = GetOneDrivePath();
            string driveJsonPath = drivePath != null ? System.IO.Path.Combine(drivePath, "JsonData") : null;

            if (!Directory.Exists(localPath) || string.IsNullOrEmpty(drivePath) || !Directory.Exists(drivePath))
            {
                UpdateUIStatus(Brushes.Red, new List<string> { "Δεν βρέθηκε ο φάκελος OneDrive" });
                return;
            }

            // pdf αρχεία
            var localFiles = Directory.GetFiles(localPath).Select(System.IO.Path.GetFileName).ToList();
            var driveFiles = new HashSet<string>(Directory.GetFiles(drivePath).Select(System.IO.Path.GetFileName));
            var missingFiles = localFiles.Where(file => !driveFiles.Contains(file)).ToList();

            // json αρχεία 
            var localJsonFiles = Directory.Exists(localJsonPath) ? Directory.GetFiles(localJsonPath).Select(f => "JsonData/" + System.IO.Path.GetFileName(f)).ToList() : new List<string>();
            var driveJsonFiles = (driveJsonPath != null && Directory.Exists(driveJsonPath)) ? new HashSet<string>(Directory.GetFiles(driveJsonPath).Select(f => "JsonData/" + System.IO.Path.GetFileName(f))) : new HashSet<string>();
            var missingJsonFiles = localJsonFiles.Where(file => !driveJsonFiles.Contains(file)).ToList();

            var allLocal = localFiles.Concat(localJsonFiles).ToList();
            var allMissing = missingFiles.Concat(missingJsonFiles).ToList();

            if (allMissing.Count == 0)
            {
                UpdateUIStatus(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981")), allMissing);
            }
            else if (allMissing.Count == allLocal.Count)
            {
                UpdateUIStatus(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444")), allMissing);
            }
            else
            {
                UpdateUIStatus(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B")), allMissing);
            }
        }

        
        private void UpdateUIStatus(Brush color, List<string> missingFiles)
        {
            StatusCircle.Fill = color;
            LstUnsyncedFiles.ItemsSource = missingFiles;
        }

        private async void BtnManualSync_Click(object sender, RoutedEventArgs e)
        {
            BtnManualSync.IsEnabled = false;
            BtnManualSync.Content = "Συγχρονισμός...";

            try
            {
                string localPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EuroStatementPdf");
                string localJsonPath = System.IO.Path.Combine(localPath, "JsonData");
                string drivePath = GetOneDrivePath();

                if (string.IsNullOrEmpty(drivePath)) return;
                if (!Directory.Exists(drivePath)) Directory.CreateDirectory(drivePath);

                string driveJsonPath = System.IO.Path.Combine(drivePath, "JsonData");
                if (!Directory.Exists(driveJsonPath)) Directory.CreateDirectory(driveJsonPath);
                if (!Directory.Exists(localJsonPath)) Directory.CreateDirectory(localJsonPath);

                var clientWin = Application.Current.Windows.OfType<ClientInformationWin>().FirstOrDefault();

                var localFiles = Directory.GetFiles(localPath);
                var driveFilesSet = new HashSet<string>(Directory.GetFiles(drivePath).Select(System.IO.Path.GetFileName));

                foreach (var file in localFiles)
                {
                    string fileName = System.IO.Path.GetFileName(file);
                    if (!driveFilesSet.Contains(fileName))
                    {
                        string destPath = System.IO.Path.Combine(drivePath, fileName);
                        if (clientWin != null) await ClientInformationWin.CopyWithRetryAsync(file, destPath);
                        else File.Copy(file, destPath, overwrite: true);
                    }
                }

                var localJsonFiles = Directory.GetFiles(localJsonPath);
                var driveJsonSet = new HashSet<string>(Directory.GetFiles(driveJsonPath).Select(System.IO.Path.GetFileName));

                foreach (var file in localJsonFiles)
                {
                    string fileName = System.IO.Path.GetFileName(file);
                    if (!driveJsonSet.Contains(fileName))
                    {
                        string destPath = System.IO.Path.Combine(driveJsonPath, fileName);
                        if (clientWin != null) await ClientInformationWin.CopyWithRetryAsync(file, destPath);
                        else File.Copy(file, destPath, overwrite: true);
                    }
                }

                var driveFiles = Directory.GetFiles(drivePath);
                var localFilesSet = new HashSet<string>(Directory.GetFiles(localPath).Select(System.IO.Path.GetFileName));

                foreach (var file in driveFiles)
                {
                    string fileName = System.IO.Path.GetFileName(file);
                    if (!localFilesSet.Contains(fileName))
                    {
                        string destPath = System.IO.Path.Combine(localPath, fileName);
                        if (clientWin != null) await ClientInformationWin.CopyWithRetryAsync(file, destPath);
                        else File.Copy(file, destPath, overwrite: true);
                    }
                }

                var driveJsonFiles = Directory.GetFiles(driveJsonPath);
                var localJsonSet = new HashSet<string>(Directory.GetFiles(localJsonPath).Select(System.IO.Path.GetFileName));

                foreach (var file in driveJsonFiles)
                {
                    string fileName = System.IO.Path.GetFileName(file);
                    if (!localJsonSet.Contains(fileName))
                    {
                        string destPath = System.IO.Path.Combine(localJsonPath, fileName);
                        if (clientWin != null) await ClientInformationWin.CopyWithRetryAsync(file, destPath);
                        else File.Copy(file, destPath, overwrite: true);
                    }
                }

                var mainWin = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                mainWin?.LoadPdfFiles();

                MessageBox.Show("Ο συγχρονισμός ολοκληρώθηκε!", "Sync finished", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Σφάλμα συγχρονισμού: " + ex.Message);
            }
            finally
            {
                BtnManualSync.IsEnabled = true;
                await CheckSyncStatusAsync();
            }
        }
        private void StatusPopup_Closed(object sender, EventArgs e)
        {
           syncPopUpIsClosed = true;
        }
    }
}
