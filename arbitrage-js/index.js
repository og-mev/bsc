const { ethers } = require("ethers");
const ccxt = require('ccxt')
const { ERC20, SwapPair, SwapBridge } = require("./common/classes");
async function main() {
    let swapContract = require("../contract/deploy.json").address;
    const dexBridge = new SwapBridge("BSC", swapContract, null);
    await sleep(2000);
    await dexBridge.import_wallets(["0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80"]);
    await cex2dex(dexBridge, null);
}

function symbol2token(chain, symbol) {
    let json = require("./symbol2token.json");
    if (json.hasOwnProperty(chain)) {
        return json[chain][symbol];
    } else {
        return null;
    }

}

async function cex2dex(dexBridge, cexBridge) {
    let exchange = new ccxt.binance();
    await exchange.loadMarkets();
    let symbols = exchange.symbols;
    symbols.map(symbol => {
        let symbol0 = symbol.split("/")[0];
        let symbol1 = symbol.split("/")[1];
        let token0 = symbol2token("BSC", symbol0);
        let token1 = symbol2token("BSC", symbol1);
        if (token0)
            console.log(token0);
        if (token1)
            console.log(token1)
    })
    return


    let token0 = symbol2token("BSC", "BNB");
    let token1 = symbol2token("BSC", "BUSD");
    console.log(token0);
    if (!token0 || !token1) {
        console.log("can't find token");
    }
    // return;
    //demo params array
    let dexOrder = [{
        symbol: "pancakeswap",
        amountIn: ethers.utils.parseEther("2.0"),
        amountOutMin: ethers.utils.parseEther("1.0"),
        path: [token0, token1]
    }];
    await dexBridge.swap(dexOrder);


}



function sleep(ms) {
    return new Promise((resolve) => {
        setTimeout(resolve, ms);
    });
}

// We recommend this pattern to be able to use async/await everywhere
// and properly handle errors.
main()
    .then(() => process.exit(0))
    .catch((error) => {
        console.error(error);
        process.exit(1);
    });
