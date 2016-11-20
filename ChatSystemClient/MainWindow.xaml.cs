/* Filename     : MainWindow.xaml.cs
 * Project      : ChatSystem/WinProgA04
 * Author(s)    : Nathan Bray, Gabe Paquette
 * Date Created : 2016-11-12
 * Description  : This program is the UI for the Chat system created to work on a local network.
 * It makes use of Named Pipes to send messages to a local server, and recieves messages through a private message queue. * 
 */
using BWCS;
using System;
using System.Messaging;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChatSystemClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string Alias { get; set; }

        // Variables for the notification of the private tab
        private bool toggle = true;
        private System.Timers.Timer notification = new System.Timers.Timer(500);

        // variable to store the currently selected user in the userList lisbox
        private string selected = "";

        private ClientQueue mq;

        public MainWindow()
        {

            InitializeComponent();
            // After startup, create/connect to the message queue
            try
            {
                // Create and connect to the message queue
                mq = new ClientQueue();
            }
            catch (InvalidOperationException)
            {

                this.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            // Invoke the startup window the get the basic information the chat program needs
            Window startup = new startupWindow();
            startup.ShowDialog();

            // when the window returns, it may set a flag indicating whther the client has connected to the server
            if (!ClientPipe.connected)
            {
                // if it didn't, exit cleanly because it meant the user wanted to cancel connecting to the application
                Environment.Exit(0);
            }
            else
            {
                // Set the label to show the users' alias
                lblAlias.Content += Alias;

                // Start the thread to constantly read from the message queue
                Thread readThread = new Thread(readLoop);
                readThread.Start();
            }

            // Initial setup for the notification timer
            notification.Elapsed += notifiyUser;
            notification.AutoReset = true;
        }


        /// <summary>
        /// This method start a loop to read messages from the message queue. When it recieves one, it will pass it off to be handled
        /// the loop wil continue to process until the application is closed
        /// </summary>
        public void readLoop()
        {
            while (true)
            {
                try
                {
                    // Read
                    string message = mq.GetMessages();
                    receiveMsg(message);
                }
                catch (MessageQueueException mqex)
                {
                    if (MessageQueue.Exists(mq.getPath()))
                    {
                        MessageBox.Show("MQ Exception: " + mqex.Message);
                    }
                    else
                    {
                        break;
                    }
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
                        scrollAll.ScrollToBottom();
                        if (message[1] == Alias)
                        {
                            lbxUserList.Items.Insert(0, message[1]);
                        }
                        else
                        {
                            lbxUserList.Items.Add(message[1]);
                        }
                        break;
                    case StatusCode.ClientDisconnected:
                        txtAll.Text += message[1] + " has disconnected.\n";
                        scrollAll.ScrollToBottom();
                        if ((string)lbxUserList.SelectedItem == message[1])
                        {
                            lbxUserList.UnselectAll();
                        }
                        lbxUserList.Items.Remove(message[1]);
                        break;
                    case StatusCode.Whisper:
                        string msg = message[1] + ": " + message[2];
                        txtPrivate.Text += msg + "\n";
                        newPrivateMessage();
                        break;
                    case StatusCode.ServerClosing:
                        MessageBox.Show("The server is now closing. Exiting your session. Goodbye.");
                        this.Close();
                        btnSend.IsEnabled = false;
                        break;
                    case StatusCode.SendUserList:
                        for (int i = 1; i < message.Length; i++)
                        {
                            if (message[i] != Alias)
                            {
                                lbxUserList.Items.Add(message[i]);
                            }
                        }
                        break;
                    case StatusCode.All:
                        if (message[1] != Alias)
                        {
                            string broadcast = message[1] + ": " + message[2];
                            txtAll.Text += broadcast + "\n";
                            scrollAll.ScrollToBottom();
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
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbxUserList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbxUserList.SelectedItem != null)
            {
                selected = lbxUserList.SelectedItem.ToString();

                if (selected != Alias)
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
            string message = txtMsg.Text;
            if (message.Trim().Length > 0)
            {
                if (tbControl.SelectedItem == tbAll)
                {
                    txtAll.Text += "You: " + message + "\n";
                    scrollAll.ScrollToBottom();
                    message = SETMessengerUtilities.makeMessage(true, StatusCode.All, Alias, "all", message);
                }
                else if (tbControl.SelectedItem == tbPrivate)
                {
                    txtPrivate.Text += "(" + selected + ") You: " + message + "\n";
                    scrollPrivate.ScrollToBottom();
                    message = SETMessengerUtilities.makeMessage(true, StatusCode.Whisper, Alias, selected, message);
                }

                ClientPipe.sendMessage(message);
                if (!ClientPipe.connected)
                {
                    MessageBox.Show("The server had an unexpected shutdown. Closing application now...", "Server Fault");
                    this.Close();
                }
                txtMsg.Clear();
                txtMsg.Focus();

            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtMsg_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool enable = false;
            lblCharCount.Content = txtMsg.Text.Length + "/1000";
            
            if (txtMsg.Text.Length > 0)
            {
                if (tbControl.SelectedItem == tbPrivate)
                {
                    if (selected.Length > 0)
                    {
                        enable = true;
                    }
                }
                else
                {
                    enable = true;
                }
            }

            btnSend.IsEnabled = enable;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnSend_Click(sender, e);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tbControl.SelectedItem == tbAll)
            {
                lbxUserList.UnselectAll();
                lbxUserList.IsEnabled = false;
                if (txtMsg.Text.Length > 0)
                {
                    btnSend.IsEnabled = true;
                }
            }
            else if (tbControl.SelectedItem == tbPrivate)
            {
                tbPrivate.ClearValue(TabItem.BackgroundProperty);
                notification.Enabled = false;
                lbxUserList.IsEnabled = true;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private void newPrivateMessage()
        {
            if (tbControl.SelectedItem != tbPrivate)
            {
                notification.Enabled = true;
                notification.Start();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void notifiyUser(object sender, ElapsedEventArgs e)
        {

            Dispatcher.Invoke(() =>
            {
                if (toggle)
                {
                    tbPrivate.SetResourceReference(Control.BackgroundProperty, SystemColors.GradientInactiveCaptionBrushKey);
                    toggle = false;
                }
                else
                {
                    tbPrivate.ClearValue(TabItem.BackgroundProperty);
                    toggle = true;
                }
            });
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmMain_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            notification.Dispose();
            try
            {
                mq.close();
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("This computer requires MSMQ enabled.\n Please go to programs and features and enable Message Queues.", "Invalid Operation");
            }
        }
    }
}
