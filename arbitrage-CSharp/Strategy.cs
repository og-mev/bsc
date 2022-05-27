using Tools;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

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
            var flashswapAddr = JObject.Parse(File.ReadAllText("../contract/deploy.json"))["address"].ToString();
            var bridge = new SwapBridge("BSC", flashswapAddr);
            await Task.Delay(2000);
            var swapArr = new List<(string symbol, decimal amountIn, decimal amountOutMin, string[] path)> {
            };
            var swap = new
            {
                symbol = "pancakeswap",
                amountIn = 2.0m,
                amountOutMin = 1.0m,
                path = new[] { "0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56", "0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56", "0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56" }
            };
            swapArr.Add(swap);
            await bridge.import_wallets("0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80");
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
