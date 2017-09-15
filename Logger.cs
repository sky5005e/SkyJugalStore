using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Data
{
    public static class Logger
    {
        public static string LogPath = @"C:\temp\Log-" + DateTime.UtcNow.ToString("yyyy-MM-dd") + ".txt";
        public static void LogError(Exception ex)
        {
            string message = string.Format("Time: {0}", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"));
            message += Environment.NewLine;
            message += "-----------------------------------------------------------";
            message += Environment.NewLine;
            message += string.Format("Message: {0}", ex.Message);
            message += Environment.NewLine;
            message += string.Format("StackTrace: {0}", ex.StackTrace);
            message += Environment.NewLine;
            message += string.Format("Source: {0}", ex.Source);
            message += Environment.NewLine;
            message += string.Format("TargetSite: {0}", ex.TargetSite.ToString());
            message += Environment.NewLine;
            message += "-----------------------------------------------------------";
            message += Environment.NewLine;

            using (var writer = new System.IO.StreamWriter(LogPath, true))
            {
                writer.WriteLine(message);
                writer.Close();
            }
        }
        public static void LogMessage(string logMessage)
        {
            string message = string.Format("Time: {0}", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"));
            message += Environment.NewLine;
            message += "-----------------------------------------------------------";
            message += Environment.NewLine;
            message += logMessage;
            message += "-----------------------------------------------------------";
            using (var writer = new System.IO.StreamWriter(LogPath, true))
            {
                writer.WriteLine(message);
                writer.Close();
            }
        }
    }
}


/// <summary>
        /// General Application Error
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Application_Error(object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError();
            Logger.LogError(ex);
        }
