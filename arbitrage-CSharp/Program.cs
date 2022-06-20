using CommandLine;
using Newtonsoft.Json;
using Tools;
using System;
using System.IO;
using System.Threading;
using System.IO.Pipes;
using System.Text;
using arbitrage_CSharp.Mode;

namespace arbitrage_CSharp
{

    class Program
    {
        static Strategy strategy;

        
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
            strategy = new Strategy(op.ConfigPath, SentMassage);
            strategy.StartAsync();
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
        private async void ReadMassage(string pipeName = "testpipe")
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
                        string tx = JsonConvert.DeserializeObject<string>(temp);
                        //await strategy.OnTxChangeAsync(tx);
                        Console.WriteLine("{0}:{1}", DateTime.Now, temp);
                    }
                }
            }
        }
        private static void SentMassage(string sign,object obj)
        {
            Decoder decoder = Encoding.UTF8.GetDecoder();
            Byte[] bytes = new Byte[10];
            Char[] chars = new Char[10];
            using (NamedPipeClientStream pipeStream = new NamedPipeClientStream("messagepipe"))
            {
                pipeStream.Connect();
                pipeStream.ReadMode = PipeTransmissionMode.Message;
                int numBytes;
                do
                {
                    string message = "";

                    do
                    {
                        numBytes = pipeStream.Read(bytes, 0, bytes.Length);
                        int numChars = decoder.GetChars(bytes, 0, numBytes, chars, 0);
                        message += new String(chars, 0, numChars);
                    } while (!pipeStream.IsMessageComplete);

                    decoder.Reset();
                    Console.WriteLine(message);
                } while (numBytes != 0);
            }
        }
    }
    public class Options
    {
        [Option('c', "configuration", Required = true, HelpText = "Set configuration file path.")]
        public string ConfigPath { get; set; }
    }
}
