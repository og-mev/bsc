using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;
using Nethereum.Contracts;
using Nethereum.HdWallet;
using Nethereum.JsonRpc.Client;
using Nethereum.JsonRpc.IpcClient;
using Nethereum.JsonRpc.WebSocketClient;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tools;

using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts.CQS;
using Nethereum.Util;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Contracts.Extensions;
using Nethereum.Contracts.Standards.ENS.Registrar.ContractDefinition;
using Nethereum.RPC.TransactionManagers;
using arbitrage_CSharp.Tools;
using Nethereum.RPC.NonceServices;
using Nethereum.RPC.Eth.DTOs;
using swapAbi;

namespace arbitrage_CSharp
{
    public static class Constants
    {
        public static readonly Dictionary<string, int> exchanges = new Dictionary<string, int> { { "PancakeSwap", 1 }, { "BiSwap", 2 } };

    }

    public class ERC20
    {
        public string symbol { get; set; }
        public string fullname { get; set; }
        public string address { get; set; }
        public decimal decimals { get; set; }
        public ERC20(string symbol, string fullname, string address, decimal decimals)
        {
            this.symbol = symbol;
            this.fullname = fullname;
            this.address = address;
            this.decimals = decimals;
        }

    }

    public class SwapPair
    {


        public string symbol { get; set; }
        public string exchange { get; set; }
        public string token0 { get; set; }
        public string token1 { get; set; }
        public string reverse0 { get; set; }
        public string reverse1 { get; set; }
        public string address { get; set; }
        public decimal decimals { get; set; }
        public SwapPair(string symbol, string exchange, string token0, string token1, string reverse0, string reverse1, string address, decimal decimals)
        {
            this.symbol = symbol;
            this.exchange = exchange;
            this.token0 = token0;
            this.token1 = token1;
            this.reverse0 = reverse0;
            this.reverse1 = reverse1;
            this.address = address;
            this.decimals = decimals;
        }

        public async Task up_reverse(string reverse0, string reverse1)
        {
            this.reverse0 = reverse0;
            this.reverse1 = reverse1;
        }
    }

    public class SwapBridge
    {


        public string symbol { get; set; }
        public string contractAdrees { get; set; }


        public ClientBase provider { get; set; }

        public Contract contract { get; set; }

        public Web3 web3{ get; set; }
        public string bridgeAbi
        {
            get { return _bridgeAbi; }
            set {
                if (value!=null)
                {
                    _bridgeAbi = value;
                }
            } 
        }

        private string _bridgeAbi;
        

        public List<(Account,BigInteger)> wallets { get; set; }

        public SwapBridge(string symbol ,string contractAdrees = "0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56",string rpcUrl = "http://127.0.0.1:8545/", string swapAbi = null)
        {
            //this._bridgeAbi = JObject.Parse(File.ReadAllText("../../contract/artifacts/contracts/flashswap.sol/Flashswap.json"))["abi"].ToString();

            this.symbol = symbol;
            this.contractAdrees = contractAdrees ;
            this.bridgeAbi = swapAbi;
            this.provider = init_provider(rpcUrl);
            
            this.contract = attach_swap_contract(rpcUrl,contractAdrees);//使用自己的高速通道创建
           


            this.wallets = new List<(Account, BigInteger)>();
        }


        private ClientBase init_provider(string url)
        {
            ClientBase provider = null;
            if (!string.IsNullOrEmpty(url))
            {
                if (url.StartsWith("ipc"))
                {
                    provider = new IpcClient(url);
                }
                else if (url.StartsWith("http"))
                {
                    provider = new RpcClient(new Uri(url));
                }
                else if (url.StartsWith("ws"))
                {
                    provider = new WebSocketClient(url);
                }
                else
                {
                    //provider = new RpcClient(new Uri("https://data-seed-prebsc-1-s1.binance.org:8545/"));
                    provider = new RpcClient(new Uri("http://127.0.0.1:8545/"));
                }
            }
            return provider;
        }

