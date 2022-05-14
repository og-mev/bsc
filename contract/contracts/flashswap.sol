//SPDX-License-Identifier: Unlicense
pragma solidity ^0.8.0;
import "hardhat/console.sol";
import "./dependencies.sol";
import "./access.sol";
address constant WBNB = 0xbb4CdB9CBd36B01bD1cBaEBF2De08d9173bc095c;

contract Flashswap is Access {
    struct Agg {
        uint8 symbol;
        address pair;
        uint amount;
        uint amountOutMin;
        address fromToken;
        address toToken;
    }

    constructor() Access() {}

    function swap(Agg[] calldata aggs) public onlyOwner {
        for (uint8 i = 0; i < aggs.length; i++) {
            console.log("agg pair: ", aggs[i].pair);
            console.log("agg amount: ", aggs[i].amount);
            addAdmin(aggs[i].pair);
        }
    }

    function withdraw(
        address token,
        uint amount,
        address to
    ) public onlyAdmin {
        IERC20(token).transfer(to, amount);
    }

    function withdrawValue(uint amount) public onlyAdmin {
        IWBNB(WBNB).withdraw(amount);
        payable(msg.sender).transfer(address(this).balance);
    }
}
