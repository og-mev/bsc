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
using WBNB;
using Nethereum.Web3.Accounts;
using static Nethereum.Util.UnitConversion;
using static arbitrage_CSharp.Tools.Util;
using Nethereum.Contracts.Standards.ERC20.TokenList;
using System.Linq;
using Nethereum.Uniswap.Contracts.UniswapV2Router02;
using Nethereum.Uniswap.Contracts.UniswapV2Router02.ContractDefinition;

namespace arbitrage_CSharp
{
    public partial class Strategy
    {
        private readonly EthereumClientIntegrationFixture _ethereumClientIntegrationFixture;

        
        Config config;
        private Web3 web3;

        private string contranctAddr;

        /// <summary>
        /// 存放所有token的兑换路径
        /// Dictionary<string（token id）, Dictionary<int（path长度）, List（相同长度多条路径）<List<string（依次兑换的 token id）>>>>
        /// </summary>
        Dictionary<string, Dictionary<int, List<List<string>>>> tokensSwapPathsDic = new Dictionary<string, Dictionary<int, List<List<string>>>>();
        /// <summary>
        /// 所有池子里面的数据 ，测试用，正式情况通过redis获取
        /// key 是
        /// </summary>
        Dictionary<string, PoolPairs> poolPairsDic;
        /// <summary>
        /// 币种对应小数点 字典
        /// </summary>
        Dictionary<string, (int Decimal, string Symbol)> tokenDecimlDic = new Dictionary<string, (int Decimal, string Symbol)>();

        /// <summary>
        /// 调用 信息的接口
        /// <命令号，数据>
        /// </summary>
        Action<string,object> SenMsg;
        public Strategy(string ConfigPath, Action<string, object> SenMsg)
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
            this.SenMsg = SenMsg;
            //string str = JsonConvert.SerializeObject(config,Formatting.Indented);
            //Logger.Debug(str);
            _ethereumClientIntegrationFixture = new EthereumClientIntegrationFixture();
        }
        /// <summary>
        /// 一直异步获取值
        /// </summary>
        public async void GetBalanceAsyncWaitTime(int milliseconds, List<Token> tokens)
        {
            while (true)
            {
                CurrTokenAmountDic = await GetBalanceAsync(web3, contranctAddr, tokens);
                Logger.Debug("++++++++++++++++++++++++++++++++完成本次获取本金！++++++++++++++++++++++++++++");
                await Task.Delay(milliseconds);
            }
        }

