using Tools;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using arbitrage_CSharp.Tools;
using Nethereum.Contracts;
using Nethereum.Web3;

using Nethereum.Uniswap.Contracts.UniswapV2Factory;

using Nethereum.Client;

namespace arbitrage_CSharp
{
    class Strategy
    {
        private readonly EthereumClientIntegrationFixture _ethereumClientIntegrationFixture;


        Config config;

        public Strategy(string ConfigPath)
        {
            Config config = null;
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
                this.config = config;
            }

            //_ethereumClientIntegrationFixture = new EthereumClientIntegrationFixture();
        }

        public async void StartAsync()
        {
            Algorithms.Test();
               //await GetDatasByContractAsync();
               //poolDatas
               var poolPairsDic = GetDatasByFile();
            Dictionary<string, string> tokenList = new Dictionary<string, string>();
            //获取有多少种类
            foreach (var item in poolPairsDic)
            {
                tokenList[item.Value.poolTokenA.tokenAddress] = "";
                tokenList[item.Value.poolTokenB.tokenAddress] = "";
            }
            //设置标识数字方便看path
            int i = 0;
            foreach (var item in tokenList)
            {
                tokenList[ item.Key] = "_"+i+++"_";
            }
            // 构成图，把相同的 地址能连接的放到一起
            foreach (var item in tokenList)
            {
                string id = item.Key;
                foreach (var poolPair in poolPairsDic)
                {
                    if (poolPair.Value.poolTokenA.tokenAddress == poolPair.Value.poolTokenB.tokenAddress)
                    {
                        
                    }
                    else
                    {

                    }

                }
            }


            RedisDB.Init(config.RedisConfig);
            var flashswapAddr = JObject.Parse(File.ReadAllText("../contract/deploy.json"))["address"].ToString();
            var bridge = new SwapBridge("BSC", flashswapAddr);

            //1 拉取 redis 获取 全路径，并且监听更新
            //RedisDB.Instance.StringGet<T>(DBKey);

            //2 监听 peending  tx

            //3 根据 tx 的交易对 获取所有对应路径

            //4 根据盈利比例计算出所有可兑换的路径，以及最大兑换数量

            //5 签名后发给 ray











            //根据算法来交换 币
            var swapArr = new List<(string symbol, decimal amountIn, decimal amountOutMin, string[] path)>();
            (string symbol, decimal amountIn, decimal amountOutMin, string[] path) swap = ("pancakeswap", 2.0m, 1.0m, new string[] { "0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56", "0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56", "0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56" });
            
            swapArr.Add(swap);
            await bridge.import_wallets("0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80");
            bridge.swap(swapArr);
        }


