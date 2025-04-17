using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Forms;
using System.Drawing;

namespace AnimeWP
{
    public partial class MainWindow : Window
    {
        private int currentFrame = 0;
        private GifBitmapDecoder decoder;
        private DispatcherTimer timer;
        private NotifyIcon trayIcon;

        public MainWindow()
        {
            InitializeComponent();
            this.Topmost = true;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string gifPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sample.gif");

            if (!System.IO.File.Exists(gifPath))
            {
                System.Windows.MessageBox.Show("GIF dosyası bulunamadı: " + gifPath, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            LoadGif(gifPath);
            InitTrayIcon();
        }

        private void LoadGif(string path)
        {
            Uri gifUri = new Uri(path, UriKind.Absolute);
            decoder = new GifBitmapDecoder(gifUri, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);

            gifImage.Source = decoder.Frames[0];

            timer?.Stop();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(60);
            timer.Tick += Timer_Tick;
            timer.Start();

            // pencere boyutunu gif'e göre ayarla
            double gifWidth = decoder.Frames[0].PixelWidth;
            double gifHeight = decoder.Frames[0].PixelHeight;
            this.Width = gifWidth;
            this.Height = gifHeight;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            currentFrame = (currentFrame + 1) % decoder.Frames.Count;
            gifImage.Source = decoder.Frames[currentFrame];
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double scaleStep = 0.05;
            double factor = e.Delta > 0 ? (1 + scaleStep) : (1 - scaleStep);

            double newWidth = this.Width * factor;
            newWidth = Math.Max(100, Math.Min(2000, newWidth));

            double aspectRatio = this.Width / this.Height;
            this.Width = newWidth;
            this.Height = newWidth / aspectRatio;
        }

        private void InitTrayIcon()
        {
            trayIcon = new NotifyIcon();
            string iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appicon.ico");

            if (System.IO.File.Exists(iconPath))
            {
                trayIcon.Icon = new Icon(iconPath);
            }

            trayIcon.Visible = true;
            trayIcon.Text = "AnimeWP";

            var menu = new ContextMenuStrip();
            menu.Items.Add("GIF Değiştir", null, (s, e) =>
            {
                Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
                ofd.Filter = "GIF Dosyası|*.gif";
                if (ofd.ShowDialog() == true)
                {
                    LoadGif(ofd.FileName);
                }
            });
            menu.Items.Add("Göster", null, (s, e) => { this.Show(); this.WindowState = WindowState.Normal; });
            menu.Items.Add("Çıkış", null, (s, e) =>
            {
                trayIcon.Visible = false;
                System.Windows.Application.Current.Shutdown();
            });

            trayIcon.ContextMenuStrip = menu;

            trayIcon.DoubleClick += (s, e) =>
            {
                this.Show();
                this.WindowState = WindowState.Normal;
            };
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            if (this.WindowState == WindowState.Minimized)
            {
                this.Hide();
            }
        }
    }
}