        private Contract attach_swap_contract(string url ,string contractAdrees)
        {
            var web3 = new Web3( url);
            //var abi = @"[{""constant"":false,""inputs"":[{""name"":""_spender"",""type"":""address""},{""name"":""_value"",""type"":""uint256""}],""name"":""approve"",""outputs"":[{""name"":""success"",""type"":""bool""}],""type"":""function""},{""constant"":true,""inputs"":[],""name"":""totalSupply"",""outputs"":[{""name"":""supply"",""type"":""uint256""}],""type"":""function""},{""constant"":false,""inputs"":[{""name"":""_from"",""type"":""address""},{""name"":""_to"",""type"":""address""},{""name"":""_value"",""type"":""uint256""}],""name"":""transferFrom"",""outputs"":[{""name"":""success"",""type"":""bool""}],""type"":""function""},{""constant"":true,""inputs"":[{""name"":""_owner"",""type"":""address""}],""name"":""balanceOf"",""outputs"":[{""name"":""balance"",""type"":""uint256""}],""type"":""function""},{""constant"":false,""inputs"":[{""name"":""_to"",""type"":""address""},{""name"":""_value"",""type"":""uint256""}],""name"":""transfer"",""outputs"":[{""name"":""success"",""type"":""bool""}],""type"":""function""},{""constant"":true,""inputs"":[{""name"":""_owner"",""type"":""address""},{""name"":""_spender"",""type"":""address""}],""name"":""allowance"",""outputs"":[{""name"":""remaining"",""type"":""uint256""}],""type"":""function""},{""inputs"":[{""name"":""_initialAmount"",""type"":""uint256""}],""type"":""constructor""},{""anonymous"":false,""inputs"":[{""indexed"":true,""name"":""_from"",""type"":""address""},{""indexed"":true,""name"":""_to"",""type"":""address""},{""indexed"":false,""name"":""_value"",""type"":""uint256""}],""name"":""Transfer"",""type"":""event""},{""anonymous"":false,""inputs"":[{""indexed"":true,""name"":""_owner"",""type"":""address""},{""indexed"":true,""name"":""_spender"",""type"":""address""},{""indexed"":false,""name"":""_value"",""type"":""uint256""}],""name"":""Approval"",""type"":""event""}]";
            var contract = web3.Eth.GetContract(bridgeAbi, contractAdrees);
            Logger.Debug($"contract {contract.ToString()}");
            return contract;
        }

        public async Task import_wallets(params string[] keys)
        {
            foreach (var key in keys)
            {
                await import_wallet(key);
            }
        }

        private async Task import_wallet(string privateKey)
        {
            var account = new Account(privateKey);
            var address = account.Address;
            web3 = new Web3(account,provider);
            var balance = await web3.Eth.GetBalance.SendRequestAsync(account.Address);
            Logger.Debug($"balacne {balance}");
            var amountInEther = Web3.Convert.FromWei(balance.Value);

            account.NonceService = new InMemoryNonceService(account.Address, web3.Client);
            var nonce  = await web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(account.Address, BlockParameter.CreatePending());
            wallets.Add((account,nonce));
            Logger.Debug($"nonce {nonce} address{address}");
            return ;
        }

        private (Account account,BigInteger noce) get_random_wallet()
        {
            int idx = new Random().Next(0, wallets.Count);
            return wallets[idx];
        }

