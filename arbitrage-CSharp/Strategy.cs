using Tools;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace arbitrage_CSharp
{
    class Strategy
    {
        Config config;
        public Strategy(string ConfigPath)
        {
            Config? config = null;
            if (File.Exists(ConfigPath))
            {
                string text = File.ReadAllText(ConfigPath);
                if (!string.IsNullOrEmpty(text))
                {
                    config = JsonConvert.DeserializeObject<Config>(text);
                }
                else
                {
                    Logger.Debug("data is empty :" + ConfigPath);
                }
            }
            else
            {
                Logger.Error("配置表路径错误！！");
                return;
            }
            if (config != null)
            {
                this.config = config.Value;
            }
        }

        public async void StartAsync()
        {
            var bridge = new SwapBridge("BSC");
            await Task.Delay(2000);
//             Tuple swapArr = new
//             {
//                 symbol = "pancakeswap",
//                 amountIn = 10,
//                 amountOutMin= 10,
//                 path=new[] { "0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56", "0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56", "0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56" }
//             };
            bridge.swap("pancakeswap",10,10,new List<string>() { "0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56", "0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56", "0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56" });
        }



        
        
    }

    public struct Config
    {
        public string symbol;

     
        public string address;

        public string abiPath;
    }
}
