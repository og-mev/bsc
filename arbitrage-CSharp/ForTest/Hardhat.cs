using Nethereum.JsonRpc.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace arbitrage_CSharp.ForTest
{
    public class HardhatResetInput
    {
        [JsonProperty(PropertyName = "forking")]
        public HardhatForkInput Forking { get; set; }
    }

    public class HardhatForkInput
    {
        [JsonProperty(PropertyName = "jsonRpcUrl")]
        public string JsonRpcUrl { get; set; }

        [JsonProperty(PropertyName = "blockNumber")]
        public long BlockNumber { get; set; }
    }

    public class HardhatReset : RpcRequestResponseHandler<bool>
    {
        public HardhatReset(IClient client) : base(client, "hardhat_reset")
        {
        }

        public Task<bool> SendRequestAsync(HardhatResetInput input, object id = null)
        {
            return base.SendRequestAsync(id, input);
        }

        public RpcRequest BuildRequest(HardhatResetInput input, object id = null)
        {
            return base.BuildRequest(id, input);
        }
    }
}
