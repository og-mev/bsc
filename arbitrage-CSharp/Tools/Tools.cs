using Nethereum.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace arbitrage_CSharp.Tools
{
    public static class Tools
    {
        public static BigDecimal Sqrt(this BigDecimal x, BigDecimal? guess = null)
        {
            var ourGuess = guess.GetValueOrDefault(x / 2m);
            var result = x / ourGuess;
            var average = (ourGuess + result) / 2m;

            if (average == ourGuess) // This checks for the maximum precision possible with a decimal.
                return average;
            else
                return Sqrt(x, average);
        }
        public static BigDecimal Sqrt2(this BigDecimal c, decimal epsilon = 0.0M)
        {
            if (c < 0)
            {
                return 0;
            }
            BigDecimal e = 1e-50m;
            BigDecimal x = c;
            BigDecimal y = (x + c / x) / 2;

            var v = x - y;

            bool canDo = true;
            canDo = (v > 0 ? v : -v) > epsilon;
            while (canDo)
            {
                x = y;
                y = (x + c / x) / 2;

                v = x - y;
                canDo = (v > 0 ? v : -v) > epsilon;
            }
            return x;
        }
    }

    public static DecodeTransaction<FunctionMessage>()
    {

    }


}
