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
        public static string komenda = "";
        public static string port = "";
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
            Variables.port = textBoxPort.Text;


            Thread mThread = new Thread(new ThreadStart(ConnectAsClient));
            mThread.Start();
        }


        //przycisk "Rozłącz"
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            DisconnectAsClient();
        }


        //przycisk "Wyślij Komendę"
        public void button2_Click(object sender, RoutedEventArgs e)
        {
            Variables.komenda = textBox1.Text;

            //wysyłanie ramki z komendą
            stream = client.GetStream();
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(Variables.komenda);
            stream.Write(data, 0, data.Length);
            Console.WriteLine(Variables.komenda);
            Console.WriteLine(data[1]);

            //odbiór ramki zwrotnej
            data = new Byte[141];
            String responseData = String.Empty;
            Int32 bytes = stream.Read(data, 0, data.Length);
            responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);

            //wyświetlanie ramki zwrotnej
            textBoxResp.Text = responseData;

          

        }


        //metoda wywoływana w metodzie "połącz'
        public void ConnectAsClient()
        {
            client.Connect(IPAddress.Parse(Variables.ip_addr), 50131);
        }


        //metoda kończąca połączenie z klientem
        private void DisconnectAsClient()
        {
            stream.Close();
            client.Close();
        }

    }
}

