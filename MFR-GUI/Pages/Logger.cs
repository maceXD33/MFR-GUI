using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFR_GUI.Pages
{
    internal class Logger
    {
        private static string filePath = Globals.projectDirectory + "/TrainingFaces/log.txt";

        public static void LogError(string tag, string message)
        {
            Task t = Task.Factory.StartNew(() =>
            {
                DateTime now = DateTime.Now;

                using (StreamWriter sw = new StreamWriter(filePath, true))
                {
                    sw.WriteLine(now.ToString("HH:mm:ss.fff") + "[ERROR][" + tag + "] " + message);
                }
            });
        }

        public static void LogInfo(string tag, string message)
        {
            Task t = Task.Factory.StartNew(() =>
            {
                DateTime now = DateTime.Now;

                using (StreamWriter sw = new StreamWriter(filePath, true))
                {
                    sw.WriteLine(now.ToString("HH:mm:ss.fff") + "[INFO][" + tag + "] " + message);
                }
            });
        }
    }
}