        public async Task swap(List<(string symbol, BigInteger amountIn, BigInteger amountOutMin, string[] path)> arrs)//(string symbols, decimal amountIns, decimal amountOutMins,List<string> paths)
        {
            List<byte> symbols = new List<byte>();
            List<BigInteger> amountIns = new List<BigInteger>();
            List<BigInteger> amountOutMins = new List<BigInteger>();
            List<byte> pathSlices = new List<byte>();
            List<string> paths = new List<string>();


            foreach (var item in arrs)
            {
                //symbols.Add(item.symbol);
                symbols.Add(1);
                //symbols.Add(Constants.exchanges[symbol]);
                amountIns.Add(  item.amountIn);
                amountOutMins.Add( item.amountOutMin);
                paths.AddRange(item.path);
                pathSlices.Add((byte)item.path.Length);
                Logger.Debug($"item.symbol {item.symbol} item.amountIn {item.amountIn} item.amountOutMin {item.amountOutMin} ");
                string pathStr = "";
                foreach (var path in paths)
                {
                    pathStr += "->"+ path;
                    
                }
                Logger.Debug($"pathStr {pathStr}");
            }
            
            var wallet = get_random_wallet();
            //wallet.noce += 1;

            //web3.Eth.TransactionManager.UseLegacyAsDefault = true;
            var transferHandler = web3.Eth.GetContractTransactionHandler<MultiSwapFunctionBase>();
            var transfer = new MultiSwapFunctionBase()
            {
                AmountToSend = 0,
                //Nonce = wallet.noce-1,
                Gas= 1500000,
                GasPrice = 695522217,
                //FromAddress = wallet.account.Address,
                
                symbols = symbols.ToArray(),
                amountIns = amountIns.ToArray(),
                amountOutMins = amountOutMins.ToArray(),
                pathSlices = pathSlices.ToArray(),
                paths = paths.ToArray(),
            };
            var contractHandler = web3.Eth.GetContractHandler(contract.Address);

            
            var multiSwapFunction = new MultiSwapFunction()
            {
                AmountToSend = 0,
                 //Nonce = 93,
                 Gas = 1500000,
                 GasPrice = 695522217,
                 //FromAddress = wallet.account.Address,
             };
            multiSwapFunction.Symbols = symbols;
            multiSwapFunction.AmountIns = amountIns;
            multiSwapFunction.AmountOutMins = amountOutMins;
            multiSwapFunction.PathSlices = pathSlices;
            multiSwapFunction.Paths = paths;
//             multiSwapFunction.Symbols = new List<byte>() { 1 };
//             multiSwapFunction.AmountIns = new List<BigInteger>() { 1000 };
//             multiSwapFunction.AmountOutMins = new List<BigInteger>() { BigInteger.Parse("0") };
//             multiSwapFunction.PathSlices = new List<byte>() { 4 };
//             multiSwapFunction.Paths = new List<string>() { "0xbb4cdb9cbd36b01bd1cbaebf2de08d9173bc095c", "0xe9e7cea3dedca5984780bafc599bd69add087d56", "0x55d398326f99059ff775485246999027b3197955", "0xbb4cdb9cbd36b01bd1cbaebf2de08d9173bc095c" };
            var sign1 = await contractHandler.SignTransactionAsync(multiSwapFunction);
            

            Logger.Debug("sign1 :" + sign1);
            Logger.Debug($" transfer.Nonce: { transfer.Nonce}   contract.Address {contract.Address}"  );
            var txId = await web3.Eth.Transactions.SendRawTransaction.SendRequestAsync("0x" + sign1);
            Logger.Debug("txId:"+txId);

        }
        [Function("multiSwap")]
        class MultiSwapFunctionBase : FunctionMessage
        {
            [Parameter("uint8[]", "symbols", 1)]
            public virtual  byte[] symbols { get; set; }
            [Parameter("uint[]", "amountIns", 2)]
            public virtual BigInteger[] amountIns { get; set; }
            [Parameter("uint[]", "amountOutMins", 3)]
            public virtual BigInteger[] amountOutMins { get; set; }
            [Parameter("uint8[]", "pathSlices", 4)]
            public virtual byte[] pathSlices { get; set; }
            [Parameter("address[]", "paths", 5)]
            public virtual string[] paths { get; set; }
            // 
            //             [Parameter("tuple[]", "symbols", 1)]
            //             public virtual byte[] symbols { get; set; }
            //             [Parameter("tuple[]", "amountIns", 2)]
            //             public virtual uint[] amountIns { get; set; }
            //             [Parameter("tuple[]", "amountOutMins", 3)]
            //             public virtual uint[] amountOutMins { get; set; }
            //             [Parameter("tuple[]", "pathSlices", 4)]
            //             public virtual byte[] pathSlices { get; set; }
            //             [Parameter("tuple[]", "paths", 5)]
            //             public virtual string[] paths { get; set; }
        }

    }
    /// <summary>
    /// 交易池中的交易对
    /// </summary>
    public class PoolPairs
    {
        public PoolToken poolToken0;

        public PoolToken poolToken1;

        public string exchangeName;

        public PoolPairs(PoolToken poolTokenA, PoolToken poolTokenB,string exchangeName)
        {
            this.poolToken0 = poolTokenA;
            this.poolToken1 = poolTokenB;
            this.exchangeName = exchangeName;
        }