        #region 工具
        public async Task<Dictionary<string, PoolPairs>> GetDatasByContractAsync()
        {
            //通过合约拉取
            var web3 = _ethereumClientIntegrationFixture.GetWeb3();
            /*
            var factoryAddress = "0x5C69bEe701ef814a2B6a3EDD4B1652CB9cc5aA6f";
            var factoryService = new UniswapV2FactoryService(web3, factoryAddress);
            var weth = "0xc02aaa39b223fe8d0a0e5c4f27ead9083c756cc2";
            var dai = "0x6b175474e89094c44da98b954eedeac495271d0f";
            var pair = await factoryService.GetPairQueryAsync(weth, dai);
            */

            //获取全部paris
            //1 获取allPairsLength
            //2 按长度循环调用 allPairs
            List<string> symbolAddressList = new List<string>();

            string factoryAbiStr = File.ReadAllText(config.uniswapV3_factoryAbi);

            string pairAbiStr = File.ReadAllText(config.uniswapV3_pairAbi);
            var factoryContract = web3.Eth.GetContract(factoryAbiStr, config.unswapV2_address);
            Logger.Debug($"contract {factoryContract.ToString()}");
            int count = await factoryContract.GetFunction("allPairsLength")
                .CallAsync<int>();
            Logger.Debug($"交易对数量 {count}");

            //存放所有的 池里面的数据
            Dictionary<string, PoolPairs> allPoolDic = new Dictionary<string, PoolPairs>();
            //先只要100个
            for (int i = 0; i < 100; i++)
            {
                
                var allPairs = factoryContract.GetFunction("allPairs");
                string pairAddress = await allPairs.CallAsync<string>(i);
                symbolAddressList.Add(pairAddress);
                Logger.Debug($"pairAddress {pairAddress}");

                //获取每个交易对的 数量和地址
                var pairContract = web3.Eth.GetContract(pairAbiStr, pairAddress);
                var reserveData = await pairContract.GetFunction("getReserves")
                .CallAsync< ReservesDto > ();

                string addressT0 = await pairContract.GetFunction("token0")
                .CallAsync<string>();
                string addressT1 = await pairContract.GetFunction("token1")
                .CallAsync<string>();

                string symbol = await pairContract.GetFunction("symbol")
                .CallAsync<string>();
                var symbolP = symbol.Split('-');
                PoolToken t0 = new PoolToken(symbolP[0], reserveData.Reserve0, addressT0);
                PoolToken t1 = new PoolToken(symbolP[1], reserveData.Reserve1, addressT1);


                allPoolDic.Add(pairAddress, new PoolPairs(t0, t1));
                Logger.Debug($"address {pairAddress} addressT0 {addressT0} {reserveData.Reserve0}  addressT1 {addressT1} {reserveData.Reserve1} symbol {symbol}");
                //string strs = JsonConvert.SerializeObject(allPoolDic, Formatting.Indented);

                //Logger.Debug(strs);
                //File.WriteAllText("./allPairs.json", strs);
            }

            string str = JsonConvert.SerializeObject(allPoolDic, Formatting.Indented);
            Logger.Debug(str);
            File.WriteAllText(config.pairsDataPath, str);

            return allPoolDic;

        }

        public Dictionary<string, PoolPairs> GetDatasByFile()
        {
            Dictionary<string, PoolPairs> allPoolDic = JsonConvert.DeserializeObject<Dictionary<string, PoolPairs>>(File.ReadAllText(config.pairsDataPath));
            return allPoolDic;
        }

        #endregion


    }



    public class Config
    {

        public string RedisConfig= "localhost,password=l3h2p1w0*";

        public string pairsDataPath = "./allPairs.json";

        public string symbol;

        public string address;

        public string abiPath;

        public string uniswapV3_factoryAbi;

        public string unswapV2_address = "0x5C69bEe701ef814a2B6a3EDD4B1652CB9cc5aA6f";

        public string uniswapV3_pairAbi;

        public Dictionary<string, string> allPaths = new Dictionary<string, string>() { {"BNB-USDT", "exchangeName:USDT&231-BNB&232,exchangeName:USDT&233-BNB&234" } };

        public List<PoolPairs> testPoolPairs = new List<PoolPairs>() ;
    }
    //https://mainnet.infura.io/v3/f7d3ed56ffc1466bbfa4d23738fc0a87
    //npx hardhat node --fork https://mainnet.infura.io/v3/f7d3ed56ffc1466bbfa4d23738fc0a87
    //npx hardhat node --fork https://eth-mainnet.alchemyapi.io/v2/uyl_NYbVcmhfPETCGr7CW0_JWCVkYh2v
    /*
     *  {
        "token0": {
          "id": "0xa0b86991c6218b36c1d19d4a2e9eb0ce3606eb48",
          "symbol": "USDC",
          "name": "USD//C",
          "derivedETH": "0.0005704799642292971753121751257002743"
        },
        "token1": {
          "id": "0xe336ac63cf871a66e7fbe74d1dc5c6774fbed281",
          "symbol": "DGT",
          "name": "DGT",
          "derivedETH": "0"
        },
        "reserve0": "0.000001",
        "reserve1": "0.000000140968211369",
        "reserveUSD": "0.0000009991948502158373759798111774433184",
        "trackedReserveETH": "0.00000000110176478891768812064172103849706",
        "token0Price": "7.093797887400220887738431272455595",
        "token1Price": "0.140968211369",
        "volumeUSD": "0",
        "txCount": "4"
      },
     */

}
