using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace Nymphicus
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public void Application_Startup(object sender, StartupEventArgs e)
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            try
            {
                AppController.Start();
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.StackTrace, exp.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                //MessageBox.Show(e.ex StackTrace, e.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch { }

        }

        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
               // MessageBox.Show(e.Exception.StackTrace, e.Exception.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch { }

            e.Handled = true;
        }
        
    }
}
