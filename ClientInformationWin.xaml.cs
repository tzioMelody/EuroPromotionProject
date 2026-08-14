using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
        public string Pharmacy => TxtPharmacy.Text;
        public string City => TxtCity.Text;
        public string Phone => TxtPhone.Text;
        public string Email => TxtEmail.Text;
        public string Program => ComboProgram.SelectionBoxItem?.ToString();
        public string Client => ComboClient.SelectionBoxItem?.ToString();
        public string Notes => TxtNotes.Text;
        public bool IsConsentChecked => ChkConsent.IsChecked == true;
        public List<StatementFile> allFiles = new List<StatementFile>();

        public class StatementFile
        {
            public string FileName { get; set; }
            public string FullPath { get; set; }
            public string DateCreated { get; set; }
        }

        public void InitWindow(Window mw)
        {
            mainWindow = mw;
        }

        public ClientInformationWin()
        {
            InitializeComponent();

            string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Ονόματα έκθεσης Οκτώβρη.xlsx");
            DataTable dt = ImportExceltoDatatable(filePath);
            var itemsListPP = dt.AsEnumerable().Select(row => row["Presentation / Promotion"]?.ToString()).Where(text => !string.IsNullOrEmpty(text)).ToList();
            var itemsListSales = dt.AsEnumerable().Select(row => row["Sales"]?.ToString()).Where(text => !string.IsNullOrEmpty(text)).ToList();

            ComboPromoter.ItemsSource = null;
            ComboPromoter.ItemsSource = itemsListPP;

            ComboPresentation.ItemsSource = null;
            ComboPresentation.ItemsSource = itemsListPP;

            ComboSales.ItemsSource = null;
            ComboSales.ItemsSource = itemsListSales;

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


        private void BtnFinish_Click(object sender, RoutedEventArgs e)
        {
            bool isPromoterEmpty = ComboPromoter.SelectedIndex <= 0;
            bool isPharmacyEmpty = string.IsNullOrWhiteSpace(TxtPharmacy.Text);
            bool isPhoneEmpty = string.IsNullOrWhiteSpace(TxtPhone.Text);

            if (isPromoterEmpty || isPharmacyEmpty || isPhoneEmpty)
            {
                MessageBox.Show("Ο Promoter, το Φαρμακείο και το Τηλέφωνο είναι υποχρεωτικά πεδία!");
                return;
            }
            if (SignCanvas.Strokes.Count == 0)
            {
                MessageBox.Show("Παρακαλώ βάλτε την υπογραφή σας.");
                return;
            }

            SaveStatement();
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

                string finalProgram = (ComboProgram.SelectedItem == ItemOther)
                            ? TxtOtherProgram.Text.Trim()
                            : Program;

                if (string.IsNullOrEmpty(finalProgram)) finalProgram = "Μη καθορισμένο";
                string StatementFileName = $"{TxtPharmacy.Text}_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
                string fullDestPath = System.IO.Path.Combine(statementPath, StatementFileName);

                PDFGenerator generator = new PDFGenerator();
                generator.CreatePdfWithSignature(
                    fullDestPath,
                    Pharmacy,      // Αντί για infoWin.TxtPharmacy.Text
                    ComboPromoter.Text,      // Χρησιμοποιεί το SelectionBoxItem αυτόματα
                    City,          // Αντί για infoWin.TxtCity.Text
                    Phone,         // Αντί για infoWin.TxtPhone.Text
                    Email,         // Αντί για infoWin.TxtEmail.Text
                    finalProgram,       // Αντί για infoWin.TxtProgram.Text
                    Client,        // Χρησιμοποιεί το SelectionBoxItem (ΝΑΙ/ΟΧΙ)
                    ComboPresentation.Text,  // Χρησιμοποιεί το SelectionBoxItem (ΝΑΙ/ΟΧΙ)
                    ComboSales.Text,         // Χρησιμοποιεί το SelectionBoxItem (ΝΑΙ/ΟΧΙ)
                    Notes,         // Αντί για infoWin.TxtNotes.Text
                    IsConsentChecked,
                    SignCanvas
                );

                try
                {
                    // Εντοπισμός του τοπικού φακέλου OneDrive του χρήστη
                    string oneDrivePath = Environment.GetEnvironmentVariable("OneDrive");

                    if (string.IsNullOrEmpty(oneDrivePath))
                    {
                        // Εναλλακτική αν πρόκειται για OneDrive for Business
                        oneDrivePath = Environment.GetEnvironmentVariable("OneDriveCommercial");
                    }

                    if (!string.IsNullOrEmpty(oneDrivePath))
                    {
                        // Ορίζουμε τον υποφάκελο στον οποίο θέλουμε να σωθεί (π.χ. OneDrive\Reports)
                        string targetFolder = System.IO.Path.Combine(oneDrivePath, "EuroStatementsPdf");

                        // Δημιουργία του φακέλου αν δεν υπάρχει
                        if (!Directory.Exists(targetFolder))
                        {
                            Directory.CreateDirectory(targetFolder);
                        }

                        string fileName = System.IO.Path.GetFileName(fullDestPath);
                        string destinationPathInOneDrive = System.IO.Path.Combine(targetFolder, fileName);

                        // Αντιγραφή του αρχείου στον φάκελο του OneDrive
                        File.Copy(fullDestPath, destinationPathInOneDrive, overwrite: true);

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
