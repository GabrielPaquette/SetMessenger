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
using System.Windows.Shapes;

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
        }



        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {            
            MainWindow.Alias = "test";
            this.Close();
        }

        private void txt_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text.Trim() == "")
            {
                textBox.BorderBrush = Brushes.Red;
                textBox.BorderThickness = new Thickness(2);
            }
            else
            {
                textBox.ClearValue(TextBox.BorderBrushProperty);
                textBox.ClearValue(TextBox.BorderThicknessProperty);
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            txtAlias.Clear();
            txtServerName.Clear();
        }
    }
}
