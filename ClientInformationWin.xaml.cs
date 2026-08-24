using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EuroPromotionProject
{
    /// <summary>
    /// Interaction logic for ClientInformationWin.xaml
    /// </summary>
    public partial class ClientInformationWin : Window
    {
        Window mainWindow;

        private StatementFile _fileToEdit;
        public string Pharmacy => TxtPharmacy.Text;
        public string City => TxtCity.Text;
        public string Phone => TxtPhone.Text;
        public string Email => TxtEmail.Text;
        public string Notes => TxtNotes.Text;
        public string ImportantNotes => TxtImportantNotes.Text;
        public bool IsConsentChecked => ChkConsent.IsChecked == true;

        public List<StatementFile> allFiles = new List<StatementFile>();

        public class StatementData
        {
            public string Pharmacy { get; set; }
            public string City { get; set; }
            public string Phone { get; set; }
            public string Email { get; set; }
            public string Promoter { get; set; }
            public string Presenter { get; set; }
            public string SalesPerson { get; set; }
            public string Program { get; set; }
            public string finalProgram { get; set; }
            public string Client { get; set; }
            public string Notes { get; set; }
            public string ImportantNotes { get; set; }
            public bool IsConsentChecked { get; set; }
        }
        public class StatementFile
        {
            public string FileName { get; set; }
            public string FullPath { get; set; }
            public string DateCreated { get; set; }
            public string ImportantNotes { get; set; }
        }
        public void InitWindow(Window mw)
        {
            mainWindow = mw;
        }

        public ClientInformationWin()
        {
            InitializeComponent();

            //Για να μην κουνιέται όλη η φόρμα όταν πάει κάποιος να υπογράψει 
            Stylus.SetIsFlicksEnabled(SignCanvas, false);
            Stylus.SetIsTapFeedbackEnabled(SignCanvas, false);
            Stylus.SetIsPressAndHoldEnabled(SignCanvas, false);

            string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Ονόματα έκθεσης Οκτώβρη.xlsx");
            DataTable dt = ImportExceltoDatatable(filePath);
            var itemsListPP = dt.AsEnumerable().Select(row => row["Presentation / Promotion"]?.ToString()).Where(text => !string.IsNullOrEmpty(text)).ToList();
            var itemsListSales = dt.AsEnumerable().Select(row => row["Sales"]?.ToString()).Where(text => !string.IsNullOrEmpty(text)).ToList();

            itemsListPP.Insert(0, "");
            itemsListSales.Insert(0, "");

            ComboPromoter.ItemsSource = null;
            ComboPromoter.ItemsSource = itemsListPP;

            ComboPresentation.ItemsSource = null;
            ComboPresentation.ItemsSource = itemsListPP;

            ComboSales.ItemsSource = null;
            ComboSales.ItemsSource = itemsListSales;

            List<string> programList = new List<string>
            {
                "-- ΕΠΙΛΕΞΤΕ ΠΡΟΓΡΑΜΜΑ --",
                "EUROMEDICA TWO",
                "D",
                "F/P",
                "Λ",
                "O",
                "S",
                "ΑΛΛΟ"
            };

            ComboProgram.ItemsSource = programList;
            ComboProgram.SelectedIndex = 0;


            List<string> clientList = new List<string>
            {
                "--ΕΠΙΛΕΞΤΕ ΝΑΙ / ΟΧΙ--",
                "ΝΑΙ", "ΟΧΙ"
            };

            ComboClient.ItemsSource = clientList;
            ComboClient.SelectedIndex = 0;
        }
        public ClientInformationWin(StatementFile fileToEdit) : this()
        {
            _fileToEdit = fileToEdit;
            LoadDataForEdit();
        }
        private void PromoterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboPromoter.SelectedIndex > -1)
            {
                ComboPromoterPlaceHolder.Visibility = Visibility.Collapsed;
            }
            TxtPharmacy.Focus();
        }

        private void PresentationCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboPresentation.SelectedIndex > -1)
            {
                ComboPresenterPlaceHolder.Visibility = Visibility.Collapsed;
            }
        }

        private void SalesPersonCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboSales.SelectedIndex > -1)
            {
                ComboSalesPlaceHolder.Visibility = Visibility.Collapsed;
            }
        }
        private void ProgramCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboProgram.SelectedItem == null) return;

            string selectedProgram = ComboProgram.SelectedItem.ToString();

            if (selectedProgram == "ΑΛΛΟ")
            {
                TxtOtherProgramBlock.Visibility = Visibility.Visible;
                TxtOtherProgram.Visibility = Visibility.Visible;
            }

            if (selectedProgram == "EUROMEDICA TWO")
            {
                ComboClient.SelectedItem = "ΝΑΙ";
            }
            else
            {
                ComboClient.SelectedItem = "ΟΧΙ";

            }
        }

        private void ClientCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TxtPhone_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void TxtPharmacy_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter || String.IsNullOrEmpty(TxtPharmacy.Text))
                return;
            if (e.Key == Key.Enter)
                TxtCity.Focus();
            e.Handled = true;
        }
        private void TxtCity_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter || String.IsNullOrEmpty(TxtPharmacy.Text))
                return;
            if (e.Key == Key.Enter)
                TxtPhone.Focus();
            e.Handled = true;
        }

        private void TxtPhone_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter || String.IsNullOrEmpty(TxtPharmacy.Text))
                return;
            if (e.Key == Key.Enter)
                TxtEmail.Focus();
            e.Handled = true;
        }
        public static DataTable ImportExceltoDatatable(string filePath)
        {
            DataTable dt = new DataTable();
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return dt;
            try
            {
                using (XLWorkbook workBook = new XLWorkbook(filePath))
                {

                    IXLWorksheet workSheet = workBook.Worksheet(1);

                    int columnNum = workSheet.ColumnsUsed().Count();
                    bool firstRow = true;
                    foreach (IXLRow row in workSheet.Rows())
                    {
                        //Use the first row to add columns to DataTable.
                        if (firstRow)
                        {
                            for (int col = 0; col < columnNum; col++)
                                dt.Columns.Add(row.Cell(col + 1).Value.ToString());

                            firstRow = false;
                        }
                        else
                        {
                            //Add rows to DataTable.
                            dt.Rows.Add();


                            for (int col = 0; col < columnNum; col++)
                            {
                                dt.Rows[dt.Rows.Count - 1][col] = row.Cell(col + 1).Value.ToString();

                            }


                        }
                    }

                    return dt;
                }
            }
            catch (Exception ex)
            {
                return dt;
            }
        }

        private void LoadDataForEdit()
        {
            if (_fileToEdit == null) return;

            signPanel.Visibility = Visibility.Collapsed;

            string jsonPath = GetJsonPathForPdf(_fileToEdit.FullPath);

            if (File.Exists(jsonPath))
            {
                try
                {
                    string jsonString = File.ReadAllText(jsonPath);
                    var data = JsonSerializer.Deserialize<StatementData>(jsonString);

                    if (data != null)
                    {
                        TxtPharmacy.Text = data.Pharmacy;
                        TxtCity.Text = data.City;
                        TxtPhone.Text = data.Phone;
                        TxtEmail.Text = data.Email;
                        TxtNotes.Text = data.Notes;
                        TxtImportantNotes.Text = data.ImportantNotes;
                        ChkConsent.IsChecked = data.IsConsentChecked;

                        ComboPromoter.SelectedItem = data.Promoter;
                        ComboPresentation.SelectedItem = data.Presenter;
                        ComboSales.SelectedItem = data.SalesPerson;
                        ComboProgram.SelectedItem = data.Program;
                        ComboClient.SelectedItem = data.Client;

                        if (!string.IsNullOrEmpty(data.Promoter) && ComboPromoterPlaceHolder != null)
                            ComboPromoterPlaceHolder.Visibility = Visibility.Collapsed;

                        if (!string.IsNullOrEmpty(data.Presenter) && ComboPresenterPlaceHolder != null)
                            ComboPresenterPlaceHolder.Visibility = Visibility.Collapsed;

                        if (!string.IsNullOrEmpty(data.SalesPerson) && ComboSalesPlaceHolder != null)
                            ComboSalesPlaceHolder.Visibility = Visibility.Collapsed;

                        if (!string.IsNullOrEmpty(data.Program) && ComboProgram.SelectedItem.ToString() == "ΑΛΛΟ")
                        {
                            TxtOtherProgramBlock.Visibility = Visibility.Visible;
                            TxtOtherProgram.Text = data.finalProgram;
                        }

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Αποτυχία φόρτωσης δεδομένων επεξεργασίας: " + ex.Message);
                }
            }
            else
            {
                string fileNameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(_fileToEdit.FileName);
                var parts = fileNameWithoutExt.Split('_');
                if (parts.Length > 0)
                {
                    TxtPharmacy.Text = parts[0];
                }
            }
        }
        private static bool IsTextAllowed(string text)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("[^0-9]+");
            return !regex.IsMatch(text);
        }

        private void QuickNote_Click(object sender, RoutedEventArgs e)
        {
            //Για τις έτοιμες απαντήσεις, όταν πατάμε το κουμπί, να προστίθεται το κείμενο στο TextBox των Σημειώσεων.
            //Αν υπάρχει ήδη κείμενο να προστίθεται με κόμμα και κενό.
            if (sender is Button btn)
            {
                string noteText = "";

                if (btn.Content is TextBlock tb)
                {
                    noteText = tb.Text;
                }
                else
                {
                    noteText = btn.Content.ToString();
                }

                noteText = noteText.Replace("+ ", "");

                if (string.IsNullOrWhiteSpace(TxtNotes.Text))
                    TxtNotes.Text = noteText;
                else
                    TxtNotes.Text += ", " + noteText;

                TxtNotes.Focus();
                TxtNotes.SelectionStart = TxtNotes.Text.Length;
            }
        }


        private async void BtnFinish_Click(object sender, RoutedEventArgs e)
        {
            bool isPharmacyEmpty = string.IsNullOrWhiteSpace(TxtPharmacy.Text);
            bool isPhoneEmpty = string.IsNullOrWhiteSpace(TxtPhone.Text);

            if (isPharmacyEmpty || isPhoneEmpty)
            {
                MessageBox.Show("Tο Φαρμακείο και το Τηλέφωνο είναι υποχρεωτικά πεδία!");
                return;
            }
            if ((SignCanvas.Strokes.Count == 0 && _fileToEdit == null && IsConsentChecked) || (SignCanvas.Strokes.Count != 0 && !IsConsentChecked))
            {
                MessageBox.Show("Για την επιτυχής καταχώρηση των στοιχείων του πελάτη πρέπει να έχει υπογράψει και συνεναίσει στην κατχώρηση των στοιχειών.");
                return;
            }

            // Απενεργοποιούμε το κουμπί όσο γίνεται το ανέβασμα για να μην πατηθεί δύο φορές και δημιουργηθούν διπλά αρχεία.
            BtnFinish.IsEnabled = false;
            try
            {
                await SaveStatement();
            }
            finally
            {
                BtnFinish.IsEnabled = true;
            }
        }


        public async Task SaveStatement()
        {
            try
            {

                //Save the signature as an image
                string statementPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EuroStatementPdf");
                if (!Directory.Exists(statementPath))
                {
                    Directory.CreateDirectory(statementPath);
                }

                string originalProgram = ComboProgram.SelectedItem?.ToString();

                string finalProgram = (originalProgram == "ΑΛΛΟ") ? TxtOtherProgram.Text.Trim() : originalProgram;

                if (string.IsNullOrEmpty(finalProgram) || finalProgram == "--ΕΠΙΛΕΞΤΕ ΠΡΟΓΡΑΜΜΑ--")
                {
                    finalProgram = "Μη καθορισμένο";
                }

                string cleanPharmacyName = string.Concat(TxtPharmacy.Text.Split(System.IO.Path.GetInvalidFileNameChars()));

                string StatementFileName = "";


                if (_fileToEdit != null)
                {

                    if (!_fileToEdit.FileName.Contains("_Edited"))
                    {
                        string originalFileNameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(_fileToEdit.FileName);

                        StatementFileName = $"{originalFileNameWithoutExt}_Edited.pdf";
                    }
                    else
                    {
                        if (File.Exists(_fileToEdit.FullPath))
                        {
                            File.Delete(_fileToEdit.FullPath);
                        }

                        string oldJsonPath = GetJsonPathForPdf(_fileToEdit.FullPath);
                        if (File.Exists(oldJsonPath))
                        {
                            File.Delete(oldJsonPath);
                        }

                        // Κρατάμε το ίδιο όνομα αρχείου για να μην αλλάξει
                        StatementFileName = _fileToEdit.FileName;
                    }

                }
                else
                {
                    // Προσθέτουμε το όνομα της συσκευής στη ονομασία τόυ αρχείου ώστε να μην υπάρχει overwrite αν καταχωρηθούν ταυτόχρονα 2 ή παραπάνω φόρμες για το ίδιο φαρμακείο μέσα στο ίδιο λεπτό.
                    string deviceId = Environment.MachineName;
                    StatementFileName = $"{cleanPharmacyName}_{DateTime.Now:yyyyMMdd_HHmmss}_{deviceId}.pdf";
                }

                string fullDestPath = System.IO.Path.Combine(statementPath, StatementFileName);
                PDFGenerator generator = new PDFGenerator();
                generator.CreatePdfWithSignature(
                    fullDestPath,
                    Pharmacy,
                    ComboPromoter.Text,
                    City,
                    Phone,
                    Email,
                    originalProgram,
                    finalProgram,
                    ComboClient.Text,
                    ComboPresentation.Text,
                    ComboSales.Text,
                    Notes,
                    ImportantNotes,
                    IsConsentChecked,
                    SignCanvas
                );

                var statementData = new StatementData
                {
                    Pharmacy = Pharmacy,
                    City = City,
                    Phone = Phone,
                    Email = Email,
                    Promoter = ComboPromoter.Text,
                    Presenter = ComboPresentation.Text,
                    SalesPerson = ComboSales.Text,
                    Program = ComboProgram.Text,
                    finalProgram = TxtOtherProgram.Text,
                    Client = ComboClient.Text,
                    Notes = Notes,
                    ImportantNotes = ImportantNotes,
                    IsConsentChecked = IsConsentChecked
                };

                string jsonPath = GetJsonPathForPdf(fullDestPath);
                string jsonString = JsonSerializer.Serialize(statementData, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(jsonPath, jsonString);

                try
                {
                    string oneDrivePath = Environment.GetEnvironmentVariable("OneDrive") ?? Environment.GetEnvironmentVariable("OneDriveCommercial");

                    if (!string.IsNullOrEmpty(oneDrivePath))
                    {
                        string targetFolder = System.IO.Path.Combine(oneDrivePath, "EuroStatementsPdf");

                        if (!Directory.Exists(targetFolder))
                        {
                            Directory.CreateDirectory(targetFolder);
                        }

                        string fileName = System.IO.Path.GetFileName(fullDestPath);

                        // Αντιγραφή PDF & JSON στο OneDriveμ με retry γιατί ο OneDrive client μπορεί να κλειδώνει προσωρινά το αρχείο ενώ κάνει sync.
                        string targetJsonFolder = System.IO.Path.Combine(targetFolder, "JsonData");
                        if (!Directory.Exists(targetJsonFolder))
                        {
                            Directory.CreateDirectory(targetJsonFolder);
                        }

                        await CopyWithRetryAsync(fullDestPath, System.IO.Path.Combine(targetFolder, fileName));
                        await CopyWithRetryAsync(jsonPath, System.IO.Path.Combine(targetJsonFolder, System.IO.Path.GetFileName(jsonPath)));

                        MessageBox.Show("Η αναφορά σώθηκε επιτυχώς στον φάκελο OneDrive και συγχρονίζεται!",
                                        "OneDrive Sync", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        throw new Exception("Δεν βρέθηκε εγκατεστημένος φάκελος OneDrive στον υπολογιστή.");
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Η αναφορά σώθηκε τοπικά, αλλά απέτυχε η μεταφορά στο OneDrive.\nΣφάλμα: " + ex.Message,
                                    "Προσοχή", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

                mainWindow.LoadPdfFiles();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Σφάλμα κατά την αποθήκευση της αναφοράς: " + ex.Message,
                                "Σφάλμα", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        // Αντιγράφει ένα αρχείο με επαναληπτικές προσπάθειες γιατί το OneDrive client μπορεί να κρατάει προσωρινό lock στο φάκελο προορισμού ενώ κάνει sync. Θα μας χρειαστεί και για το να δείχνουμε αν έχουν ανέβει όλα ή όχι. 
        public static async Task CopyWithRetryAsync(string sourcePath, string destPath, int maxAttempts = 3)
        {
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    File.Copy(sourcePath, destPath, overwrite: true);
                    return;
                }
                catch (IOException) when (attempt < maxAttempts)
                {
                    await Task.Delay(500 * attempt);
                }
            }
        }
        private static string GetJsonPathForPdf(string pdfFullPath)
        {
            string folder = System.IO.Path.GetDirectoryName(pdfFullPath);
            string jsonFolder = System.IO.Path.Combine(folder, "JsonData");
            if (!Directory.Exists(jsonFolder))
                Directory.CreateDirectory(jsonFolder);

            string fileNameNoExt = System.IO.Path.GetFileNameWithoutExtension(pdfFullPath);
            return System.IO.Path.Combine(jsonFolder, fileNameNoExt + ".json");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (mainWindow != null)
            {
                mainWindow.WindowState = this.WindowState;
                mainWindow.Show();
            }
        }
    }
}