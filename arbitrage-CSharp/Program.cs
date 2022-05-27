using CommandLine;
using Newtonsoft.Json;
using Tools;
using System;
using System.IO;
using System.Threading;

namespace arbitrage_CSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledExceptionHundle);
            if (args.Length > 0)
            {
                Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(OnParsedHandler);
            }
            else
                Logger.Error("请输入配置表");
        }
        private static void OnParsedHandler(Options op)
        {
            Strategy it = new Strategy(op.ConfigPath);
            it.StartAsync();
            while (true)
            {
                Thread.Sleep(1 * 1000);
            }

        }

        /// <summary>
        /// 抛出未经处理异常
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="isTerminating"></param>
        static private void UnhandledExceptionHundle(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Error((Exception)e.ExceptionObject);
        }
    }
    public class Options
    {
        [Option('c', "configuration", Required = true, HelpText = "Set configuration file path.")]
        public string ConfigPath { get; set; }
    }
}
