using AForge.Video;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Drawing;
using System.IO;
using AForge.Video.DirectShow;

namespace VideoStreamingUser
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private UdpSocketInteraction udpSocket;

        public MainWindow()
        {
            InitializeComponent();

            udpSocket = new UdpSocketInteraction();

        }

        private void Button_Start_Recive(object sender, RoutedEventArgs e)
        {
            var rectangle = new Rectangle();
            foreach (var screen in System.Windows.Forms.Screen.AllScreens)
            {
                rectangle = Rectangle.Union(rectangle, screen.Bounds);
                break;
            }

            Thread videoSource = new Thread(VideoSource_NewFrame);
            videoSource.Start();
        }

        private void VideoSource_NewFrame()
        {
            while (true)
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, (SendOrPostCallback)delegate
                {
                    var imageSource = udpListen();
                    if (imageSource != null) Pic.Source = imageSource;

                }, null);
            }
        }

        private BitmapSource udpListen()
        {
           
            byte[] data = udpSocket.GetByteArrayDataFromUdpSocket();
            if (data == null) return null;

           
            BitmapImage biImg = new BitmapImage();
            MemoryStream ms = new MemoryStream(data);
            biImg.BeginInit();
            biImg.StreamSource = ms;
            biImg.EndInit();

            ImageSource image = biImg as ImageSource;
            return (BitmapSource)image;

        }
    }
}
