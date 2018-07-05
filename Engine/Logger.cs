using System;

namespace Engine
{
    public enum LogType
    {
        Debug,
        Normal,
        Success,
        Failure,
        Warning,
        Error,
        Critical,
        Exception
    }

    public enum LoggerType
    {
        Console,
        File,
        Console_File
    }

    public class Logger : IDisposable
    {
        private LoggerType loggerType;
        private string datetimeFormat;
        private string logFilename;
        private string logOwner;

        public Logger(LoggerType loggingType, string loggerOwner = null)
        {
            loggerType = loggingType;
            logOwner = loggerOwner ?? "";

            if (loggingType == LoggerType.File | loggingType == LoggerType.Console_File)
            {
                datetimeFormat = "yyyy-MM-dd HH:mm:ss.fff";
                logFilename = loggerOwner + ".log";

                string logHeader = " __      __.__      __________.______________" + Environment.NewLine +
                                   @"/  \    /  \__| ____\______   \   \__    ___/" + Environment.NewLine +
                                   @"\   \/\/   /  |/    \|     ___/   | |    |   " + Environment.NewLine +
                                   @" \        /|  |   |  \    |   |   | |    |   " + Environment.NewLine +
                                   @"  \__/\  / |__|___|  /____|   |___| |____|   " + Environment.NewLine +
                                   @"       \/          \/                        " + Environment.NewLine +
                                   "     [Windows Process Injection Toolkit]     " + Environment.NewLine + Environment.NewLine;
                if (!System.IO.File.Exists(logFilename))
                {
                    WriteLine(logHeader, false);
                }
                else
                {
                    WriteLine(Environment.NewLine + Environment.NewLine + "[START NEW LOGGING PROCESS: " + DateTime.Now.ToString(datetimeFormat) + "]" + Environment.NewLine);
                }
            }
        }

        public void Log(LogType lType, string format, params object[] args)
        {
            WriteFormattedLog(lType, string.Format(format, args));
        }

        public void Log(string format, params object[] args)
        {
            Log(LogType.Normal, format, args);
        }

        public void Log(Exception exception, string format, params object[] args)
        {
            string ex = $"Exception: {exception.ToString()}\r\n";
            Log(LogType.Exception, ex + format, args);
        }

#if DEBUG
        /// <summary>
        /// Logs the input
        /// Using <see cref="System.Diagnostics.Debug"/> to print directly
        /// to the debugger aswell...
        /// </summary>
        /// <param name="format">Pre-formatted text</param>
        /// <param name="args">Optionally set arguments if formatted characters is used</param>
        public void Debug(string format, params object[] args)
        {
            Log(LogType.Debug, format, args);
            System.Diagnostics.Debug.Print(format, args);
        }
#endif

