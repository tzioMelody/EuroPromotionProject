using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.VisualBasic;
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
                UpdateUIStatus(new SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString("#10B981")), allMissing);
            }
            else if (allMissing.Count == allLocal.Count)
            {
                UpdateUIStatus(new SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString("#EF4444")), allMissing);
            }
            else
            {
                UpdateUIStatus(new SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString("#F59E0B")), allMissing);
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

        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            StatusPopup.IsOpen = false;

            string userCode = Interaction.InputBox(
                "Παρακαλώ εισάγετε τον κωδικό σας:",
                "Εισαγωγή Κωδικού",
                "");

            if (string.IsNullOrWhiteSpace(userCode))
                return;

            if (userCode != "euro405534")
            {
                MessageBox.Show(
                    "Λάθος κωδικός.",
                    "Σφάλμα",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            try
            {
                ExportAllDataToExcel();

                MessageBox.Show(
                    "Η εξαγωγή ολοκληρώθηκε με επιτυχία!",
                    "Επιτυχία",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Σφάλμα κατά την εξαγωγή στο Excel:\n" + ex.Message,
                    "Σφάλμα",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        private void ExportAllDataToExcel()
        {
            string folderPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"EuroStatementPdf");
            string jsonFolderPath = System.IO.Path.Combine(folderPath,"JsonData");

            if (!Directory.Exists(folderPath))
            {
                MessageBox.Show("Δεν βρέθηκε ο φάκελος με τα αρχεία.","Σφάλμα",MessageBoxButton.OK,MessageBoxImage.Warning);
                return;
            }

            if (!Directory.Exists(jsonFolderPath))
            {
                MessageBox.Show("Δεν βρέθηκε ο φάκελος JsonData.","Σφάλμα",MessageBoxButton.OK,MessageBoxImage.Warning);
                return;
            }

            string excelPath = System.IO.Path.Combine(folderPath,$"Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Φαρμακεία");
                worksheet.Cell(1, 1).Value = "Επωνυμία Φαρμακείου";
                worksheet.Cell(1, 2).Value = "Πόλη";
                worksheet.Cell(1, 3).Value = "Τηλέφωνο";
                worksheet.Cell(1, 4).Value = "Email";
                worksheet.Cell(1, 5).Value = "Promoter";
                worksheet.Cell(1, 6).Value = "Παρουσίαση";
                worksheet.Cell(1, 7).Value = "Πωλήσεις";
                worksheet.Cell(1, 8).Value = "Πρόγραμμα";
                worksheet.Cell(1, 9).Value = "Πελάτης";
                worksheet.Cell(1, 10).Value = "Παρατηρήσεις";
                worksheet.Cell(1, 11).Value = "Σημαντικές Παρατηρήσεις";
                worksheet.Cell(1, 12).Value = "GDPR";
                worksheet.Cell(1, 13).Value = "Επεξεργασμένο";
                worksheet.Cell(1, 14).Value = "Ημερομηνία";

                var headerRange = worksheet.Range(1, 1, 1, 11);

                headerRange.Style.Font.Bold = true;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;


                string[] pdfFiles = Directory.GetFiles(folderPath, "*.pdf");
                int row = 2;

                foreach (string pdfPath in pdfFiles)
                {
                    string pdfFileName = System.IO.Path.GetFileName(pdfPath);

                    string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(pdfFileName);

                    string jsonPath = System.IO.Path.Combine(jsonFolderPath,fileNameWithoutExtension + ".json");

                    if (!File.Exists(jsonPath))
                        continue;

                    try
                    {
                        string jsonString = File.ReadAllText(jsonPath);

                        var data =
                            JsonSerializer.Deserialize<ClientInformationWin.StatementData>(
                                jsonString);

                        if (data == null)
                            continue;

                        worksheet.Cell(row, 1).Value = data.Pharmacy ?? "";
                        worksheet.Cell(row, 2).Value = data.City ?? "";
                        worksheet.Cell(row, 3).Value = data.Phone ?? "";
                        worksheet.Cell(row, 4).Value = data.Email ?? "";
                        worksheet.Cell(row, 5).Value = data.Promoter ?? "";
                        worksheet.Cell(row, 6).Value = data.Presenter ?? "";
                        worksheet.Cell(row, 7).Value = data.SalesPerson ?? "";

                        string program = data.finalProgram;

                        if (string.IsNullOrWhiteSpace(program))
                            program = data.Program;


                        worksheet.Cell(row, 8).Value = program ?? "";

                        worksheet.Cell(row, 9).Value = data.Client ?? "";
                        worksheet.Cell(row, 10).Value = data.Notes ?? "";
                        worksheet.Cell(row, 11).Value = data.ImportantNotes ?? "";
                        worksheet.Cell(row, 12).Value = data.IsConsentChecked ? "ΝΑΙ" : "ΟΧΙ";

                        bool isEdited = pdfFileName.IndexOf( "_Edited", StringComparison.OrdinalIgnoreCase) >= 0;

                        worksheet.Cell(row, 13).Value = isEdited ? "ΝΑΙ" : "ΟΧΙ";

                        DateTime creationDate = File.GetCreationTime(pdfPath);

                        worksheet.Cell(row, 14).Value = creationDate;
                        worksheet.Cell(row, 14).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                        row++;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(
                            $"Σφάλμα στο αρχείο {pdfFileName}: {ex.Message}");
                    }
                }

                if (row > 2)
                {
                    var dataRange = worksheet.Range(1, 1, row - 1, 11);

                    dataRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    dataRange.Style.Alignment.WrapText = true;
                    worksheet.Columns().AdjustToContents();
                    worksheet.Column(5).Width = 40;
                    worksheet.Column(6).Width = 40;

                    worksheet.SheetView.FreezeRows(1);

                    worksheet.Range(1,1,row - 1,11).SetAutoFilter();
                }

                workbook.SaveAs(excelPath);
            }

            if (File.Exists(excelPath))
            {
                Process.Start(new ProcessStartInfo(excelPath){UseShellExecute = true});
            }
        }
    }
}
