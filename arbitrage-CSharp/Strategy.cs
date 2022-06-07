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
using Nethereum.RPC.Eth.DTOs;
using arbitrage_CSharp.Mode;
using System.Numerics;
using Nethereum.Util;

namespace arbitrage_CSharp
{
    class Strategy
    {
        private readonly EthereumClientIntegrationFixture _ethereumClientIntegrationFixture;


        Config config;

        private delegate void TxChange(TransactionReceipt transaction);

        /// <summary>
        /// 存放所有token的兑换路径
        /// Dictionary<string（token id）, Dictionary<int（path长度）, List（相同长度多条路径）<List<string（依次兑换的 token id）>>>>
        /// </summary>
        Dictionary<string, Dictionary<int, List<List<string>>>> tokensSwapPathsDic = new Dictionary<string, Dictionary<int, List<List<string>>>>();
        /// <summary>
        /// token 的可兑换地址
        /// </summary>
        Dictionary<string, HashSet<string>> adjacencyList;
        /// <summary>
        /// 所有池子里面的数据 ，测试用，正式情况通过redis获取
        /// key 是
        /// </summary>
        Dictionary<string, PoolPairs> poolPairsDic;

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
            //string str = JsonConvert.SerializeObject(config,Formatting.Indented);
            //Logger.Debug(str);
            _ethereumClientIntegrationFixture = new EthereumClientIntegrationFixture();
        }

        public async void StartAsync()
        {

            //await GetPoolDatasByContractAsync();
            //poolDatas

            //1 拉取 redis 获取 全路径，并且监听更新
            //RedisDB.Instance.StringGet<T>(DBKey);
            poolPairsDic = await GetPoolDatasByContractAsync();//GetPoolDatasByFile();
            //poolPairsDic = GetPoolDatasByFile();
            PoolDataHelper.Init(poolPairsDic);
            //获取所有路径,和 每个token 的可以兑换tokens;
            var (tokensSwapPathsDic, adjacencyList) = GetAllPaths(poolPairsDic);
            this.tokensSwapPathsDic = tokensSwapPathsDic;
            //RedisDB.Init(config.RedisConfig);
            //2 监听 peending  tx
            TxChange txChange = new TxChange(OnTxChangeAsync);
            var tx = new TransactionReceipt()
            {

            };
            //test 
            try
            {
                txChange(tx);
            }
            catch (Exception ex)
            {

                Logger.Error(ex);
            }
            


        }

       

        /// <summary>
        /// 监听tx消息
        /// </summary>
        /// <param name="tx"></param>
        private async void OnTxChangeAsync(TransactionReceipt tx)
        {
            //3 根据 tx 的交易对 获取所有对应路径
            //解析tx,获取到的tx是什么样子的,有可能同一个 区块中有多笔 tx改变？
            string poolId = config.testConfig.poolId;
            string adressFrom = config.testConfig.adressFrom;//DAI
            decimal amountFrom = 1000;
            string addressTo = config.testConfig.adressTo;//USDC
            //test下需要计算出能兑换多少，实际上通过服务器传送
            BigDecimal changeAmountTo = 0;
            var poolPair = poolPairsDic[poolId];
            var token0 = poolPair.GetToken(adressFrom, poolId);
            var token1 = poolPair.GetToken(addressTo, poolId);

            CFMM cFMM = new CFMM(token0.tokenReverse,token1.tokenReverse);
            //test ______________________实际上会收到 值
            changeAmountTo = CFMM.GetDeltaB(cFMM, config.uniswapV2_fee, amountFrom);
            //test______________________________
            //根据tx 修改池子里面的数量
            token0.tokenReverse += amountFrom;
            token1.tokenReverse -= changeAmountTo;
            //获取 两个token的路径
            
            var tokenPaths = GetRandomPath(token0.tokenAddress, 3);
            var (bestPath,bestAmount) = GetPathsWithAmount(tokenPaths, token0,  token1);


            //4 根据盈利比例计算出所有可兑换的路径，以及最大兑换数量

            //5 签名后发给 ray
            var flashswapAddr = JObject.Parse(File.ReadAllText("../contract/deploy.json"))["address"].ToString();
            var bridge = new SwapBridge("BSC", flashswapAddr);

            //根据算法来交换 币
            var swapArr = new List<(string symbol, decimal amountIn, decimal amountOutMin, string[] path)>();
            (string symbol, decimal amountIn, decimal amountOutMin, string[] path) swap = ("pancakeswap", 2.0m, 1.0m, new string[] { "0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56", "0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56", "0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56" });

            swapArr.Add(swap);
            await bridge.import_wallets("0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80");
            bridge.swap(swapArr);
        }

