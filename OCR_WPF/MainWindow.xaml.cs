using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OCR_Controller;
using System.IO;
using OCR_Model;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace OCR_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const String DefaultPath = @"c:\Users\Lukas\work\Projects\Konica\requirements\";

        private readonly Processer _processer;
        private readonly OcrDocumentModel _ocrDocumentModel;
        private OcrDocument _document;

        public MainWindow()
        {
            InitializeComponent();
                        
            _ocrDocumentModel = new OcrDocumentModel();
            this._processer = new Processer();
            this.DataContext = _ocrDocumentModel;

        }

        private void loadImageBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            //set Default path
            if (Directory.Exists(DefaultPath)){
                openFileDialog.InitialDirectory = DefaultPath;
            }
            
            // Set filter for file extension and default file extension 
            openFileDialog.DefaultExt = ".jpg";
            openFileDialog.Filter = "Image Files|*.jpg;*.JPG;*.jpeg;*.JPEG;*.png;*.PNG;*.bmp;*.BMP|All Files(*.*)|*.*";

            if (openFileDialog.ShowDialog() == true) {
                LoadImage( openFileDialog.FileName);                           
            }
        }

        private void LoadImage(String path) {
            BitmapImage src = new BitmapImage();
            src.BeginInit();
            src.UriSource = new Uri(path, UriKind.Absolute);
            src.EndInit();
            inputImage.Source = src;
            inputImage.Stretch = Stretch.Uniform;

            //set path to txt box
            imagePathTxtBox.Text = path;

            //save file related data
            _document = _processer.StoreFileRelatedDocumentData(path, Path.GetFileName(path), File.GetCreationTime(path));                                 
        }

        private void processButton_Click(object sender, RoutedEventArgs e)
        {
           _ocrDocumentModel.Add(_processer.Process(_document));
        }

    }
}
