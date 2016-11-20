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
    /*
    Name:
    Description:
    */
    public partial class MainWindow : Window
    {
        public static string Alias { get; set; }

        // Variables for the notification of the private tab
        private bool toggle = true;
        private System.Timers.Timer notification = new System.Timers.Timer(500);

        // variable to store the currently selected user in the userList lisbox
        private string selected = "";

        private ClientQueue mq;

        /*
        Name: MainWindow()
        Description: this is the main window constructor of the application. It creates a new client queue.
                     It then runs the startup window. Once the start up window closes, the client
                     spawns a new thread to read messages from the server.
        */
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



        /*
        Name: readLoop
        Description: This method start a loop to read messages from the message queue. When it recieves one, 
                     it will pass it off to be handled. The loop will continue to process new messages
                     until the application is closed
        */
        public void readLoop()
        {
            while (true)
            {
                try
                {
                    //Reads from the message queue
                    string message = mq.GetMessages();
                    //passes the message to the receiveMsg function to be processed
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


        /*
        Name: receiveMsg
        Parameters: string read -> this is the message read from the message queue
        Description: the function take the received message, determines what kind of message it is
                     and then executes the appropriate action.
        */
        public void receiveMsg(string read)
        {
            //parses out the statuc code from the message
            StatusCode state = (StatusCode)int.Parse(read.Substring(1, 1));
            char[] seperator = { ':' };
            string[] message;

            //if the status code is 2, then we split the message a certain way
            //if its not, then we split it the whole message up as much as we can
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
                        //writes to the screen that a client connected
                        txtAll.Text += message[1] + " has connected.\n";
                        //goes to the bottom of the textblock
                        scrollAll.ScrollToBottom();
                        //if message[1] is the user's name
                        if (message[1] == Alias)
                        {
                            //insert the name to the top of the user list
                            lbxUserList.Items.Insert(0, message[1]);
                        }
                        else
                        {
                            //if it's not, then insert it into the bottom
                            lbxUserList.Items.Add(message[1]);
                        }
                        break;
                    case StatusCode.ClientDisconnected:
                        //
                        txtAll.Text += message[1] + " has disconnected.\n";
                        scrollAll.ScrollToBottom();
                        //if the client that diconnected was selected, then unselect them from the userlist
                        if ((string)lbxUserList.SelectedItem == message[1])
                        {
                            lbxUserList.UnselectAll();
                        }
                        //removes the user from the user list
                        lbxUserList.Items.Remove(message[1]);
                        break;
                    case StatusCode.Whisper:
                        string msg = message[1] + ": " + message[2];
                        //write the message to the private tab
                        txtPrivate.Text += msg + "\n";
                        //starts the timer to show that there is a new private message for the user
                        newPrivateMessage();
                        break;
                    case StatusCode.ServerClosing:
                        //when the server closes, it tells the user, and the client has to close the window
                        MessageBox.Show("The server is now closing. Exiting your session. Goodbye.");
                        this.Close();
                        //disables the send button
                        btnSend.IsEnabled = false;
                        break;
                    case StatusCode.SendUserList:
                        //reads in the whole user list sent through the message
                        for (int i = 1; i < message.Length; i++)
                        {
                            //doesn't add the current client back into the list
                            if (message[i] != Alias)
                            {
                                //adding each user to the user list
                                lbxUserList.Items.Add(message[i]);
                            }
                        }
                        break;
                    case StatusCode.All:
                        //writes the received message to the all text block if the message
                        //was not sent by the current user.
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


        /*
        Name: lbxUserList_SelectionChanged
        Description: when the user clicks on a name in the user list that isn't their own name,
                     the send button is enabled.
        */
        private void lbxUserList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbxUserList.SelectedItem != null)
            {
                selected = lbxUserList.SelectedItem.ToString();

                //if the user did not select their own name
                if (selected != Alias)
                {
                    //if there is something in the message box, the send button is enabled
                    if (txtMsg.Text.Trim().Length > 0)
                    {
                        btnSend.IsEnabled = true;
                    }
                }
                else
                {
                    //disable the button
                    btnSend.IsEnabled = false;
                    selected = "";
                }
            }
        }


        /*
        Name: btnSend_Click
        Description:
        */
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


        /*
        Name: txtMsg_TextChanged
        Description: 
        */
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


        /*
        Name: frmMain_KeyDown
        Description: If the user hits enter, then try to send the message
        */
        private void frmMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnSend_Click(sender, e);
            }
        }


        /*
        Name: tbControl_SelectionChanged
        Description:
        */
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


        /*
        Name: newPrivateMessage
        Description: starts the timer for the tab blink when a new private message is received
        */
        private void newPrivateMessage()
        {
            if (tbControl.SelectedItem != tbPrivate)
            {
                notification.Enabled = true;
                notification.Start();
            }
        }


        /*
        Name: notifiyUser
        Description: toggles the back ground colour of the private tab
        */
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


        /*
        Name: frmMain_Closing
        Description: when the app is closed, it disposes the timer, and closes the message queue
        */
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
