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
            textBoxResp.Text = "połączono"; 
        }


        //przycisk "Rozłącz"
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SendCommand("0");
                stream.Close();
                client.Close();
                client = null;


                for (int i = 0; i < 6; i++)
                {
                    var id = (TextBox)this.FindName("id" + (i + 1).ToString());
                    var PosX = (TextBox)this.FindName("PosX" + (i + 1).ToString());
                    var PosY = (TextBox)this.FindName("PosY" + (i + 1).ToString());
                    var AngZ = (TextBox)this.FindName("AngZ" + (i + 1).ToString());



                    id.Text = "";
                    PosX.Text = "";
                    PosY.Text = "";
                    AngZ.Text = "";
                }
                textBoxResp.Text = "rozłączono";          
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
                else if (komenda[0] == '1' && komenda[1] == '1')
                {
                    int x = 2 + Convert.ToInt16(textBoxNumber.Text);
                    data = new Byte[x];
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
                            var id = (TextBox)this.FindName("id" + (i + 1).ToString());
                            var PosX = (TextBox)this.FindName("PosX" + (i + 1).ToString());
                            var PosY = (TextBox)this.FindName("PosY" + (i + 1).ToString());
                            var AngZ = (TextBox)this.FindName("AngZ" + (i + 1).ToString());

                            UInt16 id_69 = (BitConverter.ToUInt16(data, 2 + 14 * i - 1)); 
                            
                            if (id_69 > 255)
                            {
                                id_69 -= 256;
                            }
                                                        
                            id.Text = id_69.ToString();
                            PosX.Text = (BitConverter.ToSingle(data, 2 + 14 * i + 1)).ToString();
                            PosY.Text = (BitConverter.ToSingle(data, 2 + 14 * i + 5)).ToString();
                            AngZ.Text = (BitConverter.ToSingle(data, 2 + 14 * i + 9)).ToString();
                        }
                    }

                }

                
                textBoxResp.Text = responseData;
                Console.WriteLine(responseData.Length);
              

                
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                throw;
            }
            
                
        }
            

        private void Window_KeyDown(object sender, KeyEventArgs a)
        {
            // strzałka w górę
            if (a.Key == Key.W)
            {
                //zwiekszyc predkosc, bo 50 to 10^-14 !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!


                float argument = 50;
                string formatter = "{0,16:E7}{1,20}";
                byte[] byteArray = BitConverter.GetBytes(argument);
                Console.WriteLine(formatter, argument,
                    BitConverter.ToString(byteArray));




                SendCommand("5" + argument.ToString() + argument.ToString());
                Console.WriteLine("góra");
            }
            // strzałka w dół
            if (a.Key == Key.S)
            {
                Console.WriteLine("dół");
                SendCommand("005050");
            }
            // strzałka w lewo
            if (a.Key == Key.Left)
            {
                SendCommand("001040");
            }
            // strzała w prawo
            if (a.Key == Key.Right)
            {
                SendCommand("001040");
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

        private void btnControl_Click(object sender,RoutedEventArgs e)
        {
            if (textBoxNumber.Text == "") textBoxNumber.Text = "1";
            string komenda ="11"+textBoxNumber.Text;
            Console.WriteLine("komenda to:{0}", komenda);
            SendCommand(komenda);
        }


        

        
    }
}