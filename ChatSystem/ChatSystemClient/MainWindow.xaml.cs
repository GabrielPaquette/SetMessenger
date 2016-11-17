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
                mq.Purge();
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
            StatusCode state = (StatusCode)int.Parse(read.Substring(1, 1));
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
                        txtAll.Text += message[1] + " has connected.\n";
                        if (message[1] == ClientPipe.Alias)
                        {
                            lbxUserList.Items.Insert(0,message[1]);
                        }
                        else
                        {
                            lbxUserList.Items.Add(message[1]);
                        }
                        break;
                    case StatusCode.ClientDisconnected:
                        txtAll.Text += message[1] + " has disconnected.\n";
                        if ((string)lbxUserList.SelectedItem == message[1])
                        {
                            lbxUserList.UnselectAll();
                        }
                        lbxUserList.Items.Remove(message[1]);
                        break;
                    case StatusCode.Whisper:
                        string msg = message[1] + ": " + message[2];
                        txtPrivate.Text += msg + "\n";
                        btnSend.IsEnabled = false;
                        break;
                    case StatusCode.ServerClosing:
                        txtAll.Text += "Server is closed. Please leave.\n";
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
            if (lbxUserList.SelectedItem != null)
            {
                selected = lbxUserList.SelectedItem.ToString();

                if (selected != ClientPipe.Alias)
                {
                    //
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
                }
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
                    txtPrivate.Text += "("+selected+") You: " + message + "\n";
                    message = PipeClass.makeMessage(true, StatusCode.Whisper, ClientPipe.Alias, selected, message);
                    ClientPipe.sendMessage(message);
                    txtMsg.Clear();
                    txtMsg.Focus();
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
                mq.Dispose();
            }
            if (ClientPipe.connected)
            {
                string message = PipeClass.makeMessage(true, StatusCode.ClientDisconnected, ClientPipe.Alias);
                ClientPipe.sendMessage(message); 
            }
        }

        private void frmMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnSend_Click(sender, e);
            }
        }
    }
}
