using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BWCS;

namespace ChatSystemClient
{
    /// <summary>
    /// Interaction logic for startupWindow.xaml
    /// </summary>
    public partial class startupWindow : Window
    {

        public startupWindow()
        {
            InitializeComponent();
            txtAlias.Focus();
        }



        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            bool? notEmpty = false;
            if (notEmpty == false)
            {
                notEmpty =checkEmpty(txtAlias, txtServerName);                
            }
            if(notEmpty == true)
            {
                if (txtAlias.Text.Contains(':'))
                {
                    txtAlias.Text = txtAlias.Text.Replace(':', ' ');
                }
                // Add logic to connect to the server
                MainWindow.Alias = txtAlias.Text.Trim();
                ClientPipe.ServerName = txtServerName.Text;
                int ret = ClientPipe.connectToServer();
                if (ret == 0)
                {
                    string msg = SETMessengerUtilities.makeMessage(true,StatusCode.ClientConnected, MainWindow.Alias);
                    ClientPipe.sendMessage(msg);
                    ClientPipe.connected = true;
                    this.Close();
                }
                else if (ret == 1)
                {
                    MessageBox.Show("Sorry, your connection timed out.", "Time out error");
                }
                //TODO: handle a timeout or fileIO exception

                
            }

        }

        private void txt_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            checkEmpty(textBox);
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            txtAlias.Clear();
            txtServerName.Clear();
        }

        private bool? checkEmpty(params TextBox[] text)
        {
            bool? retCode = null;
            foreach (TextBox textBox in text)
            {
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