        void WriteFormattedLog(LogType level, string text)
        {
            switch (loggerType)
            {
                case LoggerType.Console:
                    WriteConsole(level, text);
                    break;
                case LoggerType.File:
                    WriteFile(level, text);
                    break;
                case LoggerType.Console_File:
                    WriteConsole(level, text);
                    WriteFile(level, text);
                    break;
            }
        }
        void WriteFile(LogType level, string text)
        {
            string pretext;

            switch (level)
            {
                case LogType.Normal:
                    pretext = System.DateTime.Now.ToString(datetimeFormat) + " [INFO]    ";
                    break;
                case LogType.Debug:
                    pretext = System.DateTime.Now.ToString(datetimeFormat) + " [DEBUG]   ";
                    break;
                case LogType.Warning:
                    pretext = System.DateTime.Now.ToString(datetimeFormat) + " [WARNING] ";
                    break;
                case LogType.Error:
                    pretext = System.DateTime.Now.ToString(datetimeFormat) + " [ERROR]   ";
                    break;
                case LogType.Critical:
                    pretext = System.DateTime.Now.ToString(datetimeFormat) + " [CRITICAL]   ";
                    break;
                case LogType.Exception:
                    pretext = System.DateTime.Now.ToString(datetimeFormat) + " [EXCEPTION]   ";
                    break;
                case LogType.Failure:
                    pretext = System.DateTime.Now.ToString(datetimeFormat) + " [FAILURE]   ";
                    break;
                case LogType.Success:
                    pretext = System.DateTime.Now.ToString(datetimeFormat) + " [SUCCESS]   ";
                    break;
                default:
                    pretext = "";
                    break;
            }
            WriteLine(pretext + text);
        }
        void WriteConsole(LogType level, string text)
        {
            ConsoleColor orgCol = Console.ForegroundColor;

            switch (level)
            {
                case LogType.Success:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write(System.DateTime.Now.ToString(datetimeFormat));
                    Console.ForegroundColor = orgCol;
                    Console.Write(" (");
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write(logOwner);
                    Console.ForegroundColor = orgCol;
                    Console.Write(") [");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("SUCCESS");
                    Console.ForegroundColor = orgCol;
                    Console.Write("] ");
                    Console.Write(text);
                    Console.Write(Environment.NewLine);
                    break;
                case LogType.Normal:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write(System.DateTime.Now.ToString(datetimeFormat));
                    Console.ForegroundColor = orgCol;
                    Console.Write(" (");
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write(logOwner);
                    Console.ForegroundColor = orgCol;
                    Console.Write(") ");
                    Console.Write(" -> ");
                    Console.Write(text);
                    Console.Write(Environment.NewLine);
                    break;
                case LogType.Warning:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write(System.DateTime.Now.ToString(datetimeFormat));
                    Console.ForegroundColor = orgCol;
                    Console.Write(" (");
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write(logOwner);
                    Console.ForegroundColor = orgCol;
                    Console.Write(") [");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("WARNING");
                    Console.ForegroundColor = orgCol;
                    Console.Write("] ");
                    Console.Write(text);
                    Console.Write(Environment.NewLine);
                    break;
                case LogType.Critical:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write(System.DateTime.Now.ToString(datetimeFormat));
                    Console.ForegroundColor = orgCol;
                    Console.Write(" (");
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write(logOwner);
                    Console.ForegroundColor = orgCol;
                    Console.Write(") [");
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write("CRITICAL");
                    Console.ForegroundColor = orgCol;
                    Console.Write("] ");
                    Console.Write(text);
                    Console.Write(Environment.NewLine);
                    break;
                case LogType.Debug:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write(System.DateTime.Now.ToString(datetimeFormat));
                    Console.ForegroundColor = orgCol;
                    Console.Write(" (");
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write(logOwner);
                    Console.ForegroundColor = orgCol;
                    Console.Write(") [");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("DEBUG");
                    Console.ForegroundColor = orgCol;
                    Console.Write("] ");
                    Console.Write(text);
                    Console.Write(Environment.NewLine);
                    break;
                case LogType.Error:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write(System.DateTime.Now.ToString(datetimeFormat));
                    Console.ForegroundColor = orgCol;
                    Console.Write(" (");
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write(logOwner);
                    Console.ForegroundColor = orgCol;
                    Console.Write(") [");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("ERROR");
                    Console.ForegroundColor = orgCol;
                    Console.Write("] ");
                    Console.Write(text);
                    Console.Write(Environment.NewLine);
                    break;
                case LogType.Exception:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write(System.DateTime.Now.ToString(datetimeFormat));
                    Console.ForegroundColor = orgCol;
                    Console.Write(" (");
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write(logOwner);
                    Console.ForegroundColor = orgCol;
                    Console.Write(") [");
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Write("EXCEPTION");
                    Console.ForegroundColor = orgCol;
                    Console.Write("] ");
                    Console.Write(text);
                    Console.Write(Environment.NewLine);
                    break;
                case LogType.Failure:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write(System.DateTime.Now.ToString(datetimeFormat));
                    Console.ForegroundColor = orgCol;
                    Console.Write(" (");
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write(logOwner);
                    Console.ForegroundColor = orgCol;
                    Console.Write(") [");
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write("FAILURE");
                    Console.ForegroundColor = orgCol;
                    Console.Write("] ");
                    Console.Write(text);
                    Console.Write(Environment.NewLine);
                    break;
            }
        }
        void WriteLine(string text, bool append = true)
        {
            try
            {
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(logFilename, append, System.Text.Encoding.UTF8))
                {
                    if (text != "")
                    {
                        writer.WriteLine(text);
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public void Dispose()
        {
            if (loggerType == LoggerType.File | loggerType == LoggerType.Console_File)
                WriteLine("[END OF LOG]" + Environment.NewLine + Environment.NewLine);
        }
    }
}