        public PoolToken GetToken(string addr, string poolAddress="")
        {
            if (poolToken0.tokenAddress == addr )
            {
                return poolToken0;
            }
            else if(poolToken1.tokenAddress == addr)
            {
                return poolToken1;
            }
            else
            {
                throw new Exception($"没有找到 token{addr}      pool address: {poolAddress}");
            }
        }
        public override string ToString()
        {
            return $"exchangeName {exchangeName} poolToken0 :{poolToken0.ToString()} poolToken1 :{poolToken1.ToString()}   ";
        }
    }

    public class PoolToken
    {
        private string _tokenAddress;

        public PoolToken(string tokenSymbol, BigDecimal tokenReverse, string tokenAddress,int decimalNum)
        {
            this.tokenSymbol = tokenSymbol;
            this.tokenReverse = tokenReverse;
            this.tokenAddress = tokenAddress;
            this.decimalNum = decimalNum;
        }

        /// <summary>
        /// 币种名称
        /// </summary>
        public string tokenSymbol { get; set; }
        /// <summary>
        /// 池子中数量
        /// </summary>
        public BigDecimal tokenReverse { get; set; }
        /// <summary>
        /// 币种地址
        /// </summary>
        public string tokenAddress { get => _tokenAddress; set => _tokenAddress = value.ToLower(); }
        /// <summary>
        /// 小数点数位
        /// </summary>
        public int decimalNum { get; set; }

        public override string ToString()
        {
            return $" tokenReverse {tokenReverse}  tokenAddress {tokenAddress}   ";
        }

        public  BigInteger tokenReverse_bigInteger( double reverse)
        {
            //return (BigInteger)(reverse * Math.Pow(10, decimalNum));
            return (BigInteger)(reverse * 1);
        }

        public PoolToken Clone()
        {
            return new PoolToken(tokenSymbol, tokenReverse, tokenAddress, decimalNum);
        }
    }

    public class PathDataAll
    {

        public List<(string exchange, List<(string address,decimal reverse)> tokenAddresses)> paths;

