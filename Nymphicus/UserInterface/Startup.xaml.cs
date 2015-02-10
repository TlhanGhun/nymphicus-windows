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

namespace Nymphicus.UserInterface
{
    /// <summary>
    /// Interaction logic for Startup.xaml
    /// </summary>
    public partial class Startup : Window
    {
        public Startup()
        {
            InitializeComponent();
            Version installedVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            LabelNymphicusVersion.Content = "Nymphicus " + Converter.prettyVersion.getNiceVersionString(installedVersion.ToString());
        }
    }
}
