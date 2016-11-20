/*
Project: ChatSystemService - startupWindow.xaml.cs
Developer(s): Gabriel Paquette, Nathaniel Bray
Date: November 19, 2016
Description: This file contains the code that lets the user enter their username and
             server name, and then lets them try to connect to the server.
             
*/


using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BWCS;

namespace ChatSystemClient
{
    public partial class startupWindow : Window
    {

        /*
        Name: startupWindow
        Description: This function initalized the components for the applications
                     and sets the focus to the alias field.
        */
        public startupWindow()
        {
            InitializeComponent();
            txtAlias.Focus();
        }


        /*
        Name: btnConnect_Click
        Description: The function handles when the user clicks the connect button.
                     if both the alias field and the server name field are filled out,
                     the client will try to connect to the server.
        */
        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            bool? notEmpty = false;
            if (notEmpty == false)
            {
                //checks if both fields are filled out
                notEmpty =checkEmpty(txtAlias, txtServerName);                
            }
            if(notEmpty == true)
            {
                //colons are not allowed, GET OUT OF HERE TROLLS
                if (txtAlias.Text.Contains(':'))
                {
                    txtAlias.Text = txtAlias.Text.Replace(':', ' ');
                }
                //take out white spaces on either side of the string
                ClientPipe.Alias = txtAlias.Text.Trim();
                ClientPipe.ServerName = txtServerName.Text;
                //try to connect to the server
                int ret = ClientPipe.connectToServer();
                //if ret is 0, the connect worked
                if (ret == 0)
                {
                    string msg = PipeClass.makeMessage(true,StatusCode.ClientConnected, ClientPipe.Alias);
                    //send the connection message to the server
                    ClientPipe.sendMessage(msg);
                    ClientPipe.connected = true;
                    //closes the start up window so the main window can run
                    this.Close();
                }
                else if (ret == 1)
                {
                    MessageBox.Show("Sorry, your connection timed out.", "Time out error");
                }
            }

        }


        /*
        Name: txt_LostFocus
        Description: This function is called the a textfield loses focus. It calls the
                     check empty function to see if the field is empty.
        */
        private void txt_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            checkEmpty(textBox);
        }


        /*
        Name: btnReset_Click
        Description: This function resets the values in the alias and serverName to blank
        */
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            txtAlias.Clear();
            txtServerName.Clear();
        }


        /*
        Name: checkEmpty
        Parameters: params TexTBox text -> this holds an array of the alias field and the server name fied
        Description: Checks if the alias and server name field are blank. If they are
                     indicated they are blank by making the text boxes red
        Return: return false if they are emply, and true if they are not
        */
        private bool? checkEmpty(params TextBox[] text)
        {
            bool? retCode = null;
            //checks each textbox in the array
            foreach (TextBox textBox in text)
            {
                //if this returns "", then the field was empty
                if (textBox.Text.Trim() == "")
                {
                    textBox.BorderBrush = Brushes.Red;
                    textBox.BorderThickness = new Thickness(2);
                    retCode = false;
                }
                else
                {
                    textBox.ClearValue(TextBox.BorderBrushProperty);
                    textBox.ClearValue(TextBox.BorderThicknessProperty);
                    if (retCode == false)
                    {
                        retCode = false;
                    }
                    else
                    {
                        retCode = true;
                    }
                    
                }
            }
            return retCode;
        }

    }
}
