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
        TcpClient client;
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
                client = null;
                
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
                client = new TcpClient();
                client.Connect(IPAddress.Parse(Variables.ip_addr), Variables.port);
                Console.WriteLine("połączono z serwerem");
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        //metoda która wysyła ramkę i odbiera odpowieź, argumentem jest komenda
        private void SendCommand(string komenda)
        {
            try
            {
                //wysyłanie ramki z komendą
                stream = client.GetStream();

                // to jest do poprawienia na jakieś ładniejsze 
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(komenda);
                for (Byte zamien = 0; zamien != data.Length; ++zamien)
                {
                    data[zamien] -= 48;
                } 

                Console.WriteLine(data.ToString());
                stream.Write(data, 0, data.Length);
                String responseData = String.Empty;

                if (komenda == "120")
                {
                    data = new Byte[2];
                    //Int32 bytes = stream.Read(data, 0, data.Length);
                    data[0] = (byte)stream.ReadByte();
                    data[1] = (byte)stream.ReadByte();
                }
                else if (komenda == "3")
                {
                    data = new Byte[141];
                    //Int32 bytes = stream.Read(data, 0, data.Length);

                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] = (byte)stream.ReadByte();
                    }

                }

                //odbiór ramki zwrotnej
                

                //wyświetlanie ramki zwrotnej

                for (int i = 0; i < data.Length; i++)
                {
                    responseData = responseData + data[i].ToString();
                }
                               

                if ( responseData.Length > 3 )
                {
                    if ( responseData[0] == '4' )
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            id1.Text = Math.Round((BitConverter.ToSingle(data, 2 + 14 * i - 1))).ToString();
                            PosX.Text = (BitConverter.ToSingle(data, 2 + 14 * i + 1)).ToString();
                            textBox_PosY.Text = (BitConverter.ToSingle(data, 2 + 14 * i + 5)).ToString();
                            textBox_AngZ.Text = (BitConverter.ToSingle(data, 2 + 14 * i + 9)).ToString();
                        }
                        //for (int i = 0; i < responseData.Length / 14; i++)
                        //{
                            //textBox_PosX.Text = (BitConverter.ToSingle(data, i * 14 + 3)).ToString();
                            
                            //try
                            //{
                                
                            //    textBox_PosX.Text = (BitConverter.ToSingle(data, i * 14 + 3)).ToString();
                            //    textBox_PosY.Text = (BitConverter.ToSingle(data, i * 14 + 7)).ToString();
                            //    textBox_AngZ.Text = (BitConverter.ToSingle(data, i * 14 + 11)).ToString();
                            //}
                            //catch (Exception)
                            //{
                            //    break;
                                
                            //}
                        //}
                    }
                }

                //Console.WriteLine(responseData[0] + ",  "+ ( responseData.Length ).ToString() );
                textBoxResp.Text = responseData;

                //textBoxResp.Text = byte1.ToString() + byte2.ToString() + byte3.ToString();

                
            }
            catch (Exception)
            {
                MessageBox.Show("serwer rozłączony");
                throw;
            }
            
                
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

        private void btnMonitor_Click(object sender, RoutedEventArgs e)
        {
            string komenda = "120";

            SendCommand(komenda);
        }

        private void btnLocation_Click(object sender, RoutedEventArgs e)
        {
            string komenda = "3";

            SendCommand(komenda);
        }

        

        
    }
}