/* Filename     : MainWindow.xaml.cs
 * Project      : ChatSystem/WinProgA04
 * Author(s)    : Nathan Bray, Gabe Paquette
 * Date Created : 2016-11-12
 * Description  : 
 */
using BWCS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Messaging;
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
        string mQueueName = @".\private$\SETQueue";
        MessageQueue mq;

        /// <summary>
        /// 
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            //
            if (!MessageQueue.Exists(mQueueName))
            {
                mq = MessageQueue.Create(mQueueName);
            }
            else
            {
                mq = new MessageQueue(mQueueName);
            }

            Window startup = new startupWindow();
            startup.ShowDialog();
            
            //
            if (!ClientPipe.connected)
            {
                this.Close();
            }
            else
            {
                //
                lblAlias.Content += ClientPipe.Alias;
                Thread readThread = new Thread(GetMessages);
                readThread.Start();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="read"></param>
        public void receiveMsg(string read)
        {
            // need try
            StatusCode state = (StatusCode)int.Parse(read.Substring(0, 1));
            char[] seperator = { ':' };
            string[] message;
            //
            if (state == StatusCode.Whisper)
            {
                message = read.Split(seperator, 3, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                message = read.Split(seperator, StringSplitOptions.RemoveEmptyEntries);
            }

            //
            Dispatcher.Invoke(() =>
            {
                switch (state)
                {
                    case StatusCode.ClientConnected:
                        txtChat.Text += message[1] + " has connected.\n";
                        lbxUserList.Items.Add(message[1]);
                        break;
                    case StatusCode.ClientDisconnected:
                        txtChat.Text += message[1] + " has disconnected.\n";
                        lbxUserList.Items.Remove(message[1]);
                        break;
                    case StatusCode.Whisper:
                        string msg = message[1] + ": " + message[2];
                        txtChat.Text += msg + "\n";
                        break;
                    case StatusCode.ServerClosing:
                        txtChat.Text += "Server is closed. Please leave.\n";
                        btnSend.IsEnabled = false;
                        break;
                    case StatusCode.SendUserList:
                        for (int i = 1; i < message.Length; i++)
                        {
                            if (message[i] != ClientPipe.Alias)
                            {
                                txtChat.Text += message[i] + "\n";
                            }
                        }
                        break;
                    default:
                        break;
                }
            });
        }


        /// <summary>
        /// 
        /// </summary>
        public void GetMessages()
        {
            mq.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });

            while (true)
            {
                try
                {
                    //
                    string message = (string)mq.Receive().Body;
                        if (message != null)
                        {
                            receiveMsg(message);
                        } 
                }
                catch (MessageQueueException mqex)
                {
                    MessageBox.Show("MQ Exception: " + mqex.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Exception: " + ex.Message);
                }
            }

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbxUserList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selected = lbxUserList.SelectedItem.ToString();
            if (selected != ClientPipe.Alias)
            {
                //
                lblSendTo.Content += selected;
                if (txtMsg.Text.Trim().Length > 0)
                {
                    btnSend.IsEnabled = true;
                }
            }
            else
            {
                //
                btnSend.IsEnabled = false;
                selected = "";
                lblSendTo.Content = "To: ";
            }

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if (selected.Length > 0)
            {
                string message = txtMsg.Text;
                if (message.Trim().Length > 0)
                {
                    //
                    txtChat.Text += "You: " + message + "\n";
                    message = PipeClass.makeMessage(true, StatusCode.Whisper, ClientPipe.Alias, selected, message);
                    ClientPipe.sendMessage(message);
                    txtMsg.Clear();
                }

            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            if (MessageQueue.Exists(mQueueName))
            {
                mq.Close(); 
            }
            if (ClientPipe.connected)
            {
                string message = PipeClass.makeMessage(true, StatusCode.ClientDisconnected, ClientPipe.Alias);
                ClientPipe.sendMessage(message); 
            }
        }
    }
}
