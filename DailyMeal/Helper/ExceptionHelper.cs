using System;
using System.IO;
using System.Windows.Forms;

namespace DailyMeal.Helper
{
    public static class ExceptionHelper
    {
        private static readonly object _lock = new object();
        private static readonly string LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

        public static void HandleThreadException(Exception ex)
        {
            LogException(ex);
            try
            {
                MessageBox.Show($"程序发生错误：\n{ex.Message}\n\n详细信息已记录至日志。", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch { }
        }

        public static void HandleUnhandledException(Exception ex)
        {
            LogException(ex);
        }

        public static void LogException(Exception ex)
        {
            try
            {
                lock (_lock)
                {
                    if (!Directory.Exists(LogDirectory))
                        Directory.CreateDirectory(LogDirectory);
                    string logFile = Path.Combine(LogDirectory, $"error_{DateTime.Now:yyyyMMdd}.txt");
                    string logContent = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ERROR] {ex.GetType().Name}\nMessage: {ex.Message}\nStackTrace: {ex.StackTrace}\n\n";
                    File.AppendAllText(logFile, logContent);
                }
            }
            catch { }
        }
    }
}
