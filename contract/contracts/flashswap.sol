//SPDX-License-Identifier: Unlicense
pragma solidity ^0.8.0;
import "hardhat/console.sol";
import "./dependencies.sol";
import "./access.sol";
address constant WBNB = 0xbb4CdB9CBd36B01bD1cBaEBF2De08d9173bc095c;
uint256 constant MAX_INT = 115792089237316195423570985008687907853269984665640564039457584007913129639935;
address constant BISWAP_FACTORY = 0x858E3312ed3A876947EA49d572A7C42DE08af7EE;
address constant PANCAKE_FACTORY = 0xcA143Ce32Fe78f1f7019d7d551a6402fC5350c73;

contract Flashswap is Access {
    string public name = "flashswapv1";

    constructor() Access() {}

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

    function multiSwap(
        uint8[] memory symbols,
        uint[] memory amountIns,
        uint[] memory amountOutMins,
        uint8[] memory pathSlices,
        address[] memory paths
    ) public onlyAdmin {
        uint8 cursor = 0;
        for (uint8 i = 0; i < symbols.length; i++) {
            uint8 symbol = symbols[i];
            uint amountIn = amountIns[i];
            uint amountOutMin = amountOutMins[i];
            uint8 slice = pathSlices[i];
            address[] memory path = new address[](slice);
            for (uint j = 0; j < slice; j++) {
                path[j] = paths[cursor + j];
            }
            cursor = cursor + slice;

            console.log("symbol", symbol);
            console.log("amountIn", amountIn);
            console.log("amountOutMin", amountOutMin);
            for (uint jz = 0; jz < path.length; jz++) {
                console.log("path", path[jz]);
            }

            if (symbol == 1) {
                uint[] memory amounts = pancakeSwapV2(
                    amountIn,
                    amountOutMin,
                    path,
                    address(this)
                );
                console.log("amounts[1]", amounts[1]);
            } else if (symbol == 2) {
                biSwapV2(amountIn, amountOutMin, path, address(this));
            }
        }
    }

    function pancakeSwapV2(
        uint amountIn,
        uint amountOutMin,
        address[] memory path,
        address to
    ) public onlyAdmin returns (uint[] memory amounts) {
        amounts = PancakeLibrary.getAmountsOut(PANCAKE_FACTORY, amountIn, path);
        require(
            amounts[amounts.length - 1] >= amountOutMin,
            "PancakeRouter: INSUFFICIENT_OUTPUT_AMOUNT"
        );
        IERC20(path[0]).transfer(
            PancakeLibrary.pairFor(PANCAKE_FACTORY, path[0], path[1]),
            amounts[0]
        );
        _pancakeSwap(amounts, path, to);
    }

    // **** SWAP ****
    // requires the initial amount to have already been sent to the first pair
    function _pancakeSwap(
        uint[] memory amounts,
        address[] memory path,
        address _to
    ) internal virtual {
        for (uint i; i < path.length - 1; i++) {
            (address input, address output) = (path[i], path[i + 1]);
            (address token0, ) = PancakeLibrary.sortTokens(input, output);
            uint amountOut = amounts[i + 1];
            (uint amount0Out, uint amount1Out) = input == token0
                ? (uint(0), amountOut)
                : (amountOut, uint(0));
            address to = i < path.length - 2
                ? PancakeLibrary.pairFor(PANCAKE_FACTORY, output, path[i + 2])
                : _to;
            IPancakePair(PancakeLibrary.pairFor(PANCAKE_FACTORY, input, output))
                .swap(amount0Out, amount1Out, to, new bytes(0));
        }
    }

    function biSwapV2(
        uint amountIn,
        uint amountOutMin,
        address[] memory path,
        address to
    ) public onlyAdmin returns (uint[] memory amounts) {
        amounts = BiswapLibrary.getAmountsOut(BISWAP_FACTORY, amountIn, path);
        require(
            amounts[amounts.length - 1] >= amountOutMin,
            "BiswapV2Router: INSUFFICIENT_OUTPUT_AMOUNT"
        );
        IERC20(path[0]).transfer(
            BiswapLibrary.pairFor(BISWAP_FACTORY, path[0], path[1]),
            amounts[0]
        );
        _biSwap(amounts, path, to);
    }

    // **** SWAP ****
    // requires the initial amount to have already been sent to the first pair
    function _biSwap(
        uint[] memory amounts,
        address[] memory path,
        address _to
    ) internal virtual {
        for (uint i; i < path.length - 1; i++) {
            (address input, address output) = (path[i], path[i + 1]);
            (address token0, ) = BiswapLibrary.sortTokens(input, output);
            uint amountOut = amounts[i + 1];
            (uint amount0Out, uint amount1Out) = input == token0
                ? (uint(0), amountOut)
                : (amountOut, uint(0));
            address to = i < path.length - 2
                ? BiswapLibrary.pairFor(BISWAP_FACTORY, output, path[i + 2])
                : _to;
            IBiswapPair(BiswapLibrary.pairFor(BISWAP_FACTORY, input, output))
                .swap(amount0Out, amount1Out, to, new bytes(0));
        }
    }
}
