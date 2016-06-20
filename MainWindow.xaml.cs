using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace KiepVisitorRegistration
{
    public partial class MainWindow : Window
    {
        private const string CACHEFILE = "Visitors.txt";
        private const string LOGFILE = "KiepVisitorViewer.log";
        private int[] CATCH_KEYCODES = { 107, 111 };

#if !DEBUG
        // TODO Change to your website running the ASP.NET registration form
        private const string URL = "http://localhost:1908/KiepVisitorViewer.aspx";
#else
        private const string URL = "http://localhost:1908/KiepVisitorViewer.aspx";
#endif

        private delegate void DummyDelegate();

        private int currentPage = 1;
        private int numberOfPages = 1;
        private string currentText = "";

        public MainWindow()
        {
            InitializeComponent();

            // Cannot debug when application has topmost
#if !DEBUG
            this.Topmost = true;
#endif

            // Wait 1 second before subscribing to keypresses
            WaitSubscribeKeypresses();

            // Log startup
            Log("Start");

            // Apply rotation animation to status image
            DoubleAnimation da = new DoubleAnimation(0, 360, new Duration(TimeSpan.FromSeconds(3)));
            RotateTransform rt = new RotateTransform();
            imgStatus.RenderTransform = rt;
            imgStatus.RenderTransformOrigin = new Point(0.5, 0.5);
            da.RepeatBehavior = RepeatBehavior.Forever;
            rt.BeginAnimation(RotateTransform.AngleProperty, da);

            // Start reading
            ShowPage(TryReadFromFile(CACHEFILE), currentPage);
            TryReadFromWeb(URL);
        }

        private void WaitSubscribeKeypresses()
        {
            try
            {
                Timer timer = new Timer(1000);
                timer.Elapsed += delegate
                {
                    this.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    (DummyDelegate)
                    delegate 
                    {
                        timer.Enabled = false;

                        // Block keys from being received by other applications
                        List<int> blockedKeys = new List<int>(CATCH_KEYCODES);
                        LowLevelKeyboardHook.Instance.SetBlockedKeys(blockedKeys);

                        // Subscribe to low level keypress events
                        LowLevelKeyboardHook.Instance.KeyboardHookEvent += new LowLevelKeyboardHook.KeyboardHookEventHandler(Instance_KeyboardHookEvent);
                    });
                };
                timer.Enabled = true;
            }
            catch (Exception) { }
        }

        void Instance_KeyboardHookEvent(int keycode)
        {
            if (new List<int>(CATCH_KEYCODES).Contains(keycode))
            {
                Log("Keypress\t" + keycode);
                Click();
            }
        }

        private void MouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            Log("MouseClick");
            Click();
        }

        private void Click()
        {
            currentPage++;
            if (currentPage > numberOfPages)
            {
                Log("Exit");
                Application.Current.Shutdown();
            }
            else
            {
                Log("Next page\t" + currentPage);
                ShowPage(currentText, currentPage);
            }
        }

        private void ShowPage(string text, int page)
        {
            currentText = text;
            currentPage = page;
            
            string[] pages = Regex.Split(text, "\r\n<page-break>\r\n");

            numberOfPages = pages.Length;

            if (currentPage > numberOfPages)
            {
                currentPage = numberOfPages;
            }

            ShowText(pages[currentPage - 1]);
        }

        private void ShowText(string text)
        {
            // Clear screen
            spViewer.Children.Clear();

            string[] textblocks = Regex.Split(text, "\r\n<new-block>\r\n");
            int i = 0;
            foreach (string textblock in textblocks)
            {
                TextBox tbTextBlock = new TextBox();
                tbTextBlock.PreviewMouseDown += MouseDownHandler;
                tbTextBlock.IsReadOnly = true;
                tbTextBlock.BorderThickness = new Thickness(0);
                tbTextBlock.Foreground = Brushes.Black;
                tbTextBlock.Text = textblock;

                if (i % 2 == 0)
                {
                    tbTextBlock.Background = Brushes.Transparent;
                }
                else
                {
                    tbTextBlock.Background = Brushes.LightGray;
                }
                i++;

                // Add text block to screen
                spViewer.Children.Add(tbTextBlock);
            }
        }

        private string TryReadFromFile(string filename)
        {
            string result = "";
            try
            {
                string baseDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                string filenameandpath = baseDir + "\\" + filename;
                if (File.Exists(filenameandpath))
                {
                    StreamReader cachefile = File.OpenText(filenameandpath);
                    result = cachefile.ReadToEnd();
                    cachefile.Close();
                }
            }
            catch (Exception) { }

            return result;
        }

        private void TryReadFromWeb(string url)
        {
            try
            {
                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += new DoWorkEventHandler(bw_DoWork);
                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
                bw.RunWorkerAsync(url);
            }
            catch (Exception) { }
        }

        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            string result = (string)e.Result;
            if (result != "")
            {
                ShowPage(result, currentPage);
                WriteToFile(CACHEFILE, result);
                imgStatus.Source = new BitmapImage(new Uri("Ok.png", UriKind.Relative));
                imgStatus.RenderTransform = null;
            }
            else
            {
                imgStatus.Source = new BitmapImage(new Uri("Error.png", UriKind.Relative));
                imgStatus.RenderTransform = null;
            }
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = "";
            try 
            {
                string url = (string)e.Argument;
                WebRequest webRequest = WebRequest.Create(url);
                webRequest.Proxy = null;
                webRequest.Timeout = 5000;
                Stream responseStream = webRequest.GetResponse().GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                string responseFromServer = reader.ReadToEnd();
                e.Result = responseFromServer;	
            }
            catch (Exception) { }
        }

        private void WriteToFile(string filename, string text)
        {
            try
            {
                if (text != "")
                {
                    string baseDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                    StreamWriter cachefile = File.CreateText(baseDir + "\\" + filename);
                    cachefile.Write(text);
                    cachefile.Close();
                }
            }
            catch (Exception) { }
        }

        private void Log(string text)
        {
            try
            {
                if (text != "")
                {
                    string baseDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                    StreamWriter cachefile = File.AppendText(baseDir + "\\" + LOGFILE);
                    cachefile.WriteLine(DateTime.Now + "\t" + text);
                    cachefile.Close();
                }
            }
            catch (Exception) { }
        }
    }
}
