using Nethereum.ABI.FunctionEncoding.Attributes;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace arbitrage_CSharp
{
    [FunctionOutput]
    public class ReservesDto : IFunctionOutputDTO
    {
        [Parameter("uint112", "reserve0", 1, false)]
        public BigInteger Reserve0 { get; set; }

        [Parameter("uint112", "reserve1", 2, false)]
        public BigInteger Reserve1 { get; set; }

        [Parameter("uint32", "blockTimestampLast", 3, true)]
        public BigInteger BlockTimestampLast { get; set; }

    }
}
