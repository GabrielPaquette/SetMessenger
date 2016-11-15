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
        private string selected = "";
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


            Dispatcher.Invoke(() =>
            {
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
                        lbxChat.Items.Add(message[1] + ": " + message[2]);
                        break;
                    case StatusCode.ServerClosing:
                        lbxChat.Items.Add("Server is closed. Please leave.");
                        btnSend.IsEnabled = false;
                        break;
                    case StatusCode.SendUserList:
                        for (int i = 1; i < message.Length; i++)
                        {
                            if (message[i] != ClientPipe.Alias)
                            {
                                lbxUserList.Items.Add(message[i]); 
                            }
                        }
                        break;
                    default:
                        break;
                }
            });
        }


        public void readMsg(object data)
        {
            while (true)
            {

                StreamReader reader = new StreamReader(ClientPipe.clientStream);
                ClientPipe.clientStream.WaitForPipeDrain();
                string read = reader.ReadLine();
                
                if (read != "")
                {
                    receiveMsg(read);
                }
            }

            //while (true)
            //{

            //    StreamReader reader = new StreamReader(ClientPipe.clientStream);
            //    ClientPipe.clientStream.WaitForPipeDrain();
            //    //string read = 
            //    byte[] bit = Encoding.ASCII.GetBytes(reader.ReadLine());

            //    if (bit.Length > 0)
            //    {
            //        ClientPipe.clientStream.Read(bit, 0, 1024);
            //        receiveMsg(bit.ToString());
            //    }
            //}
        }

        private void lbxUserList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selected = lbxUserList.SelectedItem.ToString();
            if (selected != ClientPipe.Alias)
            {
                lblSendTo.Content += selected;
                if (txtMsg.Text.Trim().Length > 0)
                {
                    btnSend.IsEnabled = true;
                }
            }
            else
            {
                btnSend.IsEnabled = false;
                selected = "";
                lblSendTo.Content = "To: ";
            }

        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if (selected.Length > 0 )
            {
                string message = txtMsg.Text;
                if (message.Trim().Length > 0)
                {
                    message = PipeClass.makeMessage(StatusCode.Whisper, ClientPipe.Alias, selected, message);
                    ClientPipe.sendMessage(message);
                }
                
            }
        }

        private void txtMsg_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtMsg.Text.Length > 0)
            {
                if (selected.Length > 0)
                {
                    btnSend.IsEnabled = true;
                }
                else
                {
                    btnSend.IsEnabled = false;
                } 
            }
            else
            {
                btnSend.IsEnabled = false;
            }
        }

        private void frmMain_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string message = PipeClass.makeMessage(StatusCode.ClientDisconnected, ClientPipe.Alias);
            ClientPipe.sendMessage(message);
        }
    }
}
