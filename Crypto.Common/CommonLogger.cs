using Crypto.Interface;
using Serilog;
using Serilog.Core;
using Serilog.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Common
{
    internal class CommonLogger : ICommonLogger
    {
        private Logger m_oLogger;
        private ICryptoSetup m_oSetup;

        private enum eLogType
        {
            Information,
            Debug,
            Warning,
            Error,
        }
        private class LogData
        {
            public eLogType LogType { get; set; } = eLogType.Information;

            public string Message { get; set; } = string.Empty;

            public Exception? Exception { get; set; }


        }

        private ConcurrentQueue<LogData> m_aQueue = new ConcurrentQueue<LogData>();

        private CancellationToken m_oToken;
        private Task m_oTask;

        public CommonLogger(ICryptoSetup oSetup, string strFile, CancellationToken oToken)
        {
            m_oSetup = oSetup;
            m_oToken = oToken;
            m_oTask = LogLoop();

            String logTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u}] {Message}{NewLine}{Exception}";
            string strLogFile = string.Format("{0}/{1}_{2}.txt", oSetup.LogPath, strFile, DateTime.Today.ToString("yyyyMMdd"));
            m_oLogger = new LoggerConfiguration()
                .Enrich.WithExceptionDetails()
                .MinimumLevel.Information()
                .WriteTo.Console(outputTemplate: logTemplate)
                .WriteTo.File(strLogFile, outputTemplate: logTemplate, flushToDiskInterval: TimeSpan.FromSeconds(10))
                .CreateLogger();

        }


        private async Task LogLoop()
        {
            while (!m_oToken.IsCancellationRequested)
            {
                LogData? oLog = null;
                if (m_aQueue.TryDequeue(out oLog))
                {
                    switch (oLog.LogType)
                    {
                        case eLogType.Information:
                            m_oLogger.Information(oLog.Message);
                            break;
                        case eLogType.Debug:
                            m_oLogger.Debug(oLog.Message);
                            break;
                        case eLogType.Warning:
                            m_oLogger.Warning(oLog.Message, oLog.Exception);
                            break;
                        case eLogType.Error:
                            // m_oLogger.Error(oLog.Message, oLog.Exception);
                            m_oLogger.Error(oLog.Exception, oLog.Message + " !!!!! Exception !!!!!");
                            break;
                    }
                }
                else await Task.Delay(200);
            }
        }

        public void Info(string strMessage)
        {
            m_aQueue.Enqueue(new LogData() { LogType = eLogType.Information, Message = strMessage, Exception = null });
        }

        public void Warning(string strMessage, Exception? ex = null)
        {
            m_aQueue.Enqueue(new LogData() { LogType = eLogType.Warning, Message = strMessage, Exception = ex });
        }

        public void Error(string strMessage, Exception? ex = null)
        {
            m_aQueue.Enqueue(new LogData() { LogType = eLogType.Error, Message = strMessage, Exception = ex });
        }
    }
}
