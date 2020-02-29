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

namespace RabbitConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Rabbit _converter = null;

        public MainWindow()
        {
            InitializeComponent();
            this._converter = new Rabbit();

            this.txtUnicode.Text = @"သီဟိုဠ်မှ ဉာဏ်ကြီးရှင်သည် အာယုဝဍ္ဎနဆေးညွှန်းစာကို ဇလွန်ဈေးဘေး ဗာဒံပင်ထက် အဓိဋ္ဌာန်လျက် ဂဃနဏဖတ်ခဲ့သည်။";
            this.txtZawgyi.Text = this._converter.Uni2Zg(this.txtUnicode.Text);
        }

        private void CommonCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void onClear_Click(object sender, RoutedEventArgs e) 
        {
            this.txtZawgyi.Text = string.Empty;
            this.txtUnicode.Text = string.Empty;
        }

        private void onCopyZawGyi_Click(object sender, RoutedEventArgs e) 
        {
            if (!string.IsNullOrEmpty(this.txtZawgyi.Text))
            {
                this.txtZawgyi.SelectAll();
                this.txtZawgyi.Copy();
            }
        }

        private void onCopyUnicode_Click(object sender, RoutedEventArgs e) 
        {
            if (!string.IsNullOrEmpty(this.txtUnicode.Text))
            {
                this.txtUnicode.SelectAll();
                this.txtUnicode.Copy();
            }
        }

        private void txtZawgyi_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.txtZawgyi.IsFocused)
            {
                this.txtUnicode.Text = this._converter.Zg2Uni(this.txtZawgyi.Text);
            }
        }

        private void txtUnicode_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.txtUnicode.IsFocused)
            {
                this.txtZawgyi.Text = this._converter.Uni2Zg(this.txtUnicode.Text);
            }
        }
    }
}