        /// <summary>
        /// 获取 路径对应的盈利 数量 和路径 
        /// </summary>
        /// <param name="tokenPaths"></param>
        /// <param name="token0"> 表示我们拥有要兑换的</param>
        /// <param name="token1"></param>
        private (List<string> backPath, BigDecimal bestAmountT0ALL) GetPathsWithAmount(List<List<string>> tokenPaths, PoolToken token0, PoolToken token1)
        {
            List<string> backPath = new List<string>();
            BigDecimal bestAmountT0ALL = 0;
            BigDecimal bestProfit = 0;
            //循环计算 所有路径的 最大盈利
            foreach (var path in tokenPaths)
            {
                //把 所有路径合成 一个CFMM
                List<PoolPairs> cFMMPaths = new List<PoolPairs>();
               
                for (int i = 0; i < path.Count-1; i++)
                {
                    var _poolPair = PoolDataHelper.GetPoolPair(path[i],path[i+1]);
                    Logger.Debug($"path[i]_path[+1]  {path[i]}_{path[i + 1]}");
                    //t0表示我们其实拥有的token，t1是要兑换的
                    if (_poolPair!=null)
                    {
                        Logger.Debug(_poolPair.ToString());
                        cFMMPaths.Add(_poolPair);
                    }
                }
                try
                {
                    CFMM endCFMM = CFMM.GetVisualCFMM(config.uniswapV2_fee, cFMMPaths.ToArray());
                    BigDecimal bestAmountT0 = CFMM.GetBestChangeAmount(endCFMM.R0, endCFMM.R1, config.uniswapV2_fee);
                    Logger.Debug($" bestAmountT0 {bestAmountT0} 路径最近兑换数量 {string.Join("-->", path.ToArray()) }");
                    //计算利润
                    if (bestAmountT0> 0)
                    {
                        //test 测试
                        //bestAmountT0 = 10;
                        BigDecimal profit = CFMM.GetDeltaB(endCFMM, config.uniswapV2_fee,(decimal) bestAmountT0) - bestAmountT0;
                        if (profit>bestProfit)
                        {
                            bestProfit = profit;
                            backPath = path;
                            bestAmountT0ALL = bestAmountT0;
                            Logger.Debug($" bestProfit {bestProfit} ");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
            return (backPath, bestAmountT0ALL);

        }
        /// <summary>
        /// 根据 token 返回 一定数量的随机路径
        /// </summary>
        /// <param name="tokenAddress"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        private List<List<string>> GetRandomPath(string tokenAddress, int num)
        {
            List<List<string>> paths = new List<List<string>>();
            foreach (var item in tokensSwapPathsDic[tokenAddress])
            {
                paths.AddRange(item.Value);
            }
            return paths;
        }


        /// <summary>
        /// 返回根据 路径长度添装的 路径list字典
        /// </summary>
        /// <param name="poolPairsDic"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        private static (Dictionary<string, Dictionary<int, List<List<string>>>>,Dictionary<string,HashSet<string>>) GetAllPaths(Dictionary<string, PoolPairs> poolPairsDic,int maxHosp=5,bool log = true)
        {
            //存放所有token 对应的兑换路径
            Dictionary<string, Dictionary<int, List<List<string>>>> _tokensSwapPathsDic = new Dictionary<string, Dictionary<int, List<List<string>>>>();
            List<string> allTokens = new List<string>();
            List<string> vertices = new List<string>();
            List<Tuple<string, string>> edges = new List<Tuple<string, string>>();
            //获取有多少种类
            foreach (var item in poolPairsDic)
            {
                if (!allTokens.Contains(item.Value.poolToken0.tokenAddress))
                {
                    allTokens.Add(item.Value.poolToken0.tokenAddress);
                }
                if (!allTokens.Contains(item.Value.poolToken1.tokenAddress))
                {
                    allTokens.Add(item.Value.poolToken1.tokenAddress);
                }
                
            }
            foreach (var item in allTokens)
            {
                vertices.Add(item);
            }
            // 构成图，把相同的 地址能连接的放到一起
            foreach (var poolPair in poolPairsDic)
            {
                edges.Add(new Tuple<string, string>(poolPair.Value.poolToken0.tokenAddress, poolPair.Value.poolToken1.tokenAddress));
            }

            var graph = new Graph<string>(vertices, edges);
            //循环获得所有tokens的兑换路径
            for (int i = 0; i < allTokens.Count; i++)
            {
                var token = allTokens[i];
                var allPaths = Algorithms.DFSAllPaths<string>(graph, vertices[i], maxHosp);
                _tokensSwapPathsDic.Add(token, allPaths);
                if (log)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var edge in edges)
                    {
                        sb.AppendLine($"{edge.Item1} {edge.Item2} 100");
                    }

                    for (int j = 0; j < vertices.Count; j++)
                    {
                        sb.AppendLine($"{vertices[j]}");
                    }
                    Logger.Debug(sb.ToString());
                }
            }
            return (_tokensSwapPathsDic,graph.AdjacencyList);
        }


        #region 工具
        /// <summary>
        /// 获取所有的 交易对 通过合约
        /// </summary>
        /// <returns></returns>
        public async Task<Dictionary<string, PoolPairs>> GetPoolDatasByContractAsync()
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
            string tokenAbiStr = File.ReadAllText(config.tokenAbi);


            string pairAbiStr = File.ReadAllText(config.uniswapV3_pairAbi);
            var factoryContract = web3.Eth.GetContract(factoryAbiStr, config.unswapV2_FactoryAddress);
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
                string pairAddress = (await allPairs.CallAsync<string>(i)).ToLower();
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

//                 string symbol = await pairContract.GetFunction("symbol")
//                 .CallAsync<string>();
//                 var symbolP = symbol.Split('-');
                Logger.Debug($"addressT0 {addressT0} addressT1 {addressT1} ");

                //var ss = reserveData.Reserve0 / reserveData.Reserve1;

                try
                {
                    var token0Contract = web3.Eth.GetContract(tokenAbiStr, addressT0);
                    int dec0 = await token0Contract.GetFunction("decimals")
                        .CallAsync<int>();
                    var token1Contract = web3.Eth.GetContract(tokenAbiStr, addressT1);
                    int dec1 = await token1Contract.GetFunction("decimals")
                        .CallAsync<int>();
                    string symbol0 = await token0Contract.GetFunction("symbol")
                                    .CallAsync<string>();
                    string symbol1 = await token1Contract.GetFunction("symbol")
                .   CallAsync<string>();
                    BigDecimal r0 = new BigDecimal(reserveData.Reserve0, -dec0);
                    BigDecimal r1 = new BigDecimal(reserveData.Reserve1, -dec1);

                    PoolToken t0 = new PoolToken(symbol0, r0, addressT0);
                    PoolToken t1 = new PoolToken(symbol1, r1, addressT1);


                    allPoolDic.Add(pairAddress, new PoolPairs(t0, t1));
                    Logger.Debug($"address {pairAddress} addressT0 {addressT0} {reserveData.Reserve0}  addressT1 {addressT1} {reserveData.Reserve1} symbol {symbol0} {symbol1}");
                }
                catch (Exception)
                {

                    Logger.Error($"这个不是erc20 ！！！！！！！！！！！！pairAddress {pairAddress}");
                }
                
                //string strs = JsonConvert.SerializeObject(allPoolDic, Formatting.Indented);

                //Logger.Debug(strs);
                //File.WriteAllText("./allPairs.json", strs);
            }

            string str = JsonConvert.SerializeObject(allPoolDic, Formatting.Indented);
            Logger.Debug(str);
            File.WriteAllText(config.pairsDataPath, str);

            return allPoolDic;

        }
        /// <summary>
        /// 通过存档获取所有交易对
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, PoolPairs> GetPoolDatasByFile()
        {//需要解析
            string str = File.ReadAllText(config.pairsDataPath);

            JObject Info = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(str);
            Dictionary<string, PoolPairs> allPoolDic = JsonConvert.DeserializeObject<Dictionary<string, PoolPairs>>(str);
            foreach (var pool in allPoolDic)
            {
                var item = Info[pool.Key];
                var v = item["poolToken0"]["tokenReverse"];
                BigInteger s = BigInteger.Parse( v["Mantissa"].ToString());
                int de = int.Parse(v["Exponent"].ToString());
                pool.Value.poolToken0.tokenReverse = new BigDecimal(s, de);

                var v1 = item["poolToken1"]["tokenReverse"];
                BigInteger s1 = BigInteger.Parse(v1["Mantissa"].ToString());
                int de1 = int.Parse(v["Exponent"].ToString());
                pool.Value.poolToken1.tokenReverse = new BigDecimal(s1, de1);
            }
            return allPoolDic;
        }
        #endregion
    }



    public class Config
    {

        public string RedisConfig= "localhost,password=l3h2p1w0*";

        public string pairsDataPath = "./allPairs.json";

        public string tokenAbi = "./tokenAbi.json";

        public string uniswapV3_factoryAbi;

        public string unswapV2_FactoryAddress = "0x5C69bEe701ef814a2B6a3EDD4B1652CB9cc5aA6f";

        public string uniswapV3_pairAbi;

        public decimal uniswapV2_fee = 0.0m;
        /// <summary>
        /// 当前各种币的数量的字典
        /// </summary>
        public Dictionary<string, decimal> CurrTokenAmountDic = new Dictionary<string, decimal>() { { "0xc02aaa39b223fe8d0a0e5c4f27ead9083c756cc2", 1 } };

        public Dictionary<string, string> allPaths = new Dictionary<string, string>() { {"BNB-USDT", "exchangeName:USDT&231-BNB&232,exchangeName:USDT&233-BNB&234" } };

        public List<PoolPairs> testPoolPairs = new List<PoolPairs>() ;

        public TestConfig testConfig = new TestConfig();
    }

    public class TestConfig
    {
        public string poolId = "0xae461ca67b15dc8dc81ce7615e0320da1a9ab8d5";

        public string adressFrom = "0x6b175474e89094c44da98b954eedeac495271d0f";

        public string adressTo= "0xa0b86991c6218b36c1d19d4a2e9eb0ce3606eb48";
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