        public PathDataAll(string str)
        //exchangeName:USDT&234-BNB&234,exchangeName:USDT&234-BNB&234
        {
            paths = new List<(string exchange, List<(string address, decimal reverse)> tokenAddresses)>();
            //里面包含多个交易所
            var strs = str.Split(',');
            foreach (var item in strs)
            {
                (string exchange, List<(string address, decimal reverse)> tokenAddresses) path = ("", new List<(string address, decimal reverse)>());
                var pathStr = item.Split(':');
                path.exchange = pathStr[0];
                var tokenAddresses = new List<string>( pathStr[1].Split('-'));
                foreach (var p in tokenAddresses)
                {
                    var pp = p.Split('&');
                    var tokenAddresse = (pp[0], decimal.Parse(pp[1]));
                    path.tokenAddresses.Add(tokenAddresse);
                }
                paths.Add(path);   
            }
        }
        /// <summary>
        /// 返回利润
        /// </summary>
        /// <param name="maxBalance"></param>
        /// <returns></returns>
        public decimal GetProfit(decimal maxBalance)
        {
            return 0;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var path in paths)
            {
                sb.Append($"{path.exchange}:");
                foreach (var item in path.tokenAddresses)
                {
                    sb.Append($"{item.address}-{item.reverse}->");
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
    /// <summary>
    /// 恒定参数做市
    /// </summary>
    public class CFMM
    {
        /// <summary>
        /// 起始数量币0 入币
        /// </summary>
        public BigDecimal R0;
        /// <summary>
        /// 起始数量币1 出币
        /// </summary>
        public BigDecimal R1;
        /// <summary>
        /// 入币0数量  已知
        /// </summary>
        //private decimal DeltaA;
        /// <summary>
        /// 出币1 数量  求解
        /// </summary>
        //private decimal DeltaB;

        public CFMM(BigDecimal r0, BigDecimal r1)
        {
            R0 = r0;
            R1 = r1;
            //DeltaA = deltaA;
        }
        public CFMM(PoolPairs poolPairs)
        {
            R0 = poolPairs.poolToken0.tokenReverse;
            R1 = poolPairs.poolToken1.tokenReverse;
        }

        public CFMM()
        {

        }



        /// <summary>
        /// 求解 detaB
        /// </summary>
        /// <param name="cfmm"></param>
        /// <param name="fee"></param>
        /// <returns></returns>
        public static BigDecimal GetDeltaB(CFMM cfmm, BigDecimal fee, BigDecimal DeltaA)
        {
            BigDecimal r = 1 - fee;
            BigDecimal deltaB = cfmm.R1 * r * DeltaA / (cfmm.R0 + r * DeltaA);
            return deltaB;
        }

        /// <summary>
        /// 获取  最优兑换数量
        /// </summary>
        /// <param name="amountStart"></param>
        /// <param name="amountEnd"></param>
        /// <returns></returns>
//         public static BigDecimal GetBestChangeAmount(BigDecimal r0, BigDecimal r1, BigDecimal fee)
//         {
//             BigDecimal amountStart = r0;
//             BigDecimal amountEnd = r1;
//             BigDecimal r = 1 - fee;
//             double db = (double)(amountStart * amountEnd * r);
//             BigDecimal de = (decimal) Math.Sqrt(db);
//             BigDecimal bestAmount = ((de - amountStart) / r);
//             //decimal bestAmount = ((Math.Sqrt((amountStart * amountEnd * r)) - amountStart) / r);
//             return bestAmount;
//         }

        /// <summary>
        /// 获取  最优兑换数量
        /// </summary>
        /// <param name="amountStart"></param>
        /// <param name="amountEnd"></param>
        /// <returns></returns>
        public static BigDecimal GetBestChangeAmount(BigDecimal r0, BigDecimal r1, BigDecimal fee)
        {
            BigDecimal amountStart = r0;
            BigDecimal amountEnd = r1;
            BigDecimal r = 1 - fee;
            var db = (amountStart * amountEnd * r);
            BigDecimal de =db.Sqrt2(0.000001m);
            BigDecimal bestAmount = ((de - amountStart) / r);
            //decimal bestAmount = ((Math.Sqrt((amountStart * amountEnd * r)) - amountStart) / r);
            return bestAmount;
        }
        /// <summary>
        /// 通过 池 A-B 和 B-C ，返回虚拟池  A-C 的CFMM
        /// </summary>
        /// <param name="A_B">c1 池交易对 A-B</param>
        /// <param name="B_C">c2 池交易对 B-C</param>
        /// <returns>返回 虚拟交易池 A-C 的参数 </returns>
        public static CFMM GetVisualCFMM(decimal fee ,params PoolPairs [] poolPairsPaths)
        {
            CFMM A_B = null;
            CFMM B_C = null;
            for (int i = 0; i < poolPairsPaths.Length-1; i++)
            {
               
                if (i==0)
                {
                    A_B = new CFMM(poolPairsPaths[i]);
                    B_C = new CFMM(poolPairsPaths[i + 1]);
                    
                }
                else
                {
                    B_C = new CFMM(poolPairsPaths[i + 1]);
                }
                decimal r = 1 - fee;
                CFMM A_C = new CFMM();
                var E0 = (A_B.R0 * B_C.R0) / (B_C.R0 + A_B.R1 * r);
                var E1 = (r * A_B.R1 * B_C.R1) / (B_C.R0 + A_B.R1 * r);

                A_C.R0 = E0;
                A_C.R1 = E1;
                A_B = A_C;

            }
            return A_B;
           
        }
        public static CFMM GetVisualCFMM(decimal fee, params CFMM[] cfmms)
        {
            CFMM A_B = null;
            CFMM B_C = null;
            for (int i = 0; i < cfmms.Length - 1; i++)
            {

                if (i == 0)
                {
                    A_B = cfmms[i];
                    B_C =  (cfmms[i + 1]);

                }
                else
                {
                    B_C =  (cfmms[i + 1]);
                }
                decimal r = 1 - fee;
                CFMM A_C = new CFMM();
                var E0 = (A_B.R0 * B_C.R0) / (B_C.R0 + A_B.R1 * r);
                var E1 = (r * A_B.R1 * B_C.R1) / (B_C.R0 + A_B.R1 * r);

                A_C.R0 = E0;
                A_C.R1 = E1;
                A_B = A_C;

            }
            return A_B;

        }

    }
}
