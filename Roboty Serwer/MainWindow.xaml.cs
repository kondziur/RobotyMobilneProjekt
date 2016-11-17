using System;
using System.Collections.Generic;
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
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;



namespace RobotyMobilne
{
    //ogólnodostępne zmienne
    public class Variables
    {
        public static string ip_addr = "";
        public static int port;
    }

    public partial class MainWindow : Window
    {
        // to jest tutaj, bo jak jest w metodach, to w innych nie da się użyć
        TcpClient client = new TcpClient();
        NetworkStream stream;


        public MainWindow()
        {
            InitializeComponent();
        }


        //przycisk "Połącz"
        public void button_Click(object sender, RoutedEventArgs e)
        {
            Variables.ip_addr = textBox.Text;
            Variables.port = Convert.ToInt32(textBoxPort.Text);

            Thread mThread = new Thread(new ThreadStart(ConnectAsClient));
            mThread.Start();
        }


        //przycisk "Rozłącz"
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                stream.Close();
                client.Close();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }

        }


        //przycisk "Wyślij Komendę"
        public void button2_Click(object sender, RoutedEventArgs e)
        {
            //try
            {
                string komenda = textBox1.Text;
                
                SendCommand(komenda);
            }
            //catch (Exception ex)
            //{

            //    MessageBox.Show(ex.Message);
            //}
        }


        //metoda wywoływana w metodzie "połącz'
        public void ConnectAsClient()
        {
            try
            {
                client.Connect(IPAddress.Parse(Variables.ip_addr), Variables.port);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        //metoda która wysyła ramkę i odbiera odpowieź, argumentem jest komenda
        private void SendCommand(string komenda)
        {
            //wysyłanie ramki z komendą
            stream = client.GetStream();
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(komenda);
            stream.Write(data, 0, data.Length);

            //odbiór ramki zwrotnej
            data = new Byte[141];
            String responseData = String.Empty;
            Int32 bytes = stream.Read(data, 0, data.Length);
            responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);

            //wyświetlanie ramki zwrotnej
            textBoxResp.Text = responseData;
        }
            

        private void Window_KeyDown(object sender, KeyEventArgs a)
        {
            // strzałka w górę
            if (a.Key == Key.Up)
            {

                SendCommand("001010");
                Console.WriteLine("góra");
            }
          
        }
    }
}