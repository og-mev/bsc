using CommandLine;
using Newtonsoft.Json;
using Tools;
using System;
using System.IO;
using System.Threading;
using System.IO.Pipes;
using System.Text;
using arbitrage_CSharp.Mode;
using arbitrage_CSharp.Tools;
using System.Net.WebSockets;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace arbitrage_CSharp
{

    class Program
    {
        static Strategy strategy;
        static WebSocketLink link;
        
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
            strategy = new Strategy(op.ConfigPath, SentMassageTo);
            strategy.StartAsync().Sync();
            //Thread.Sleep(20 * 1000);
            bool readScokect = true;
            if (readScokect)
            {
                //"ws://121.40.165.18:8800"
                ReadMassage(false, "ws://158.247.203.163:18080/txs", (result) => {
                    Logger.Debug(result);
                    DoExe(result);
                });
                
            }
            

            //strategy.AddTxAsync("0x885b1ef42bba87c199de03390d64dff218937493f424d47076c80ff2ad66542f").Sync();
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
        private static async void ReadMassage(bool usePipe = true, string pipeName = "testpipe",Action<string> readAc=null)
        {
            if (usePipe)
            {
                using (NamedPipeClientStream pipeStream = new NamedPipeClientStream(pipeName))
                {
                    pipeStream.Connect();
                    //在client读取server端写的数据
                    using (StreamReader rdr = new StreamReader(pipeStream))
                    {
                        string temp;
                        while ((temp = rdr.ReadLine()) != "stop")
                        {

                            DoExe(temp);
                        }
                    }
                }
            }
            else
            {
                ReadMassageWS(pipeName, readAc);
            }
           
        }
        static private void DoExe(string temp)
        {
            strategy.AddTxAsync(temp, false);
            Console.WriteLine("{0}:{1}", DateTime.Now, temp);
        }

        private static async Task ReadMassageWS(string url,Action<string> readAc)
        {
            var ws = new WebSocketLink(url, readAc );
            ws.ConnectAuthReceive();
            link = ws;
        }
        
        private static void SentMassageTo(string command, object obj)
        {
            SentMassage(command, obj,false);
        }
        private static void SentMassage(string command,object obj, bool usePipe = true, string pipeName = "testpipe")
        {
            string signs = obj as string;
            if (usePipe)
            {
                using (NamedPipeServerStream pipeStream = new NamedPipeServerStream("testpipe"))
                {
                    pipeStream.WaitForConnection();

                    using (StreamWriter writer = new StreamWriter(pipeStream))
                    {
                        writer.AutoFlush = true;
                        string temp;

                        //while ((temp = Console.ReadLine()) != "stop")
                        {
                            
   
                             writer.WriteLine(signs);
                            

                        }
                    }
                }

            }
            else
            {
                link.SendRequest(signs);
            }
            
        }

      
    }
    public class Options
    {
        [Option('c', "configuration", Required = true, HelpText = "Set configuration file path.")]
        public string ConfigPath { get; set; }
    }
}
