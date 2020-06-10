using System;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Drawing;


using System.Windows.Forms;
using AForge.Video;
using System.Threading;
using System.IO;
using AForge.Video.DirectShow;
using System.Net;

namespace VideoStreamingServer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private int ImageQualityLevel = 1; /// quality level

        public IVideoSource _videoSource;

        private UdpSocketInteraction UdpInteraction;

        public MainWindow()
        {
            InitializeComponent();

            UdpInteraction = new UdpSocketInteraction();
        }

        private void Button_Start(object sender, RoutedEventArgs e)
        {
            var rectangle = new Rectangle();
            foreach (var screen in System.Windows.Forms.Screen.AllScreens)
            {
                rectangle = Rectangle.Union(rectangle, screen.Bounds);
                break;
            }

            _videoSource = new ScreenCaptureStream(rectangle);
            _videoSource.NewFrame += new NewFrameEventHandler(VideoCaptureDevice_NewFrame);

            _videoSource.Start();
        }


        private void VideoCaptureDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {

            Bitmap img = (Bitmap)eventArgs.Frame.Clone();

            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, (SendOrPostCallback)delegate
            {
                IntPtr hBitmap = img.GetHbitmap();
                System.Windows.Media.Imaging.BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                GC.Collect();

                Pic.Source = bitmapSource;
                SendByUdpSocket(bitmapSource);

                DeleteObject((IntPtr)hBitmap);
                img.Dispose();

            }, null);

            return;
        }

        private void SendByUdpSocket(BitmapSource image)
        {
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.QualityLevel = ImageQualityLevel;

            byte[] data;

            using (MemoryStream stream = new MemoryStream())
            {
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(stream);
                data = stream.ToArray();
                stream.Close();
            }

            UdpInteraction.SendData(data);
            Thread.Sleep(50);
        }

        private void Grid_ContextMenuClosing(object sender, System.Windows.Controls.ContextMenuEventArgs e)
        {
            /// avoid memory leak
            _videoSource.NewFrame -= VideoCaptureDevice_NewFrame;
            Environment.Exit(0);
        }
    }
}
