﻿/* Projekt z przedmiotu roboty mobilne
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
            try
            {
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

                textBoxResp.Text = responseData;
                Console.WriteLine("Długość response data:{0}",responseData.Length);
 
            }
            catch (Exception e)
            {
                MessageBox.Show("Błąd podczas odbierania danych: " + e.Message);
            }
            
                
        }

        // obsługa przycisków klawiatury
        private void Window_KeyDown(object sender, KeyEventArgs a)
        {
            // strzałka w górę
            if (a.Key == Key.W)
            {
                //zwiekszyc predkosc, bo 50 to 10^-14 !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!


                float argument = 90;
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

        // obsługa przycisku "Monitor"
        // metoda wysyła do serwera komendę wejścia w tryb monitor
        private void btnMonitor_Click(object sender, RoutedEventArgs e)
        {
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
            }

            catch (Exception ex)
            {
                MessageBox.Show("Błąd podczas konwersji danych: " + ex.Message);
            }
            

            // deklaracja ramki zawierającej dane do wysłania o długości zależnej od wybranej ilości robotów 
            int n = int.Parse(textBoxNumber.Text);
            byte[] Ramka = new byte[1 + 8 * n];
            
            // zapisanie komendy sterowania ('5') oraz wartości silników do wysyłanej ramki
            BitConverter.GetBytes('5' - 48).CopyTo(Ramka, 0);

            for (int i = 0; i < n; i++)
            {
                BitConverter.GetBytes(eng1).CopyTo(Ramka, 1 + 8*i);
                BitConverter.GetBytes(eng2).CopyTo(Ramka, 5 + 8*i); 
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
    }
}