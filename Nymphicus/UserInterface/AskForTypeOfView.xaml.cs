using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Nymphicus.Model;

namespace Nymphicus.UserInterface
{
    /// <summary>
    /// Interaction logic for AskForTypeOfView.xaml
    /// </summary>
    public partial class AskForTypeOfView : Window
    {
        public View view { get; set; }

        public AskForTypeOfView(View toBeAskedForView)
        {
            InitializeComponent();
            view = toBeAskedForView;
        }

        private void buttonGoAhead_Click_1(object sender, RoutedEventArgs e)
        {
            if (radiobuttonTwitterOnly.IsChecked.Value)
            {
                view.isTwitterOnlyView = true;
            }
            Close();
        }
    }
}
