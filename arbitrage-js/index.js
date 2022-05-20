const { ethers } = require("ethers");
const { ERC20, SwapPair, SwapBridge } = require("./common/classes");
async function main() {
    console.log("hello world!");
    const bridge = new SwapBridge("BSC","0xEDF0E9D1d84706a389BD93B572b86D0739EfDF11");
    await sleep(2000);
    //demo params array
    let swapArr = [{
        symbol: "pancakeswap",
        amountIn: ethers.utils.parseEther("2.0"),
        amountOutMin: ethers.utils.parseEther("1.0"),
        path: ["0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56", "0x55d398326f99059ff775485246999027b3197955"]
    }];
    await bridge.import_wallets(["0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80"]);
    await bridge.swap(swapArr);
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
