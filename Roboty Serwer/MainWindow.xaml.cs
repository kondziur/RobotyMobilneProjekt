/* Projekt z przedmiotu roboty mobilne
 * 
 * Autorzy:
 * Jardzioch Pawel
 * Kruk Konrad 
 * Kukla Mateusz
 * 
 */

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
    // ogólnodostępne zmienne
    public class Variables
    {
        public static string ip_addr = "";
        public static int port;
        public static string tryb = "";

        public static bool wasd = false;
    }


    public partial class MainWindow : Window
    {
        // deklaracja obiektów potrzebnych do połączenia sieciowego
        TcpClient client;
        NetworkStream stream;

        // inicjalizacja okna
        public MainWindow()
        {
            InitializeComponent();

            btnDisconnect.IsEnabled = false;
            btnSend.IsEnabled = false;
            btnEngines.IsEnabled = false;
            btnMonitor.IsEnabled = false;
            btnLocation.IsEnabled = false;
            btnControl.IsEnabled = false;

            textBoxCommand.IsEnabled = false;
            Eng1.IsEnabled = false;
            Eng2.IsEnabled = false;
            textBoxNumber.IsEnabled = false;
        }

        public void xorButtons()
        {
            btnConnect.IsEnabled = !btnConnect.IsEnabled;
            btnDisconnect.IsEnabled = !btnDisconnect.IsEnabled;
            btnSend.IsEnabled = !btnSend.IsEnabled;
            btnEngines.IsEnabled = !btnEngines.IsEnabled;
            btnMonitor.IsEnabled = !btnMonitor.IsEnabled;
            btnLocation.IsEnabled = !btnLocation.IsEnabled;
            btnControl.IsEnabled = !btnControl.IsEnabled;

            textBoxIP.IsEnabled = !textBoxIP.IsEnabled;
            textBoxPort.IsEnabled = !textBoxPort.IsEnabled;
            textBoxCommand.IsEnabled = !textBoxCommand.IsEnabled;
            Eng1.IsEnabled = !Eng1.IsEnabled;
            Eng2.IsEnabled = !Eng2.IsEnabled;
            textBoxNumber.IsEnabled = !textBoxNumber.IsEnabled;
        }

        // metoda wywoływana w metodzie "btnConnect"
        public void ConnectAsClient()
        {
            try
            {
                // utworzenie klienta TCP oraz połączenie z serwerem
                client = new TcpClient();
                client.Connect(IPAddress.Parse(Variables.ip_addr), Variables.port);
                Console.WriteLine("połączono z serwerem");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd podczas łączenia z serwerem: " + ex.Message);
            }
        }

        // obsługa przycisku "Połącz"
        // metoda nawiązuje komunikację z serwerem poprzez utworzenie wątku, obsługującego nawiązanie połączenia
        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            xorButtons();
            btnEngines.IsEnabled = false;
            btnLocation.IsEnabled = false;
           
            try
            {
                // aktualizacja zmiennych przechowujących dane dotyczące adresu IP i portu
                Variables.ip_addr = textBoxIP.Text;
                Variables.port = Convert.ToInt32(textBoxPort.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błędny format portu: " + ex.Message);
            }
            

            try
            {
                // utworzenie i uruchomienie wątku
                Thread mThread = new Thread(new ThreadStart(ConnectAsClient));
                mThread.Start();
                textBoxResp.Text = "połączono"; 
            }

            catch (Exception ex)
            {
                MessageBox.Show("Błąd podczas tworzenia wątku: " + ex.Message);
            }
            
        }


        // obsługa przycisku "Rozłącz"
        // metoda zamykająca połączenie i resetująca pola tekstowe
        private void btnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            xorButtons();
            btnMonitor.IsEnabled = false;
            btnControl.IsEnabled = false;
            btnEngines.IsEnabled = false;
            btnLocation.IsEnabled = false;
            
            try
            {
                if (Variables.tryb == "sterowanie")
                {
                    SendSpeeds(0, 0);
                    Thread.Sleep(2000);
                }
              

                // wysłanie komendy rozłączenia
                SendCommand("0");

                // zamknięcie strumienia
                stream.Close();

                // destrukcja klienta
                client.Close();
                client = null;


                // pętla resetująca zawartość pól tekstowych, zawierających informacje o położeniu robotów
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
                Variables.tryb = "";
                

            }

            catch (Exception ex)
            {

                MessageBox.Show("Błąd podczas rozłączania z serwerem: " + ex.Message);
            }
            
        }


        // obsługa przycisku "Wyślij"
        // metoda wysyłająca komendę zdefiniowaną przez użytkownika
        public void btnSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // przekazanie zawartości pola komendy do metody "SendCommand"
                string komenda = textBoxCommand.Text;
                SendCommand(komenda);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }


        //metoda która wysyła ramkę i odbiera odpowiedź, argumentem jest komenda
        private void SendCommand(string komenda)
        {
            try
            {
                // otwarcie strumienia danych z serwerem
                stream = client.GetStream();
            }

            catch (Exception ex)
            {
                MessageBox.Show("Błąd podczas wysyłania komendy: " + ex.Message);
            }

            // deklaracja zmiennej przechowującej dane doo wysłania
            Byte[] data = null;

            try
            {
                // konwersja komendy na postać gotową do wysłania
                data = System.Text.Encoding.ASCII.GetBytes(komenda);
                for (Byte zamien = 0; zamien != data.Length; ++zamien)
                {
                    data[zamien] -= 48;
                }

                Console.WriteLine("Data to string:{0}", data.ToString());
            }

            catch (Exception ex)
            {
                MessageBox.Show("Błąd podczas konwersji komendy: " + ex.Message);
            }


            try
            {
                // wysyłanie ramki z komendą
                stream.Write(data, 0, data.Length);
            }

            catch (Exception ex)
            {
                MessageBox.Show("Błąd podczas wysyłania ramki: " + ex.Message);
            }

            // deklaracja zmiennej przechowującej odebrane dane
            String responseData = String.Empty;

            try
            {
                // odbiór wartości na polecenie "monitor"
                if (komenda == "120")
                {
                    data = new Byte[2];

                    data[0] = (byte)stream.ReadByte();
                    data[1] = (byte)stream.ReadByte();
                }

                // odbiór wartości na polecenie "położenie"
                else if (komenda == "3")
                {
                    data = new Byte[141];

                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] = (byte)stream.ReadByte();
                    }

                }

                // odbiór wartości na polecenie "sterowanie"
                else if (komenda[0] == '1' && komenda[1] == '1')
                {
                    // rozmiar tablicy zależy od wybranej liczby robotów
                    int x = 2 + Convert.ToInt16(textBoxNumber.Text);
                    data = new Byte[x];

                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] = (byte)stream.ReadByte();
                    }
                }

                // wyświetlanie ramki zwrotnej
                for (int i = 0; i < data.Length; i++)
                {
                    responseData = responseData + data[i].ToString();
                }
                               
                // wpisywanie danych o położeniu robotów do pól tekstowych
                if ( responseData.Length > 3 && responseData[0] == '4' )
                {
                    RozkodujRamke(data);                   
                }

                textBoxResp.Text = responseData;
                Console.WriteLine("Długość response data:{0}",responseData.Length);
 
            }
            catch (Exception e)
            {
                MessageBox.Show("Błąd podczas odbierania danych: " + e.Message);
            }
            
                
        }

        public void RozkodujRamke(byte[] data)
        {
            for (int i = 0; i < 6; i++)
            {
                var id = (TextBox)this.FindName("id" + (i + 1).ToString());
                var PosX = (TextBox)this.FindName("PosX" + (i + 1).ToString());
                var PosY = (TextBox)this.FindName("PosY" + (i + 1).ToString());
                var AngZ = (TextBox)this.FindName("AngZ" + (i + 1).ToString());

                UInt16 idRobot = (BitConverter.ToUInt16(data, 2 + 14 * i - 1));

                if (idRobot > 255)
                {
                    idRobot -= 256;
                }

                id.Text = idRobot.ToString();
                PosX.Text = (BitConverter.ToSingle(data, 2 + 14 * i + 1)).ToString();
                PosY.Text = (BitConverter.ToSingle(data, 2 + 14 * i + 5)).ToString();
                AngZ.Text = (BitConverter.ToSingle(data, 2 + 14 * i + 9)).ToString();
            }
        }

        // obsługa przycisków klawiatury WASD
        private void Window_KeyDown(object sender, KeyEventArgs a)
        {
            if (Variables.wasd == true)
            {
                if (a.Key == Key.W)
                {
                    float e1 = (float)slider_Speed.Value;
                    float e2 = e1;
                    SendSpeeds(e1, e2);
                }
                // A
                if (a.Key == Key.A)
                {
                    float e1 = -(float)slider_Speed.Value;
                    float e2 = -e1;
                    SendSpeeds(e1, e2);
                }
                // S
                if (a.Key == Key.S)
                {
                    float e1 = -(float)slider_Speed.Value;
                    float e2 = e1;
                    SendSpeeds(e1, e2);
                }
                // D
                if (a.Key == Key.D)
                {
                    float e1 = (float)slider_Speed.Value;
                    float e2 = -e1;
                    SendSpeeds(e1, e2);
                }
            }
        }

        // obsługa przycisku "Monitor"
        // metoda wysyła do serwera komendę wejścia w tryb monitor
        private void btnMonitor_Click(object sender, RoutedEventArgs e)
        {
            btnControl.IsEnabled = false;
            btnEngines.IsEnabled = false;
            btnLocation.IsEnabled = true;
            btnMonitor.IsEnabled = false;

            Variables.tryb = "monitor";
            string komenda = "120";
            SendCommand(komenda);
        }

        // obsługa przycisku "Położenie"
        // metoda wysyła do serwera zapytanie o aktualne położenie robotów
        private void btnLocation_Click(object sender, RoutedEventArgs e)
        {
            string komenda = "3";
            SendCommand(komenda);
        }

        // obsługa przycisku "Sterowanie"
        // metoda wysyła do serwera komendę wejścia w tryb sterowanie
        private void btnControl_Click(object sender,RoutedEventArgs e)
        {
            btnMonitor.IsEnabled = false;
            btnEngines.IsEnabled = true;
            btnLocation.IsEnabled = true;
            btnControl.IsEnabled = false;
            Variables.tryb = "sterowanie";
            // jeśli użytkownik nie zdefiniował liczby robotów, domyślnie ustawiana jest wartość 1
            if (textBoxNumber.Text == "")
            {
                textBoxNumber.Text = "1";
            }

            string komenda ="11" + textBoxNumber.Text;
            Console.WriteLine("komenda to:{0}", komenda);
            SendCommand(komenda);
        }

        // obsługa przycisku "Steruj"
        // metoda wysyła do serwera komendę ruchu wybranego robota
        private void btnEngines_Click(object sender, RoutedEventArgs e)
        {
            float eng1 = 0;
            float eng2 = 0;

            try
            {
                // pobranie wartości silników z pól tekstowych
                eng1 = (float)int.Parse(Eng1.Text);
                eng2 = (float)int.Parse(Eng2.Text);

                Console.WriteLine(eng1 + eng2);
                SendSpeeds(eng1, eng2);
            }

            catch (Exception ex)
            {
                MessageBox.Show("Błąd podczas konwersji danych: " + ex.Message);
            }
            
            
        }

        public void SendSpeeds(float eng1, float eng2)
        {
            byte[] Ramka = null;

            try
            {
                // deklaracja ramki zawierającej dane do wysłania o długości zależnej od wybranej ilości robotów 
                int n = int.Parse(textBoxNumber.Text);
                Ramka = new byte[1 + 8 * n];

                // zapisanie komendy sterowania ('5') oraz wartości silników do wysyłanej ramki
                BitConverter.GetBytes('5' - 48).CopyTo(Ramka, 0);

                for (int i = 0; i < n; i++)
                {
                    BitConverter.GetBytes(eng1).CopyTo(Ramka, 1 + 8 * i);
                    BitConverter.GetBytes(eng2).CopyTo(Ramka, 5 + 8 * i);
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show("Błąd podczas przygotowywania ramki: " + ex.Message);
            }

            try
            {
                // wysłanie ramki
                stream.Write(Ramka, 0, Ramka.Length);
            }

            catch (Exception ex)
            {
                MessageBox.Show("Błąd podczas wysyłania ramki: " + ex.Message);
            }


            //odbiór ramki zwrotnej
            String responseData = String.Empty;

            //wyświetlanie ramki zwrotnej
            textBoxResp.Text = responseData;
        }

        private void checkBox_WASD_Checked(object sender, RoutedEventArgs e)
        {
            Variables.wasd = true;
        }

        private void checkBox_WASD_Unchecked(object sender, RoutedEventArgs e)
        {
            Variables.wasd = false;
        }

        private void Grid_KeyUp(object sender, KeyEventArgs e)
        {

        }



    }
}