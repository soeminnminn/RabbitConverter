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
using RabbitConverter.Models;

namespace RabbitConverter.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainViewModel model;
        public MainWindow()
        {
            InitializeComponent();

            model = new MainViewModel();
            DataContext = model;
        }

        private void ExitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void AboutCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show(this, "Develop by Soe Minn Minn\nCopyright © S16 Co. Ltd. 2023\nThe Open Source Project",
                "About - Rabbit Converter",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
