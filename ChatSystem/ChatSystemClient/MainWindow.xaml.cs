using BWCS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using System.Windows.Shapes;

namespace ChatSystemClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ClientPipe client = new ClientPipe();
        public MainWindow()
        {
            InitializeComponent();
            Window startup = new startupWindow();
            startup.ShowDialog();
            if (!ClientPipe.connected)
            {
                this.Close();
            }
            else
            {
                lblAlias.Content += ClientPipe.Alias;
                Thread readThread = new Thread(readMsg);
                readThread.Start();
            }
        }

        public void receiveMsg(string read)
        {
            // need try
            StatusCode state = (StatusCode)int.Parse(read.Substring(0, 1));
            char[] seperator = { ':' };
            string[] message;
            if (state == StatusCode.Whisper)
            {
                message = read.Split(seperator, 3, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                message = read.Split(seperator, StringSplitOptions.RemoveEmptyEntries);
            }
            //char[] seper = { ':' };
            //string test = "1:two:-1:message:astest";
            //string[] testArray = test.Split(seper, 4, StringSplitOptions.RemoveEmptyEntries);
            //foreach (string item in testArray)
            //{
            //    Console.WriteLine(item);
            //}

            switch (state)
            {
                case StatusCode.ClientConnected:
                    lbxChat.Items.Add(message[1] + " has connected.");
                    lbxUserList.Items.Add(message[1]);
                    break;
                case StatusCode.ClientDisconnected:
                    lbxChat.Items.Add(message[1] + " has disconnected.");
                    lbxUserList.Items.Remove(message[1]);
                    break;
                case StatusCode.Whisper:
                    lbxChat.Items.Add(message[1] + ": "+ message[2]);
                    break;
                case StatusCode.ServerClosing:
                    lbxChat.Items.Add("Server is closed. Please leave.");
                    btnSend.IsEnabled = false;
                    break;
                case StatusCode.SendUserList:
                    for (int i = 1; i < message.Length; i++)
                    {
                        lbxUserList.Items.Add(message[i]);
                    }
                    break;
                default:
                    break;
            }
        }

        public void readMsg(object data)
        {
            while (true)
            {
                StreamReader reader = new StreamReader(ClientPipe.clientStream);
                string read = reader.ReadLine();
                if (read != "")
                {
                    receiveMsg(read);
                }
            }
        }

        private void lbxUserList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lblSendTo.Content+= lbxUserList.SelectedItem.ToString();
        }
    }
}
