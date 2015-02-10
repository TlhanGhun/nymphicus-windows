using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Nymphicus;
using Nymphicus.Model;

namespace Nymphicus
{
    public class Logging
    {
        public string logFilePath { get; private set; }
        public ThreadSaveObservableCollection<DebugMessage> DebugMessages { get; set; }
        public bool OverrideDebugMessages_enableAnyway { get; set; }

        public Logging(string filePath)
        {
            OverrideDebugMessages_enableAnyway = false;
            logFilePath = filePath;
            try
            {
                File.WriteAllText(logFilePath, "Logging started at " + DateTime.Now.ToShortDateString() + ", " + DateTime.Now.ToShortTimeString() + "\r\n");
                File.AppendAllText(logFilePath, "Nymphicus for Windows " + Converter.prettyVersion.getNiceVersionString(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()) + "\r\n");
                File.AppendAllText(logFilePath, "Debug logging: " + Properties.Settings.Default.LogDebug.ToString() + "\r\n");
                File.AppendAllText(logFilePath, "Errog logging: " + Properties.Settings.Default.LogError.ToString() + "\r\n" + "\r\n");
            }
            catch
            {
                // deswegen wollen wir sicherlich nicht abstürzen, oder...?
            }
            DebugMessages = new ThreadSaveObservableCollection<DebugMessage>();
        }

        public void writeToLogfile(string text, bool isError)
        {
            if (isError && !Properties.Settings.Default.LogError)
            {
                return;
            }
            if (!isError && !Properties.Settings.Default.LogDebug)
            {
                return;
            }

            string logText = DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToLongTimeString() + ": " + text + "\r\n"; ;
            try
            {
                File.AppendAllText(logFilePath, logText);
                addDebugMessage("Log message", text, type: DebugMessage.DebugMessageTypes.LogMessage);
            }
            catch
            {
                // deswegen wollen wir sicherlich nicht abstürzen, oder...?
            }
        }

        public void writeToLogfile(string text)
        {
            writeToLogfile(text, false);
        }

        public void writeToLogfile(Exception exp)
        {
            writeToLogfile(exp, true);
        }

        public void writeToLogfile(Exception exp, bool isError)
        {
            if (isError && !Properties.Settings.Default.LogError)
            {
                return;
            }
            if (!isError && !Properties.Settings.Default.LogDebug)
            {
                return;
            }
            string logText = DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToLongTimeString() + ": " + exp.Message + "\r\n";
            logText += exp.StackTrace + "\r\n";
            if (exp.InnerException != null)
            {
                logText += "*** InnerException\r\n";
            }
            try
            {
                File.AppendAllText(logFilePath, logText);
                addDebugMessage(exp.Message, exp.StackTrace, type: DebugMessage.DebugMessageTypes.LogMessage);
            }
            catch
            {
                // deswegen wollen wir sicherlich nicht abstürzen, oder...?
            }
            if (exp.InnerException != null)
            {
                writeToLogfile(exp.InnerException);
            }
        }


        public void addDebugMessage(string title, string text, IItem item = null, IAccount account = null, View view = null, DebugMessage.DebugMessageTypes type = DebugMessage.DebugMessageTypes.General)
        {
            if (AppController.Current != null && (AppController.EnableDebugMessages || OverrideDebugMessages_enableAnyway))
            {
                DebugMessages.Add(new DebugMessage(title, text, item, account, view, type));
            }
        }
    }
}
