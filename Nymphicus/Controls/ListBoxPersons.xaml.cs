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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Nymphicus.Model;

namespace Nymphicus.Controls
{
    /// <summary>
    /// Interaction logic for ListBoxPersons.xaml
    /// </summary>
    public partial class ListBoxPersons : UserControl
    {
        public ListBoxPersons()
        {
            InitializeComponent();
        }

        public List<Person> getSelectedPersons()
        {
            List<Person> selectedPersons = new List<Person>();
            

            return selectedPersons;
        }
    }
}
