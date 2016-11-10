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
            Variables.komenda = "[" + textBox1.Text + "]";

            //wysyłanie ramki z komendą
            stream = client.GetStream();
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(Variables.komenda);
            stream.Write(data, 0, data.Length);

            //odbiór ramki zwrotnej
            data = new Byte[28];
            String responseData = String.Empty;
            Int32 bytes = stream.Read(data, 0, data.Length);
            responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);

            //wyświetlanie ramki zwrotnej
            textBoxResp.Text = responseData;

            textBoxStat.Text = int.Parse(responseData.Substring(1, 2), System.Globalization.NumberStyles.HexNumber).ToString();
            //bateria jest litle Endian
            textBoxBat.Text = int.Parse(responseData.Substring(5, 2) + responseData.Substring(3, 2), System.Globalization.NumberStyles.HexNumber).ToString();
            textBoxS1.Text = int.Parse(responseData.Substring(7, 4), System.Globalization.NumberStyles.HexNumber).ToString();
            textBoxS2.Text = int.Parse(responseData.Substring(11, 4), System.Globalization.NumberStyles.HexNumber).ToString();
            textBoxS3.Text = int.Parse(responseData.Substring(15, 4), System.Globalization.NumberStyles.HexNumber).ToString();
            textBoxS4.Text = int.Parse(responseData.Substring(19, 4), System.Globalization.NumberStyles.HexNumber).ToString();
            textBoxS5.Text = int.Parse(responseData.Substring(23, 4), System.Globalization.NumberStyles.HexNumber).ToString();

        }


        //metoda wywoływana w metodzie "połącz'
        public void ConnectAsClient()
        {
            client.Connect(IPAddress.Parse(Variables.ip_addr), 8000);
        }


        //metoda kończąca połączenie z klientem
        private void DisconnectAsClient()
        {
            stream.Close();
            client.Close();
        }

    }
}
//huj kurwa
