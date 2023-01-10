using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace MFR_GUI.Pages
{
    internal class Logger
    {
        private enum LogType { Info, Warning, Error };
        private readonly string _filePath = Globals.projectDirectory + "/TrainingFaces/log.txt";
        private readonly BlockingCollection<Param> _blockingCollection = new BlockingCollection<Param>();

        public Logger()
        {
            Task.Factory.StartNew(() =>
            {
                foreach (Param p in _blockingCollection.GetConsumingEnumerable())
                {
                    switch (p.LogType)
                    {
                        case LogType.Info:
                            using (StreamWriter sw = new StreamWriter(_filePath, true))
                            {
                                sw.WriteLine(p.Time.ToString("HH:mm:ss.fff") + "[INFO] " + p.Message);
                            }
                            break;
                        case LogType.Warning:
                            using (StreamWriter sw = new StreamWriter(_filePath, true))
                            {
                                sw.WriteLine(p.Time.ToString("HH:mm:ss.fff") + "[WARNING][" + p.Object + " in " + p.Trigger + "] " + p.Message);
                            }
                            break;
                        case LogType.Error:
                            using (StreamWriter sw = new StreamWriter(_filePath, true))
                            {
                                sw.WriteLine(p.Time.ToString("HH:mm:ss.fff") + "[ERROR][" + p.Object + " in " + p.Trigger + "] " + p.Message);
                            }
                            break;
                        default:
                            using (StreamWriter sw = new StreamWriter(_filePath, true))
                            {
                                sw.WriteLine(p.Time.ToString("HH:mm:ss.fff") + "[INFO] " + p.Message);
                            }
                            break;
                    }
                }
            });
        }

        public void LogInfo(string message)
        {
            Param p = new Param(LogType.Info, message);
            _blockingCollection.Add(p);
        }

        public void LogWarning(string message, string trigger, string obj)
        {
            Param p = new Param(LogType.Warning, message, trigger, obj);
            _blockingCollection.Add(p);
        }

        public void LogError(string message, string trigger, string obj)
        {
            Param p = new Param(LogType.Error, message, trigger, obj);
            _blockingCollection.Add(p);
        }

        private class Param
        {
            public LogType LogType { get; }     //Type of log
            public string Message { get; }      //Message
            public string Trigger { get; }      //Name of the file or method which triggered the Error/Warning
            public string Object { get; }       //Object that was processed when the Error/Warning occured
            public DateTime Time { get; }

            public Param()
            {
                LogType = LogType.Info;
                Message = "";
                Time = DateTime.Now;
            }

            public Param(LogType logType, string logMsg)
            {
                LogType = logType;
                Message = logMsg;
                Time = DateTime.Now;
            }

            public Param(LogType logType, string logMsg, string logAction, string logObj)
            {
                LogType = logType;
                Message = logMsg;
                Trigger = logAction;
                Object = logObj;
                Time = DateTime.Now;
            }
        }
    }
}