        public async Task StartAsync()
        {
            web3 = new Web3(config.NodeUrl);
            contranctAddr = JObject.Parse(File.ReadAllText(config.contractPath + "deploy.json"))["address"].ToString();

            GetBalanceAsyncWaitTime(config.SpanMillisecondsBalance, config.BalanceTokens);
            await TestGetBaseTokenAsync(contranctAddr);
            //await GetBalanceAsync();
            //await GetPoolDatasByContractAsync();
            //poolDatas

            //1 拉取 redis 获取 全路径，并且监听更新
            RedisDB.Init(config.RedisConfig);
            //RedisDB.Instance.StringGet<T>(DBKey);
            //poolPairsDic = await GetPoolDatasByContractAsync();//GetPoolDatasByFile();
            //poolPairsDic = GetPoolDatasByFile();
            poolPairsDic = GetPoolDatasByRedis();
            PoolDataHelper.Init(poolPairsDic);
            //test 兑换 wbnb
            



            //获取所有路径,和 每个token 的可以兑换tokens;
            var (tokensSwapPathsDic, adjacencyList) = GetAllPaths(poolPairsDic,4,false);
            this.tokensSwapPathsDic = tokensSwapPathsDic;
            Logger.Debug("===================================init suc=====================================");
            //2 监听 peending  tx
            var tx = new TX()
            {

            };
            //test 
            try
            {
                //await OnTxChangeAsync("");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            foreach (var item in config.testConfig.txHashs)
            {
                Logger.Debug($"============================当前tx:  {item} +++++++++++++++++");
                await this.AddTxAsync(item);
            }

        }




        /// <summary>
        /// 监听tx消息
        /// </summary>
        /// <param name="tokenChangeNumDic"> <reserves key  <token adrr, changeNum >></param>
        public async Task OnTxChangeAsync(Dictionary<string, Dictionary<string, BigInteger>> tokenChangeNumDic,List <string> sortPath)
        {
            //3 根据 tx 的交易对 获取所有对应路径
            //解析tx,获取到的tx是什么样子的,有可能同一个 区块中有多笔 tx改变？ 不考虑
            //test下需要计算出能兑换多少，实际上通过服务器传送
            

            //循环所有的 有改变的 token，寻找 其中有利润的路径
            foreach (var changeToken in sortPath)
            {
                string tokenAddress = changeToken;
                int decimalNum = tokenDecimlDic[tokenAddress].Decimal;
                if (CurrTokenAmountDic.TryGetValue(tokenAddress, out BigDecimal balance))//有token 才进行计算
                {
                    //根据tx 修改池子里面的数量
                    //tokenChangeNumDic 每次都要维护这个值，在一个新tx后改变

                    //4 根据盈利比例计算出所有可兑换的路径，以及最大兑换数量
                    //获取 两个token的路径
                    var tokenPaths = GetRandomPath(tokenAddress, 3);
                    var (bestPath, bestAmount, amountOut) = GetPathsWithAmount(tokenPaths, tokenChangeNumDic, true);

                    if (bestAmount <= 0)
                    {
                        Logger.Debug($"没有好的路径！！{changeToken}");
                        continue;
                    }
                    else
                    {
                        Logger.Debug($"兑换率最高 bestAmount {bestAmount} amountOut{amountOut}");
                    }
                    //如果超过最大现金，设置为最大现金
                    if (bestAmount > balance)
                    {
                        Logger.Debug($"超过最大现金 balance {balance} bestAmount{bestAmount}");
                        amountOut = (amountOut / bestAmount) * balance;
                        bestAmount = balance;
                    }

                    //由bigdecimal转换为bignumber
                    //取余，避免兑换小数差别
                    amountOut = amountOut * (1 - config.IgnoreRate);

                    var bestAmount_0 = bestAmount.Mantissa * BigDecimal.Pow(10, decimalNum + bestAmount.Exponent);
                    var bestAmount_int = (BigInteger)((decimal)(bestAmount_0));
                    var amountOut_0 = amountOut.Mantissa * BigDecimal.Pow(10, decimalNum + amountOut.Exponent);
                    var amountOut_int = (BigInteger)((decimal)(amountOut_0));

                    //TestAllTokens(token0, token1);

                    //5 签名后发给 ray
                    var flashswapAddr = contranctAddr;
                    var swapAbi = JObject.Parse(File.ReadAllText(config.contractPath + "artifacts/contracts/flashswap.sol/Flashswap.json"))["abi"].ToString();
                    var bridge = new SwapBridge("BSC", flashswapAddr.ToLower(), swapAbi: swapAbi);

                    //根据算法来交换 币
                    var swapArr = new List<(string symbol, BigInteger amountIn, BigInteger amountOutMin, string[] path)>();


                    var v0 = bestAmount_int - (bestAmount_int % 10000000);
                    var v1 = amountOut_int - (amountOut_int % 10000000);

                    (string symbol, BigInteger amountIn, BigInteger amountOutMin, string[] path) swap = ("pancakeswap", v0, v1, bestPath.ToArray());
                    swapArr.Add(swap);
                    await bridge.import_wallets(config.privateKeys.ToArray());
                    try
                    {
                        await bridge.swap(swapArr);
                    }
                    catch (Exception ex)
                    {

                        Logger.Error(ex);
                    }
                }
            }
            Logger.Debug("完成本次套利检查了！！！！！！！！！！！");
        }

        /// <summary>
        /// 获取 路径对应的盈利 数量 和路径 
        /// </summary>
        /// <param name="tokenPaths"></param>
        private (List<(string addr, string exchangeName)> backTokenPath, BigDecimal bestAmountT0ALL,BigDecimal bestAmountOut) GetPathsWithAmount(List<List<string>> tokenPaths,Dictionary<string, Dictionary<string, BigInteger>> tokenChangeNumDic, bool islog = true)
        {
            List<(string addr,string exchangeName )> backTokenPath = new List<(string addr, string exchangeName)>();
            BigDecimal bestAmountT0ALL = 0;
            BigDecimal bestProfit = 0;
            BigDecimal bestAmountOut = 0;

            List<(string pairAddr, string exchangeName)> bestPairPaths = new List<(string pairAddr, string exchangeName)>();

            var exchangeNames = (from ex in Constants.exchanges
                             select ex.Key).ToList();
            
            //循环计算 所有路径的 最大盈利
            foreach (var tokendPath in tokenPaths)
            {
                //把单条路径 通过不同交易所的相同交易对，扩展到n个路径  n 个path< path<pairAddr,exchangeName>>
                List<List<(string pairAddr, string exchangeName)>> pathsExt = new List<List<(string pairAddr, string exchangeName)>>();

                //把 所有路径合成 一个CFMM
                List<PoolPairs> cFMMPaths = new List<PoolPairs>();
                for (int tokenPathCount = 0; tokenPathCount < tokendPath.Count-1; tokenPathCount++)
                {

                    if (tokenPathCount==0)//第一次生成 添加 list
                    {
                        pathsExt.Add(new List<(string pairAddr, string exchangeName)>());
                    }

                    //获取本次区块改变后的值
                    var dexPairDic = PoolDataHelper.GetPoolPair(tokendPath[tokenPathCount],tokendPath[tokenPathCount+1], exchangeNames,tokenChangeNumDic);

                    //clone dexPairDic 数量 个 pathsExt
                    List<List<(string pairAddr, string exchangeName)>> cloneList = new List<List<(string pairAddr, string exchangeName)>>();
                    int dexCount = 0;
                    foreach (var data in dexPairDic)
                    {
                        List<List<(string pairAddr, string exchangeName)>> clonePathsExt = pathsExt;
                        if (dexCount >0 )
                        {
                            clonePathsExt = pathsExt.Clone();
                        }

                        dexCount++;
                        string exchangeName = data.Key;
                        var (_poolPair, pairAddr) = data.Value;
                        if (islog)
                        {
                            Logger.Debug($" exchangeName: {exchangeName} path[i]_path[+1]  {tokendPath[tokenPathCount]}_{tokendPath[tokenPathCount + 1]}");
                        }

                        //t0表示我们其实拥有的token，t1是要兑换的
                        if (_poolPair != null)
                        {
                            if (islog)
                                Logger.Debug(_poolPair.ToString());
                            cFMMPaths.Add(_poolPair);
                            foreach (var clonePath in clonePathsExt)
                            {
                                clonePath.Add((pairAddr, _poolPair.exchangeName));
                            }
                        }
                        cloneList.AddRange(clonePathsExt);
                    }
                    pathsExt = cloneList;
                }

               

                try
                {
                    CFMM endCFMM = CFMM.GetVisualCFMM(config.uniswapV2_fee, cFMMPaths.ToArray());
                    BigDecimal bestAmountT0 = CFMM.GetBestChangeAmount(endCFMM.R0, endCFMM.R1, config.uniswapV2_fee);
                    if (islog)
                        Logger.Debug($" bestAmountT0 {bestAmountT0} 路径最近兑换数量 {string.Join("-->", tokendPath.ToArray()) }");
                    //计算利润
                    if (bestAmountT0> 0)
                    {
                        //test 测试
                        //bestAmountT0 = 10;
                        BigDecimal amountOut = CFMM.GetDeltaB(endCFMM, config.uniswapV2_fee, (decimal)bestAmountT0);
                        BigDecimal profit = amountOut - bestAmountT0;
                        if (profit>bestProfit)
                        {
                            bestProfit = profit;
                            backTokenPath = tokendPath;
                            bestAmountT0ALL = bestAmountT0;
                            bestAmountOut = amountOut;
                            bestPairPaths = allDexPairPaths;
                            //if (islog)
                            
                                Logger.Debug($"有利润 bestProfit {bestProfit} bestAmountT0ALL {bestAmountT0ALL} amountOut{amountOut}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
            Logger.Debug("path pairAddr: " + string.Join(" ", bestPairPaths));
            Logger.Debug("backTokenPath : " + string.Join(" ", backTokenPath));
            Logger.Debug($" bestProfit {bestProfit} ");
            return (backTokenPath, bestAmountT0ALL, bestAmountOut);

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
            List<string> vertices = new List<string>();//顶点（tokens）
            List<Tuple<string, string>> edges = new List<Tuple<string, string>>();//边 , 可以构成的交易对
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
            Logger.Debug("获得全部的 token");
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
                if (i%200==0)
                {
                    Logger.Debug($"当前计算到了{i}");
                }
            }
            return (_tokensSwapPathsDic,graph.AdjacencyList);
        }


        #region 通信方法
        /// <summary>
        /// 添加tx
        /// </summary>
        /// <param name="txList"></param>
        public async Task AddTxAsync(string transaction)
        {
            //https://speedy-nodes-nyc.moralis.io/5c66f82a76ba601169cd112d/bsc/mainnet/archive
            var _web3 = new Web3("https://speedy-nodes-nyc.moralis.io/5c66f82a76ba601169cd112d/bsc/mainnet/archive");
            var transactionRpc = await _web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(transaction);
            //var web3_ = new Web3("https://mainnet.infura.io/v3/ddd5ed15e8d443e295b696c0d07c8b02");
            //var transactionRpc = await web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(transaction);

            var txPares = Util.DecodeTransaction(transactionRpc);
            if (txPares!=null)
            {
                Logger.Debug(txPares.ToString());
                var (tokenChangeNumDic, sortPath) = GetTokenChangeAmount(txPares, config.uniswapV2_fee, tokenDecimlDic);
                if (tokenChangeNumDic!=null)
                {
                    OnTxChangeAsync(tokenChangeNumDic, sortPath);
                }
            }
            else
            {
                Logger.Debug($"不是正确的swap方法 transactionRpc {transactionRpc}");
            }
            

        }
        /// <summary>
        /// 提交签名给服务器
        /// </summary>
        /// <param name="sign"></param>
        public void SendExchange(string sign ,int blockNum=0)
        {
            Logger.Debug($"发送tx {sign}");
            SenMsg(Commands.Send_Sign, sign);
        }

        #endregion

        #region 工具


        /// <summary>
        /// 计算出 本次交易 所有币种变化的数量
        /// </summary>
        /// <param name="pathData"></param>
        /// <param name="fee"></param>
        /// <param name="tokenDecimlDic"></param>
        /// <param name="exchangeName"></param>
        /// <returns>  <reserves key  <token adrr, changeNum >> </returns>
        public static (Dictionary<string, Dictionary<string, BigInteger>> tokenChangeNumDic, List<string> sortPath) GetTokenChangeAmount(PathData pathData,decimal fee,Dictionary<string, (int Decimal, string Symbol)> tokenDecimlDic, string exchangeName = "PancakeSwap")
        {
            //CFMM cFMM = new CFMM(poolP.poolToken0.tokenReverse, poolP.poolToken1.tokenReverse);
            //正向计算
            List<string> sortPath = new List<string>();
            BigInteger amoutIn = 0;
            if (pathData.amountKonw == AmountEnum.KnowIn)
            {
                sortPath = pathData.paths;
                amoutIn = pathData.amountIns;

            }//反向计算 ，把  path 翻转，依次计算值
            else if (pathData.amountKonw == AmountEnum.KnowOut)
            {
                pathData.paths.Reverse();
                sortPath = pathData.paths;
                amoutIn = pathData.amountOutMins;

            }
            //拉取 数据库对应数据参数
            //<air addr  <token addr , changeNum>>
            Dictionary<string, Dictionary<string, BigInteger>> tokenChangeNumDic = new Dictionary<string, Dictionary<string, BigInteger>>();
            List<(string name, BigDecimal changeNum)> tokenChangeNum = new List<(string name, BigDecimal changeNum)>();
            var paths = sortPath;
            for (int i = 0; i < paths.Count - 1; i++)
            {
                var addr0 = paths[i];
                var addr1 = paths[i + 1];
                
                bool isReverse = false;
                
                string keyTab = $"reserves";
                string keyName = $"{addr0}:{addr1}:{exchangeName}";
                string value = RedisDB.Instance.HashGet(keyTab, keyName);
                if (string.IsNullOrEmpty(value))
                {
                    keyTab = $"reserves";
                    keyName = $"{addr1}:{addr0}:{exchangeName}";
                    value = RedisDB.Instance.HashGet(keyTab, keyName);
                    isReverse = true;
                }
                if (string.IsNullOrEmpty(value))
                {
                    Logger.Error($" 找不到地址：reserves {addr0}:{addr1}:{exchangeName}");
                    return (null, null);
                }
                else
                {
                    (PoolPairs poolP, string pairAddr) = PraseRedis2Pair(keyName, value, tokenDecimlDic, null, isReverse);
                    BigDecimal amountOut =0;
                    //需要计算 是增加还是减少
                    //正向 是已知a增加 求b减少
                    CFMM cFMM = new CFMM(poolP.poolToken0.tokenReverse, poolP.poolToken1.tokenReverse);
                    Dictionary<string, BigInteger> keyValues ;
                    string pairKey0 = GetReservesKey( poolP.poolToken0.tokenAddress, poolP.poolToken1.tokenAddress , exchangeName);
                    string pairKey1 = GetReservesKey(poolP.poolToken0.tokenAddress, poolP.poolToken1.tokenAddress, exchangeName);

                    if (!tokenChangeNumDic.TryGetValue(pairKey0, out keyValues))
                    {
                        if (!tokenChangeNumDic.TryGetValue(pairKey1, out keyValues))
                        {
                            keyValues = new Dictionary<string, BigInteger>();
                            tokenChangeNumDic.Add(pairKey0, keyValues);
                        }
                    }
                    
                    if (i==0)
                    {//判断是否已经有这个池子，如果有直接添加值
                     //判断池子是否有这个 token 如果有直接改变 值
                        if (keyValues.ContainsKey(poolP.poolToken0.tokenAddress))
                        {
                            keyValues[poolP.poolToken0.tokenAddress] += amoutIn;
                        }
                        else
                        {
                            keyValues.Add(poolP.poolToken0.tokenAddress, amoutIn);
                        }
                        amountOut = CFMM.GetDeltaB(cFMM, fee,new BigDecimal(amoutIn,poolP.poolToken0.decimalNum));
                    }
                    else
                    {//反向的话 是已知b 减少 a求增加
                        BigDecimal nextOut = tokenChangeNum[i - 1].changeNum;
                        amountOut = CFMM.GetDeltaB(cFMM, fee, nextOut);
                        //判断池子是否有这个 token 如果有直接改变 值
                        BigInteger nextOut_int = nextOut.ParseBigDecimal(poolP.poolToken0.decimalNum);
                        if (keyValues.ContainsKey(poolP.poolToken0.tokenAddress))
                        {
                            keyValues[poolP.poolToken0.tokenAddress] += nextOut_int;
                        }
                        else
                        {
                            keyValues.Add(poolP.poolToken0.tokenAddress, nextOut_int);
                        }
                    }
                    //token1 减少 数量
                    tokenChangeNum.Add((poolP.poolToken1.tokenAddress, amountOut));
                    if (keyValues.ContainsKey(poolP.poolToken1.tokenAddress))
                    {

                        keyValues[poolP.poolToken1.tokenAddress] -= amountOut.ParseBigDecimal(poolP.poolToken1.decimalNum);
                    }
                    else
                    {
                        keyValues.Add(poolP.poolToken1.tokenAddress, -amountOut.ParseBigDecimal(poolP.poolToken1.decimalNum));
                    }
                }
            }
            return (tokenChangeNumDic,sortPath);
        }
        /// <summary>
        /// 获取 某个池子里面 token变换后数量
        /// </summary>
        /// <param name="tokenChangeNumDic"> Dictionary<string, Dictionary<string, BigInteger>> </param>
        /// <param name="dbKey"></param>
        public Dictionary<string, PoolPairs> GetTokenAfterTxChange(Dictionary<string, Dictionary<string, BigInteger>> tokenChangeNumDic , string exchangeName= "PancakeSwap")
        {
            Dictionary<string, PoolPairs> changeDic = new Dictionary<string, PoolPairs>();
            foreach ( var changePair in tokenChangeNumDic)
            {
                string addr0 = "";
                string addr1 = "";
                int idx = 0;
                foreach (var item in changePair.Value)
                {
                    if (idx==0)
                    {
                        addr0 = item.Key;
                    }
                    else
                    {
                        addr1 = item.Key;
                    }
                    idx++;
                }

                string dbTab = "tokens";
                string pairKey0 = GetReservesKey(addr0,addr1, exchangeName);
                string pairKey1 = GetReservesKey(addr1,addr0, exchangeName);
                bool isRe = false;//池子的 token顺序
                string realKey="";
                var value = RedisDB.Instance.HashGet(dbTab, pairKey0);
                realKey = pairKey0;
                if (string.IsNullOrEmpty(value))
                {
                    isRe = true;
                    value = RedisDB.Instance.HashGet(dbTab, pairKey1);
                    realKey = pairKey1;
                }
                if (string.IsNullOrEmpty(value))
                {
                    throw new Exception($"找不到池子 pairKey0 {pairKey0}  pairKey1 {pairKey1} ");
                }

                (PoolPairs poolP, string pairAddr) = PraseRedis2Pair(realKey, value, tokenDecimlDic, null);

                PoolToken t0, t1;
                Dictionary<string, BigInteger> keyValues;

                if (!tokenChangeNumDic.TryGetValue(pairKey0, out keyValues))
                {
                    if (!tokenChangeNumDic.TryGetValue(pairKey1, out keyValues))
                    {
                        throw new Exception($"没找到对应池子 pairKey1 {pairKey1} pairKey1 {pairKey1} ");
                    }
                }
                t0 = poolP.poolToken0.Clone();
                t1 = poolP.poolToken1.Clone();

                t0.tokenReverse += keyValues[t0.tokenAddress];
                t1.tokenReverse += keyValues[t1.tokenAddress];
                changeDic.Add(realKey, new PoolPairs(t0, t1, exchangeName));
            }
            return changeDic;

        }
        /// <summary>
        /// 获取 db key
        /// </summary>
        /// <param name="addr0"></param>
        /// <param name="addr1"></param>
        /// <param name="exchangeName"></param>
        /// <returns></returns>
        public static string GetReservesKey(string addr0,string addr1,string exchangeName)
        {
            string pairKey = addr0 + ":" + addr1 + ":" + exchangeName;
            return pairKey;
        }

        /// <summary>
        /// 返回 没有小数位数和Symbol
        /// </summary>
        /// <param name="reservesKey"></param>
        /// <param name="reservesValue"></param>
        /// <returns></returns>
        private static (PoolPairs poolP, string pairAddr) PraseRedis2Pair(string reservesKey, string reservesValue, Dictionary<string, (int Decimal, string Symbol)> tokenDecimlDic, StringBuilder sb,bool isReverse = false)
        {
            

            string[] tokenAddr = reservesKey.Split(':');//0x93d5a19a993D195cfC75AcdD736A994428290a59:0xbb4CdB9CBd36B01bD1cBaEBF2De08d9173bc095c:PancakeSwap
            string[] tokenDatas = reservesValue.ToString().Split('|');//r0|r1|pairAddr|update_timestamp   PancakeSwap: 3|3136980|0x696d16539Dd3eB00C19103f7d8cfA2cb32d66086|1654968667
            string addr0 = tokenAddr[0];
            string addr1 = tokenAddr[1];

            PoolToken t0 = new PoolToken("", BigDecimal.Parse(tokenDatas[0]), addr0, 0);
            PoolToken t1 = new PoolToken("", BigDecimal.Parse(tokenDatas[1]), addr1, 0);
            string pairAddr = tokenDatas[2];
            string exchangeName = tokenAddr[2];


            (int Decimal, string Symbol) tokenData0 = (0, null);
            (int Decimal, string Symbol) tokenData1 = (0, null);


            bool can = true;
            if (tokenDecimlDic.TryGetValue(addr0, out (int Decimal, string Symbol) data0Str))
            {
                tokenData0 = data0Str;

            }
            else
            {
                if (sb!=null)
                    sb.AppendLine(addr0);
                can = false;
            }
            if (tokenDecimlDic.TryGetValue(addr1, out (int Decimal, string Symbol) data1Str))
            {
                tokenData1 = data1Str;
            }
            else
            {
                if (sb != null)
                    sb.AppendLine(addr1);
                can = false;
            }
            PoolPairs poolP = null;
            if (can)
            {
                t0.tokenSymbol = tokenData0.Symbol;
                t0.decimalNum = tokenData0.Decimal;
                t1.tokenSymbol = tokenData1.Symbol;
                t1.decimalNum = tokenData1.Decimal;
                if (isReverse)
                {
                    poolP = new PoolPairs(t1, t0, exchangeName);

                }
                else
                {
                    poolP = new PoolPairs(t0, t1, exchangeName);
                }


            }

            return (poolP, pairAddr);
        }
        #endregion

        #region test
        private void TestAllTokens(PoolToken token0, PoolToken token1)
        {
            foreach (var item in tokensSwapPathsDic)
            {
                var tokenPaths = GetRandomPath(item.Key, 4);
                var (bestPath, bestAmountIn, amountOut) = GetPathsWithAmount(tokenPaths,  null, false);
                if (bestAmountIn > 0)
                {
                    Logger.Debug($"有利润！！ {item.Key}  {bestAmountIn}  {string.Join("->", bestPath)}");
                }
            }
        }
        /// <summary>
        /// 添加默认token
        /// </summary>
        /// <param name="url"></param>
        /// <param name="contractAddress"></param>
        /// <returns></returns>
        private async Task TestGetBaseTokenAsync(string sendToAddr, string url = "http://127.0.0.1:8545/",string contractAddress= "0xbb4CdB9CBd36B01bD1cBaEBF2De08d9173bc095c")
        {
            await Task.Delay(15000);
            //var flashswapAddr = JObject.Parse(File.ReadAllText(config.contractPath + "deploy.json"))["address"].ToString();
            //contractAddress = flashswapAddr;
            var account = new Account(config.privateKeys[0]);
            var address = account.Address;
            var web3 = new Web3(account, url);
            string wnnbAbi = File.ReadAllText(config.wbnbAbi);
            var contractHandler = web3.Eth.GetContractHandler(contractAddress);

            BigInteger v = await account.NonceService.GetNextNonceAsync();
            v += 1;

            BigInteger amount = UnitConversion.Convert.ToWei(1000);
            //质押
            DepositFunction deposit = new DepositFunction()
            {
                AmountToSend = amount,
                Gas = 30000000,
                GasPrice = 5000000000,
                FromAddress = account.Address,
                Nonce = v,
            };

            var depositFunctionTxnReceipt = await contractHandler.SendRequestAndWaitForReceiptAsync<DepositFunction>(deposit);
            //转账
            var transferFunction = new TransferFunction()
            {
                AmountToSend = 0,
                Gas = 30000000,
                GasPrice = 5000000000,
                FromAddress = account.Address,
                Nonce = v+1
             };
            transferFunction.Dst = address;
            transferFunction.Wad = amount;
            var transferFunctionTxnReceipt = await contractHandler.SendRequestAndWaitForReceiptAsync(transferFunction);

            Logger.Debug($"change {amount} to bnb");

            UniswapV2Router02Service ser = new UniswapV2Router02Service(web3, "0x10ed43c718714eb63d5aa57b78b54704e256024e");
            List<Token> tokens = new List<Token>()
            {
                new Token()
                {
                    ChainId = 1,
                    Address = "0x55d398326f99059ff775485246999027b3197955",
                    Symbol = "USDT",
                    Name = "Tether USD",
                    Decimals = 18,
                },
                new Token()
                {
                    ChainId = 1,
                    Address = "0xe9e7cea3dedca5984780bafc599bd69add087d56",
                    Symbol = "BUSD",
                    Name = "BUSD Token",
                    Decimals = 18,
                },
                 new Token()
                {
                    ChainId = 1,
                    Address = "0x8ac76a51cc950d9822d68b83fe1ad97b32cd580d",
                    Symbol = "USDC",
                    Name = "USDC",
                    Decimals = 18,
                },
                  new Token()
                {
                    ChainId = 1,
                    Address = "0x0e09fabb73bd3ade0a17ecc321fd13a19e81ce82",
                    Symbol = "Cake",
                    Name = "Cake",
                    Decimals = 18,
                }
            };

            int add = 1;
            int addAmount = 10;
            string fromAddr = "0xbb4cdb9cbd36b01bd1cbaebf2de08d9173bc095c";
            foreach (var token in tokens)
            {
                add++;
                SwapETHForExactTokensFunction function = new SwapETHForExactTokensFunction()
                {
                    AmountToSend = addAmount * BigInteger.Pow(10, token.Decimals),
                    Gas = 30000000,
                    GasPrice = 5000000000,
                    FromAddress = account.Address,
                    Nonce = v + add,

                    AmountOut = addAmount * BigInteger.Pow(10, token.Decimals),
                    Path = new List<string>() { fromAddr, token.Address },
                    To = sendToAddr,//account.Address,
                    Deadline = 165545446800

                };
                
                await ser.SwapETHForExactTokensRequestAsync(function);
                Logger.Debug($"swap {token.Symbol} suc");
            }
            //新增 Tether USD USDT 0x55d398326f99059fF775485246999027B3197955

            //新增 BUSD Token BUSD 0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56

            //新增 PancakeSwap Token Cake 0x0E09FaBB73Bd3Ade0a17ECC321fD13a19e81cE82

            //新增 USD Coin USDC 0x8AC76a51cc950d9822D68b83fE1Ad97B32Cd580d
        }
        #endregion
    }



    public class Config
    {
        
        public List<string> privateKeys = new List<string> { "0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80" };//真实测试  0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80
        //158.247.203.163
        //public string NodeUrl = "http://158.247.203.163:8545/";//"http://localhost:8545/";
        public string NodeUrl = "http://localhost:8545/";

        public string RedisConfig= "localhost,password=l3h2p1w0*";

        public string contractPath = "D://_Work//_mev//bsc//contract//";

        public string pairsDataPath = "./allPairs.json";

        public string tokenAbi = "./tokenAbi.json";

        public string uniswapV3_factoryAbi;

        public string unswapV2_FactoryAddress = "0x5C69bEe701ef814a2B6a3EDD4B1652CB9cc5aA6f";

        public string uniswapV3_pairAbi;

        public string wbnbAbi = "./wbnbAbi.json";

        public decimal uniswapV2_fee = 0.0m;
        /// <summary>
        ///忽略比例 ，忽略兑换比例,取整
        /// </summary>
        public decimal IgnoreRate = 0.00001m;
        /// <summary>
        /// 获取本金间隔时间
        /// </summary>
        public int SpanMillisecondsBalance = 30000;

        /// <summary>
        /// 当前各种币的数量的字典
        /// </summary>
        public Dictionary<string, decimal> CurrTokenAmountDic = new Dictionary<string, decimal>() { { "0x55d398326f99059ff775485246999027b3197955", 100 }, { "0xe9e7cea3dedca5984780bafc599bd69add087d56", 100 }, { "0x8ac76a51cc950d9822d68b83fe1ad97b32cd580d", 100 }, { "0x0e09fabb73bd3ade0a17ecc321fd13a19e81ce82", 100 } };

        public List<Token> BalanceTokens = new List<Token>()
            {
            new Token()
        {
            ChainId = 1,
                    Address = "0xbb4CdB9CBd36B01bD1cBaEBF2De08d9173bc095c",
                    Symbol = "WBNB",
                    Name = "WBNB",
                    Decimals = 18,
                },
                new Token()
        {
            ChainId = 1,
                    Address = "0x55d398326f99059ff775485246999027b3197955",
                    Symbol = "USDT",
                    Name = "Tether USD",
                    Decimals = 18,
                },
                new Token()
        {
            ChainId = 1,
                    Address = "0xe9e7cea3dedca5984780bafc599bd69add087d56",
                    Symbol = "BUSD",
                    Name = "BUSD Token",
                    Decimals = 18,
                },
                 new Token()
        {
            ChainId = 1,
                    Address = "0x8ac76a51cc950d9822d68b83fe1ad97b32cd580d",
                    Symbol = "USDC",
                    Name = "USDC",
                    Decimals = 18,
                },
                  new Token()
        {
            ChainId = 1,
                    Address = "0x0e09fabb73bd3ade0a17ecc321fd13a19e81ce82",
                    Symbol = "Cake",
                    Name = "Cake",
                    Decimals = 18,
                }
    };
        
        public Dictionary<string, string> allPaths = new Dictionary<string, string>() { {"BNB-USDT", "exchangeName:USDT&231-BNB&232,exchangeName:USDT&233-BNB&234" } };

        public List<PoolPairs> testPoolPairs = new List<PoolPairs>() ;

        public TestConfig testConfig = new TestConfig();

    }

    public  class TestConfig
    {
        public string poolId = "0x74e4716e431f45807dcf19f284c7aa99f18a4fbc";//"0xae461ca67b15dc8dc81ce7615e0320da1a9ab8d5";

        public string adressFrom = "0x2170ed0880ac9a755fd29b2688956bd959f933f8";//"0x6b175474e89094c44da98b954eedeac495271d0f";

        public string adressTo = "0xbb4cdb9cbd36b01bd1cbaebf2de08d9173bc095c";//"0xa0b86991c6218b36c1d19d4a2e9eb0ce3606eb48";
        /// <summary>
        /// 测试，只下拉最多条数的 交易池，方便计算
        /// </summary>
        public int maxPairCount = 1000;

        public List<string> txHashs;
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
