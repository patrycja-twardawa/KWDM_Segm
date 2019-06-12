using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using Spire.Doc;
using System.Windows.Forms;
using System.Threading;

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
        public List<string> lista_instancji;
        public List<string> lista_maski;
        public string badanie;
        public string info;
        public string path = System.AppDomain.CurrentDomain.BaseDirectory + "ORTHANC";
        public string actual_img = "1";
        public string actual_instance;
        public string segm_method = "segmCV";
        System.Windows.Point currentPoint = new System.Windows.Point();
        public int fl = 0;
        public int flaga_maski = 0;
        

        public MainWindow()
        {
            InitializeComponent();
            paintSurface.Visibility = Visibility.Hidden;
 
            //OrthStart();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string name = (sender as System.Windows.Controls.Button).Name.ToString();

            if (name == "AktualizujButton") { AktualizujMatlab(); } //Proba();
            else if (name == "DodajButton") { Dodaj(); }
            else if (name == "SegmentujButton") { SegmentujMatlab(); }
            else if (name == "pedzelButton") { Pedzel(); }
            else if(name == "SendServer") { wyslijDICOM();  }
            else if (name == "Raport") { CreateDocument(); }
            else if (name == "LinijkaButton") { Linijka(); }
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

        private void Linijka()
        {

            paintSurface.AddHandler(InkCanvas.MouseDownEvent, new MouseButtonEventHandler(Canvas_MouseDown), true);
            attribute.Width = 10;
            attribute.Height = 10;
           
        



        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string list_value = (sender as System.Windows.Controls.ListBox).SelectedIndex.ToString();
            if (int.Parse(list_value) != -1)
            {
                string list_string = (sender as System.Windows.Controls.ListBox).SelectedValue.ToString();
                string list_name = (sender as System.Windows.Controls.ListBox).Name.ToString();

                if (list_name == "ListaPacjentow") { WczytajBadaniaMatlab(list_value); }
                else if (list_name == "ListaBadan") { WczytajSerieMatlab(list_string); }
                else if (list_name == "ListaSerii") { list_value = (sender as System.Windows.Controls.ListBox).SelectedValue.ToString(); WczytajInstancjeMatlab(list_value); flaga_maski = 0; }
                else if (list_name == "ListaMaski") { list_value = (sender as System.Windows.Controls.ListBox).SelectedValue.ToString(); WczytajInstancjeMatlab(list_value); flaga_maski = 1; }
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


        double[] pointDistance = new double[4];
        double pixelSpacing = 2.0;
        int ind = 0;

        private void Canvas_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ind < 4)
            {
                if (e.ButtonState == MouseButtonState.Pressed)
                {
                    currentPoint = e.GetPosition(this);


                    pointDistance[ind] = currentPoint.X;
                    pointDistance[ind + 1] = currentPoint.Y;
                    ind += 2;


                }
            }

            if (ind == 4)
            {

                double distance = (Math.Sqrt((Math.Pow(pointDistance[0] - pointDistance[2], 2)
                 + Math.Pow(pointDistance[1] - pointDistance[3], 2)))) * pixelSpacing;

                distance = Math.Round(distance,2);

                System.Windows.MessageBox.Show("Długość: " + distance.ToString()+ " mm"); // w mm juz 

                ind = 0;
                paintSurface.Strokes.Clear();

            }

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

            string path_save = null;
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    path_save = fbd.SelectedPath;
                    btm.Save(fbd.SelectedPath + "\\Wynik-Segmentacja.png", jpegCodec, encoderParams); //zmieniona ścieżka
                    System.Windows.MessageBox.Show("Obraz zapisany poprawnie (Wynik-Segmentacja.png)!", "Zapis wyniku segmentacji");
                }
            }

            btm.Dispose();
        }

        private void CreateDocument()
        {
            try
            {
                Document doc = new Document(); //tworzenie nowego dokumentu Word
                Spire.Doc.Section section = doc.AddSection(); //dodawanie nowej sekcji
                Spire.Doc.Documents.Paragraph para = section.AddParagraph(); //nagłówek

                para.Format.BeforeAutoSpacing = false; //spacing przed i po
                para.Format.BeforeSpacing = 15;
                para.Format.AfterAutoSpacing = false;
                para.Format.AfterSpacing = 15;

                Spire.Doc.Fields.TextRange rangeH = para.AppendText("Raport: program do segmentacji naskórka"); //dołączenie tekstu
                rangeH.CharacterFormat.FontSize = 25;
                rangeH.CharacterFormat.Bold = true;

                Spire.Doc.Documents.Paragraph Para = section.AddParagraph(); //dodanie paragrafu

                Para.Format.BeforeAutoSpacing = false; //spacing
                Para.Format.BeforeSpacing = 15;
                Para.Format.AfterAutoSpacing = false;
                Para.Format.AfterSpacing = 15;

                Spire.Doc.Fields.TextRange range = Para.AppendText("1. Obraz naskórka przed segmentacją");
                range.CharacterFormat.FontSize = 20;
                range.CharacterFormat.Bold = true;

                Spire.Doc.Documents.Paragraph Imageparagraph = section.AddParagraph(); //obrazek przed segmentacją 
                System.Drawing.Image image = System.Drawing.Image.FromFile(path + "\\temp\\" + actual_img + ".png");
                Spire.Doc.Fields.DocPicture picture = Imageparagraph.AppendPicture(image);
                picture.Height = 320;
                picture.Width = 320;
                Imageparagraph.Format.HorizontalAlignment = Spire.Doc.Documents.HorizontalAlignment.Center;

                Spire.Doc.Documents.Paragraph Para1 = section.AddParagraph(); //obrazek po segmentacji 
                Para1.Format.BeforeAutoSpacing = false;
                Para1.Format.BeforeSpacing = 15;
                Para1.Format.AfterAutoSpacing = false;
                Para1.Format.AfterSpacing = 15;
                Spire.Doc.Fields.TextRange range1 = Para1.AppendText("2. Obraz naskórka po segmentacji ");
                range1.CharacterFormat.FontSize = 20;
                range1.CharacterFormat.Bold = true;

                Spire.Doc.Documents.Paragraph Imageparagraph1 = section.AddParagraph();
                System.Drawing.Image image1 = System.Drawing.Image.FromFile(path + "\\temp\\" + segm_method + ".png");
                Spire.Doc.Fields.DocPicture picture1 = Imageparagraph1.AppendPicture(image1);
                picture1.Height = 300;
                picture1.Width = 320;
                Imageparagraph1.Format.HorizontalAlignment = Spire.Doc.Documents.HorizontalAlignment.Center;

                Spire.Doc.Documents.Paragraph Para2 = section.AddParagraph(); //wykres intensywności
                Para2.Format.BeforeAutoSpacing = false;
                Para2.Format.BeforeSpacing = 15;
                Para2.Format.AfterAutoSpacing = false;
                Para2.Format.AfterSpacing = 15;

                Spire.Doc.Fields.TextRange range2 = Para2.AppendText("3. Wykres intensywności");
                range2.CharacterFormat.FontSize = 20;
                range2.CharacterFormat.Bold = true;

                Spire.Doc.Documents.Paragraph Imageparagraph2 = section.AddParagraph();
                System.Drawing.Image image2 = System.Drawing.Image.FromFile(path + "\\temp\\Histogram.png");
                Spire.Doc.Fields.DocPicture picture2 = Imageparagraph2.AppendPicture(image2);
                picture2.Height = 320;
                picture2.Width = 320;
                Imageparagraph2.Format.HorizontalAlignment = Spire.Doc.Documents.HorizontalAlignment.Center;

                Spire.Doc.Fields.TextRange range22 = Para2.AppendText("\nOś pozioma - intensywność pikseli; oś pionowa - ilość pikseli.");
                range22.CharacterFormat.FontSize = 10;

                Spire.Doc.Documents.Paragraph Para30 = section.AddParagraph(); //informacje o obrazie
                Para30.Format.BeforeAutoSpacing = false;
                Para30.Format.BeforeSpacing = 15;
                Para30.Format.AfterAutoSpacing = false;
                Para30.Format.AfterSpacing = 15;

                Spire.Doc.Fields.TextRange range30 = Para30.AppendText("4. Informacje o obrazie");
                range30.CharacterFormat.FontSize = 20;
                range30.CharacterFormat.Bold = true;

                Spire.Doc.Documents.Paragraph ParaInfo = section.AddParagraph();
                Spire.Doc.Fields.TextRange rangeInfo = ParaInfo.AppendText(Environment.NewLine + info);
                rangeInfo.CharacterFormat.FontSize = 10;

                Spire.Doc.Documents.Paragraph Para3 = section.AddParagraph(); //notatki 
                Para3.Format.BeforeAutoSpacing = false;
                Para3.Format.BeforeSpacing = 15;
                Para3.Format.AfterAutoSpacing = false;
                Para3.Format.AfterSpacing = 15;

                Spire.Doc.Fields.TextRange range3 = Para3.AppendText("4. Notatki");
                range3.CharacterFormat.FontSize = 20;
                range3.CharacterFormat.Bold = true;

                Spire.Doc.Documents.Paragraph ParaNotatki = section.AddParagraph();
                Spire.Doc.Fields.TextRange rangeNotatki = ParaNotatki.AppendText(Environment.NewLine + notatkitb.Text);
                rangeNotatki.CharacterFormat.FontSize = 10;

                Spire.Doc.Documents.Paragraph Para5 = section.AddParagraph(); //informacje dodatkowe
                Para5.Format.BeforeAutoSpacing = false;
                Para5.Format.BeforeSpacing = 15;
                Para5.Format.AfterAutoSpacing = false;
                Para5.Format.AfterSpacing = 15;

                Spire.Doc.Fields.TextRange range4 = Para5.AppendText("5. Informacje Dodatkowe ");
                range4.CharacterFormat.FontSize = 20;
                range4.CharacterFormat.Bold = true;

                Spire.Doc.Documents.Paragraph ParaInfo2 = section.AddParagraph();
                Spire.Doc.Fields.TextRange rangeInfo2 = ParaInfo2.AppendText(Environment.NewLine + infotb.Text);
                rangeInfo2.CharacterFormat.FontSize = 10;

                string path_save = null;
                using (var fbd = new FolderBrowserDialog())
                {
                    DialogResult result = fbd.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        path_save = fbd.SelectedPath;
                    }
                }

                //doc.SaveToFile(@"d:\\MyWord.docx", Spire.Doc.FileFormat.Docx);
                doc.SaveToFile(path_save + "\\Raport-Segmentacja.pdf", FileFormat.PDF); //zapis i uruchomienie
                System.Windows.MessageBox.Show("Dokument utworzony poprawnie (Raport-Segmentacja.pdf)!", "Raport PDF");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
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

            info = "Brak danych.";
            DaneObrazoweLabel.Content = "Brak danych.";
            ListaPacjentow.ItemsSource = lista_pacjentow;
            ListaBadan.ItemsSource = null;
            ListaSerii.ItemsSource = null;
            ListaInstancji.ItemsSource = null;
            ImageO1.Source = null;
            ImageO2.Source = null;
            image.Source = null;
            IntensywnoscLabel.Visibility = Visibility.Hidden;
            IloscLabel.Visibility = Visibility.Hidden;
            ZapiszButton.IsEnabled = false;
            Raport.IsEnabled = false;
            SendServer.IsEnabled = false;
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

            info = "Brak danych.";
            DaneObrazoweLabel.Content = "Brak danych.";
            ListaBadan.ItemsSource = lista_badan;
            ListaSerii.ItemsSource = null;
            ListaInstancji.ItemsSource = null;
            ImageO1.Source = null;
            ImageO2.Source = null;
            image.Source = null;
            IntensywnoscLabel.Visibility = Visibility.Hidden;
            IloscLabel.Visibility = Visibility.Hidden;
            ZapiszButton.IsEnabled = false;
            Raport.IsEnabled = false;
            SendServer.IsEnabled = false;
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

            info = "Brak danych.";
            DaneObrazoweLabel.Content = "Brak danych.";
            ListaSerii.ItemsSource = lista_serii;
            ListaInstancji.ItemsSource = null;
            ImageO1.Source = null;
            ImageO2.Source = null;
            image.Source = null;
            IntensywnoscLabel.Visibility = Visibility.Hidden;
            IloscLabel.Visibility = Visibility.Hidden;
            ZapiszButton.IsEnabled = false;
            Raport.IsEnabled = false;
            SendServer.IsEnabled = false;


            matlab.Execute(@"cd " + path);
            object result2 = null; // wyjście default
            matlab.Feval("OrthSeriesSEGM", 1, out result2, list_string); //wywołanie funkcji
            object[] res2 = result2 as object[];
            OrthStop();

            res2[0] = RemoveLastEscape(res2[0].ToString());
            lista_maski = new List<string>(Regex.Split(res2[0].ToString(), "\n"));
            ListaMaski.ItemsSource = lista_maski;
        }

        private void WczytajInstancjeMatlab(string list_val_studyid)
        {
            MLApp.MLApp matlab = MatlabInitialize();
            OrthStart();

           matlab.Execute(@"cd " + path);
           object result = null; // wyjście default
                                
           matlab.Feval("OrthInstances", 2, out result, badanie, list_val_studyid);
           object[] res = result as object[];
           OrthStop();

           res[0] = RemoveLastEscape(res[0].ToString());
           lista_instancji = new List<string>(Regex.Split(res[0].ToString(), "\n"));

           info = Regex.Replace(res[1].ToString(), @"[_]", string.Empty); //jak w pacjentach
           info = RemoveLastEscape(info);
           DaneObrazoweLabel.Content = info;
           ListaInstancji.ItemsSource = lista_instancji;

            ImageO1.Source = null;
            ImageO2.Source = null;
            image.Source = null;
            IntensywnoscLabel.Visibility = Visibility.Hidden;
            IloscLabel.Visibility = Visibility.Hidden;
            ZapiszButton.IsEnabled = false;
            Raport.IsEnabled = false;
            SendServer.IsEnabled = false;
        }

        private void WczytajObrazMatlab(string list_string)
        {
            if (flaga_maski == 0)
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
                DispImage(actual_img, 0); //wyświetlenie obrazu oryginalnego
                image.Source = null;
                IntensywnoscLabel.Visibility = Visibility.Hidden;
                IloscLabel.Visibility = Visibility.Hidden;
                ZapiszButton.IsEnabled = false;
                Raport.IsEnabled = false;
                SendServer.IsEnabled = false;
            }
            else if(flaga_maski == 1)
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
                matlab.Feval("OrthancDownloadInstance", 1, out result, lista_instancji[0]); //wywołanie funkcji});
                OrthStop();
                matlab.Feval("DicomConvert", 0, out result2, lista_instancji[0]); //przekształcenie na format png - I TAK ZOSTAWIĆ, OBRAZ BĘDZIE SŁUŻYĆ TYLKO DO WYŚWIETLANIA W OKNIE PROGRAMU
                                                                           //});
                                                                           //t.Wait();
                                                                           //t.Dispose();
                DispImage(actual_img, 0); //wyświetlenie obrazu oryginalnego

                /****/
    
                OrthStart();

                matlab.Execute(@"cd " + path);
                object result21 = null; // wyjście default
                object result22 = null;
                //Task t = Task.Run(() => {
                matlab.Feval("OrthancDownloadInstance", 1, out result21, lista_instancji[1]); //wywołanie funkcji});
                OrthStop();
                matlab.Feval("DicomConvertMask", 0, out result22, lista_instancji[1]); //przekształcenie na format png - I TAK ZOSTAWIĆ, OBRAZ BĘDZIE SŁUŻYĆ TYLKO DO WYŚWIETLANIA W OKNIE PROGRAMU
                                                                           //});
                                                                           //t.Wait();
                string nm = "2";                                                         //t.Dispose();
                DispImage(nm, 1); //wyświetlenie obrazu oryginalnego


                image.Source = null;
                IntensywnoscLabel.Visibility = Visibility.Hidden;
                IloscLabel.Visibility = Visibility.Hidden;
                ZapiszButton.IsEnabled = false;
                Raport.IsEnabled = false;
                SendServer.IsEnabled = false;


            }
        }

        public void SegmentujMatlab()
        {
            string file_path = path + "\\temp";
            string im_path = "\\" + actual_instance + ".dcm"; //+ actual_img + ".png"; //path + "\\temp\\" + actual_img + ".png";
            object result = null; // wyjście default

            AktualizujButton.IsEnabled = false;
            DodajButton.IsEnabled = false;
            ListaPacjentow.IsEnabled = false;
            ListaBadan.IsEnabled = false;
            ListaSerii.IsEnabled = false;
            ListaInstancji.IsEnabled = false;
            pedzelButton.IsEnabled = false;
            GumkaButton.IsEnabled = false;
            LinijkaButton.IsEnabled = false;
            LupaButton.IsEnabled = false;
            SegmentujButton.IsEnabled = false;
            Thread.Sleep(100);

            MLApp.MLApp matlab = MatlabInitialize();
            matlab.Execute(@"cd " + path);
            matlab.Feval("segmentacja", 0, out result, file_path, im_path); //wywołanie funkcji

            DispImage(segm_method, 1);
            DispImage("Histogram", 2);
            IntensywnoscLabel.Visibility = Visibility.Visible;
            IloscLabel.Visibility = Visibility.Visible;
            ZapiszButton.IsEnabled = true;
            Raport.IsEnabled = true;
            SendServer.IsEnabled = true;
            AktualizujButton.IsEnabled = true;
            DodajButton.IsEnabled = true;
            ListaPacjentow.IsEnabled = true;
            ListaBadan.IsEnabled = true;
            ListaSerii.IsEnabled = true;
            ListaInstancji.IsEnabled = true;
            pedzelButton.IsEnabled = true;
            GumkaButton.IsEnabled = true;
            LinijkaButton.IsEnabled = true;
            LupaButton.IsEnabled = true;
            SegmentujButton.IsEnabled = true;
        }

        private void wyslijDICOM()//(file_path, segmentation, DICOMfilename)
        {
            string file_path = "segmCV.png";
            string path_n = path + "\\temp";
            string im_path = actual_instance + ".dcm"; //+ actual_img + ".png"; //path + "\\temp\\" + actual_img + ".png";
            object result = null; // wyjście default
            string dicom_name = actual_instance + "_segm.dcm";
            string mask_name = actual_instance + "_segmMask.dcm";

            MLApp.MLApp matlab = MatlabInitialize();
            //send dicom
            matlab.Execute(@"cd " + path); //lokalizacja funkcji MATLAB-a
            matlab.Feval("OrthancSendDicom", 0, out result, path_n, im_path, file_path, dicom_name, mask_name, badanie); //wywołanie funkcji
        }

        // FUNKCJE KONSOLOWE

        private void Dodaj()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog1 = new Microsoft.Win32.OpenFileDialog();
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
            System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo("CMD.exe", strCmdText);

            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;
            procStartInfo.CreateNoWindow = true;
            procStartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            p = new System.Diagnostics.Process();
            p.StartInfo = procStartInfo;
            p.Start(); //p = System.Diagnostics.Process.Start("CMD.exe", strCmdText); //widoczna konsola
        }

        private void OrthStop()
        {
            //p.CloseMainWindow();
            p.Close();
        }

        private void DispImage(string name_img, int lr)
        {
            if (lr == 0) { ImageO1.Source = null; }
            else if (lr == 1) { ImageO2.Source = null; }
            else { image.Source = null; } //lr = 2

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
            else { image.Source = bitmap; }

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
            System.Windows.MessageBox.Show(result, "Operacja na serwerze DICOM - wynik");
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


            //send dicom
            matlab.Execute(@"cd " + path); //lokalizacja funkcji MATLAB-a
            matlab.Feval("OrthancSend", 1, out result, URL, dicom_name, badanie); //wywołanie funkcji
            object[] res = result as object[]; //wyniki
            string reply = res[0].ToString();
            return reply;

        }

        private void DodajMatlab()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog1 = new Microsoft.Win32.OpenFileDialog();
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
                System.Windows.MessageBox.Show(reply, "Wysyłanie plików na serwer DICOM - log");
            }
        }

        private void OverlayImg_Checked(object sender, RoutedEventArgs e)
        {
            string file_path = path + "\\temp";
            string dicom_path = "\\" + actual_instance + ".dcm"; //+ actual_img + ".png"; //path + "\\temp\\" + actual_img + ".png";
            object result = null; // wyjście default
            string im_path = "\\segmCV.png";

            MLApp.MLApp matlab = MatlabInitialize();
            matlab.Execute(@"cd " + path);
            matlab.Feval("OverlayImg", 0, out result, file_path, dicom_path, im_path); //wywołanie funkcji

            DispImage("maskOver", 1);
        }

        private void OverlayImg_Unchecked(object sender, RoutedEventArgs e)
        {
            DispImage(segm_method, 1);
        }
    }
}
