using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using System.Threading;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;

namespace KWDM_Segm
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public static class ExtensionMethods
    {
        private static Action EmptyDelegate = delegate () { };

        public static void Refresh(this UIElement uiElement)
        {
            uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }
    }

    public partial class MainWindow : Window
    {

        public System.Diagnostics.Process p;
        public List<string> lista_pacjentow;
        public List<string> lista_id;
        public List<string> lista_serii;
        public string badanie;
        public string path = System.AppDomain.CurrentDomain.BaseDirectory + "ORTHANC";
        public string actual_img = "1";
        public string actual_instance;
        public string segm_method = "segmCV";
        System.Windows.Point currentPoint = new System.Windows.Point();
        public int fl = 0;
        

        public MainWindow()
        {
            InitializeComponent();
            paintSurface.Visibility = Visibility.Hidden;
 
            //OrthStart();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string name = (sender as Button).Name.ToString();

            if (name == "AktualizujButton") { AktualizujMatlab(); } //Proba();
            else if (name == "DodajButton") { Dodaj(); }
            else if (name == "SegmentujButton") { SegmentujMatlab(); }
            else if (name == "pedzelButton") { Pedzel(); }
            else if(name == "SendServer") { wyslijDICOM();  }
        }

        private void Pedzel()
        {
            paintSurface.Visibility = Visibility.Visible;

            string file_path = path + "\\temp";

            string file_name = path + "\\temp\\" + "segmCV" + ".png"; //ew. Label.Content = file_name;
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(file_name);
            bitmap.EndInit();

            ImageBrush ib = new ImageBrush();
            ib.ImageSource = bitmap;
            //ib.ImageSource = new BitmapImage(new Uri("D://KWDM_proj//KWDM_Segm//KWDM_Segm//bin//Debug//ORTHANC//temp//segmCV.png", UriKind.RelativeOrAbsolute));
            paintSurface.Background = ib;

            /*GC.Collect();
            DirectoryInfo dir = new DirectoryInfo(path + "\\temp");
            dir.Refresh();

            string file_name = path + "\\temp\\" + "segmCV" + ".png"; //ew. Label.Content = file_name;
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(file_name);
            bitmap.EndInit();*/
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string list_value = (sender as ListBox).SelectedIndex.ToString();
            if (int.Parse(list_value) != -1)
            {
                string list_string = (sender as ListBox).SelectedValue.ToString();
                string list_name = (sender as ListBox).Name.ToString();

                if (list_name == "ListaPacjentow") { WczytajBadaniaMatlab(list_value); }
                else if (list_name == "ListaBadan") { WczytajSerieMatlab(list_string); }
                else if (list_name == "ListaSerii") { WczytajInstancjeMatlab(list_value); }
                else { WczytajObrazMatlab(list_string); } //ListaInstancji
            }
        }

        private void Button_Click_Gumka(object sender, RoutedEventArgs e)
        {
            paintSurface.EditingMode = InkCanvasEditingMode.EraseByPoint;
        }

        private void Button_Click_Pedzel(object sender, RoutedEventArgs e)
        {
            paintSurface.EditingMode = InkCanvasEditingMode.Ink;
        }

        private void Canvas_MouseDown_1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                currentPoint = e.GetPosition(this);
        }

        private void Canvas_MouseMove_1(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //if (e.LeftButton == MouseButtonState.Pressed)
            //{
            //    Line line = new Line();
            //    line.Stroke = SystemColors.WindowFrameBrush;
            //    line.X1 = currentPoint.X;
            //    line.Y1 = currentPoint.Y;
            //    line.X2 = e.GetPosition(this).X;
            //    line.Y2 = e.GetPosition(this).Y;
            //    currentPoint = e.GetPosition(this);
            //    paintSurface.Children.Add(line);
            //}
        }

        private void Button_Click_Zapisz(object sender, RoutedEventArgs e)
        {
            double width = paintSurface.ActualWidth;
            double height = paintSurface.ActualHeight;
            RenderTargetBitmap bmpCopied = new RenderTargetBitmap((int)Math.Round(width), (int)Math.Round(height), 96, 96, PixelFormats.Default);
            DrawingVisual dv = new DrawingVisual();

            using (DrawingContext dc = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(paintSurface);
                dc.DrawRectangle(vb, null, new Rect(new System.Windows.Point(), new System.Windows.Size(width, height)));
            }

            bmpCopied.Render(dv);

            System.Drawing.Bitmap bitmap;

             using (MemoryStream outStream = new MemoryStream())
             {
                 // from System.Media.BitmapImage to System.Drawing.Bitmap 
                 BitmapEncoder enc = new BmpBitmapEncoder();
                 enc.Frames.Add(BitmapFrame.Create(bmpCopied));
                 enc.Save(outStream);
                 bitmap = new System.Drawing.Bitmap(outStream);
             }

             EncoderParameter qualityParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 85L);
             ImageCodecInfo jpegCodec = getEncoderInfo("image/png"); // Jpeg image codec

             if (jpegCodec == null)
                 return;

             EncoderParameters encoderParams = new EncoderParameters(1);
             encoderParams.Param[0] = qualityParam;
             System.Drawing.Bitmap btm = new Bitmap(bitmap);
             bitmap.Dispose();
             btm.Save(path + "\\temp\\segmCV.png", jpegCodec, encoderParams); //zmieniona ścieżka
             btm.Dispose();
        }

               //FUNKCJE GŁÓWNE - OBSŁUGA ZDARZEŃ -----------------------------------------------------------------------------------------------------------------------------------

        // FUNKCJE MATLAB-a

        private void AktualizujMatlab()
        {
            MLApp.MLApp matlab = MatlabInitialize();
            OrthStart();

            matlab.Execute(@"cd " + path);
            object result = null; // wyjście default
            matlab.Feval("OrthPatients", 2, out result); //wywołanie funkcji
            object[] res = result as object[];
            OrthStop();

            //DaneObrazoweLabel.Content = res[0].ToString(); //TEST W LABELU, CZY COKOLWIEK SIĘ POJAWIA - powinny się pojawiać id pacjentów
            string wynik = Regex.Replace(res[0].ToString(), @"[_]", string.Empty); //usuwanie dodatkowych znaków (dla wyrównania tablic charów w 2015a - nie ma klasy string)
            wynik = RemoveLastEscape(wynik);
            lista_pacjentow = new List<string>(Regex.Split(wynik, "\n")); 
            lista_id = new List<string>(Regex.Split(res[1].ToString(), "\n"));

            ListaPacjentow.ItemsSource = lista_pacjentow;
            ListaBadan.ItemsSource = null;
            ListaSerii.ItemsSource = null;
            ListaInstancji.ItemsSource = null;
            ImageO1.Source = null;
            ImageO2.Source = null;
        }

        private void WczytajBadaniaMatlab(string list_val_pacjid)
        {
            string pacjent_id = lista_id[int.Parse(list_val_pacjid)]; //numer wyboru na liście

            MLApp.MLApp matlab = MatlabInitialize();
            OrthStart();

            matlab.Execute(@"cd " + path);
            object result = null; // wyjście default
            matlab.Feval("OrthStudies", 1, out result, pacjent_id); //wywołanie funkcji
            object[] res = result as object[];
            OrthStop();

            res[0] = RemoveLastEscape(res[0].ToString());
            List<string> lista_badan = new List<string>(Regex.Split(res[0].ToString(), "\n"));

            ListaBadan.ItemsSource = lista_badan;
            ListaSerii.ItemsSource = null;
            ListaInstancji.ItemsSource = null;
            ImageO1.Source = null;
            ImageO2.Source = null;
        }

        private void WczytajSerieMatlab(string list_string)
        {
            badanie = list_string;
            MLApp.MLApp matlab = MatlabInitialize();
            OrthStart();

            matlab.Execute(@"cd " + path);
            object result = null; // wyjście default
            matlab.Feval("OrthSeries", 1, out result, list_string); //wywołanie funkcji
            object[] res = result as object[];
            OrthStop();

            res[0] = RemoveLastEscape(res[0].ToString());
            lista_serii = new List<string>(Regex.Split(res[0].ToString(), "\n"));

            ListaSerii.ItemsSource = lista_serii;
            ListaInstancji.ItemsSource = null;
            ImageO1.Source = null;
            ImageO2.Source = null;
        }

        private void WczytajInstancjeMatlab(string list_val_studyid)
        {
            MLApp.MLApp matlab = MatlabInitialize();
            OrthStart();

            matlab.Execute(@"cd " + path);
            object result = null; // wyjście default
            matlab.Feval("OrthInstances", 1, out result, badanie, int.Parse(list_val_studyid) + 1); //wywołanie funkcji
            object[] res = result as object[];
            OrthStop();

            res[0] = RemoveLastEscape(res[0].ToString());
            List<string> lista_instancji = new List<string>(Regex.Split(res[0].ToString(), "\n"));

            ListaInstancji.ItemsSource = lista_instancji;
            ImageO1.Source = null;
            ImageO2.Source = null;
        }

        private void WczytajObrazMatlab(string list_string)
        {
            if (Directory.Exists(path + "\\temp"))
            {
                System.IO.DirectoryInfo di = new DirectoryInfo(path + "\\temp");
                foreach (FileInfo file in di.GetFiles()) { file.Delete(); }
                di.Delete();
            }
            Directory.CreateDirectory(path + "\\temp");

            MLApp.MLApp matlab = MatlabInitialize();
            actual_instance = list_string;
            OrthStart();

            matlab.Execute(@"cd " + path);
            object result = null; // wyjście default
            object result2 = null;
            //Task t = Task.Run(() => {
            matlab.Feval("OrthancDownloadInstance", 1, out result, list_string); //wywołanie funkcji});
            OrthStop();
            matlab.Feval("DicomConvert", 0, out result2, list_string); //przekształcenie na format png - I TAK ZOSTAWIĆ, OBRAZ BĘDZIE SŁUŻYĆ TYLKO DO WYŚWIETLANIA W OKNIE PROGRAMU
            //});
            //t.Wait();
            //t.Dispose();

            int lr = 0; //2 okna
            DispImage(actual_img, lr);
        }

        public void SegmentujMatlab()
        {
            string file_path = path + "\\temp";
            string im_path = "\\" + actual_instance + ".dcm"; //+ actual_img + ".png"; //path + "\\temp\\" + actual_img + ".png";
            object result = null; // wyjście default

            MLApp.MLApp matlab = MatlabInitialize();
            matlab.Execute(@"cd " + path);
            matlab.Feval("segmentacja", 0, out result, file_path, im_path); //wywołanie funkcji

            int lr = 1; //prawe okno
            DispImage(segm_method, lr);
        }

        private void wyslijDICOM()//(file_path, segmentation, DICOMfilename)
        {
            string file_path = "segmCV.png";
            string path_n = path + "\\temp";
            string im_path = actual_instance + ".dcm"; //+ actual_img + ".png"; //path + "\\temp\\" + actual_img + ".png";
            object result = null; // wyjście default

            MLApp.MLApp matlab = MatlabInitialize();
            matlab.Execute(@"cd " + path);
            matlab.Feval("OrthancSend", 0, out result, path_n, file_path, im_path); //wywołanie funkcji
        }

        // FUNKCJE KONSOLOWE

        private void Dodaj()
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            string selectedFileName = null;

            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "Pliki DICOM (*.dcm)|*.dcm;"; //średniki między rozszerzeniami
            openFileDialog1.FilterIndex = 0;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == true)
            {
                selectedFileName = openFileDialog1.FileName;
            }

            if (selectedFileName != null)
            {
                string strCmdText = "/c cd ORTHANC\\exe && storescu -aec ORTHANC -aet ARCHIWUM localhost 4242 -v " + 
                    selectedFileName;
                OrthExecute(strCmdText);
            }
        }

        private void Aktualizuj()
        {
            string strCmdText = "/c cd ORTHANC\\exe && findscu -aet ARCHIWUM -aec ORTHANC -P -k 8,52=\"PATIENT\"" +
                " -k 10,10=\"*\" localhost 4242 -v ";
            string output = OrthExecute(strCmdText, true); //TODO: wyświetla cały komunikat serwera, trzeba wyłowić ID
        }
 
        //FUNKCJE POMOCNICZE --------------------------------------------------------------------------------------------------------------------------------------------------

        private void OrthStart()
        {
            string strCmdText = "/c cd ORTHANC\\exe && Orthanc.exe orth.json";
            p = System.Diagnostics.Process.Start("CMD.exe", strCmdText);
        }

        private void OrthStop()
        {
            p.CloseMainWindow();
            p.Close();
        }

        private void DispImage(string name_img, int lr)
        {
            if (lr == 0)
            {
                ImageO1.Source = null;
            }
            else if (lr == 1)
            {
                ImageO2.Source = null;
            }
            UpdateLayout();
            GC.Collect();
            DirectoryInfo dir = new DirectoryInfo(path + "\\temp");
            dir.Refresh();

            string file_name = path + "\\temp\\" + name_img + ".png"; //ew. Label.Content = file_name;
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(file_name);
            bitmap.EndInit();

            if (lr == 0) { ImageO1.Source = bitmap; }
            else if (lr == 1) { ImageO2.Source = bitmap; }
            else
            {
                ImageO1.Source = bitmap;
                ImageO2.Source = bitmap;
            }

            //ImageO1.Refresh();
            //ImageO2.Refresh();
        }

        private string RemoveLastEscape(string temp)
        {
            if (temp.Substring(temp.Length - 1) == '\n'.ToString())
            {
                temp = temp.Remove(temp.Length - 1);
            }
            return temp;
        }

        private ImageCodecInfo getEncoderInfo(string mimeType)
        {
            // Get image codecs for all image formats
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            // Find the correct image codec
            for (int i = 0; i < codecs.Length; i++)
                if (codecs[i].MimeType == mimeType)
                    return codecs[i];
            return null;
        }

        // FUNKCJE POMOCNICZE DLA MATLABA

        private MLApp.MLApp MatlabInitialize()
        {
            Type MatlabType = Type.GetTypeFromProgID("Matlab.Desktop.Application");
            MLApp.MLApp matlab = (MLApp.MLApp)Activator.CreateInstance(MatlabType);
            return matlab;
        }

        // FUNKCJE POMOCNICZE DLA KONSOLI

        private void OrthExecute(string strCmdText)
        {
            OrthStart();
            System.Diagnostics.Process d = System.Diagnostics.Process.Start("CMD.exe", strCmdText);            
            //Sprawdz(d);
            d.WaitForExit();

            //d.CloseMainWindow();
            d.Close();
            OrthStop();
        }

        private string OrthExecute(string strCmdText, bool t)
        {
            OrthStart();
            System.Diagnostics.Process d = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo dStartInfo = new System.Diagnostics.ProcessStartInfo();
            dStartInfo.UseShellExecute = false;
            dStartInfo.RedirectStandardOutput = true;
            dStartInfo.FileName = "CMD.exe";
            dStartInfo.Arguments = strCmdText;
            d.StartInfo = dStartInfo;
            d.Start();

            //Sprawdz(d);
            string output = d.StandardOutput.ReadToEnd();
            d.WaitForExit();

            d.CloseMainWindow();
            d.Close();
            OrthStop();
            return output;
        }

        //TEST -----------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void Sprawdz(Process o)
        {
         string result;
         if (o.ExitCode == 0) { result = "Mały krok dla DICOMa, wielki skok dla studenta. :)"; }
         else { result = "Vanitas vanitatum et omnia vanitas. Kod: " + o.ExitCode.ToString() + ". :("; }
         MessageBox.Show(result, "Operacja na serwerze DICOM - wynik");
        }

        private void Proba()
        {
            Type MatlabType = Type.GetTypeFromProgID("Matlab.Desktop.Application");
            MLApp.MLApp matlab = (MLApp.MLApp)Activator.CreateInstance(MatlabType);
            string path = System.AppDomain.CurrentDomain.BaseDirectory; //C:\\Users\\hp...\\bin\\Debug

            matlab.Execute(@"cd " + path); //C:\Users\hp\Desktop\KWDM_PROJEKT\KWDM_Segm\KWDM_Segm\bin\Debug"); //(@"cd c:\temp\example"); //lokalizacja funkcji MATLAB-a
            object result = null; // wyjście default
            matlab.Feval("myfunc", 2, out result, 3.14, 42.0, "world"); //wywołanie funkcji
            object[] res = result as object[]; //wyświetlenie wyników

            DaneObrazoweLabel.Content = res[1].ToString();
            //tx2.Text = res[1].ToString();
        }

        private string MatlabExecuteSend(string URL, string dicom_name) //WYSYŁANIE PLIKÓW NA SERWER
        {
            Type MatlabType = Type.GetTypeFromProgID("Matlab.Desktop.Application");
            MLApp.MLApp matlab = (MLApp.MLApp)Activator.CreateInstance(MatlabType);

            string path = System.AppDomain.CurrentDomain.BaseDirectory + "\\ORTHANC";
            object result = null; // wyjście default

            matlab.Execute(@"cd " + path); //lokalizacja funkcji MATLAB-a
            matlab.Feval("OrthancSend", 1, out result, URL, dicom_name); //wywołanie funkcji
            object[] res = result as object[]; //wyniki
            string reply = res[0].ToString();
            return reply;
        }

        private void DodajMatlab()
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            string selectedFileName = null;

            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "Pliki DICOM (*.dcm)|*.dcm;"; //średniki między rozszerzeniami
            openFileDialog1.FilterIndex = 0;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == true)
            {
                selectedFileName = openFileDialog1.FileName;
            }

            if (selectedFileName != null)
            {
                string URL = "http://localhost:8042";
                OrthStart();
                string reply = MatlabExecuteSend(URL, selectedFileName);
                OrthStop();
                MessageBox.Show(reply, "Wysyłanie plików na serwer DICOM - log");
            }
        }
    }
}
