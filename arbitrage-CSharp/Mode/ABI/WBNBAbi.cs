using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Web3;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Contracts.CQS;
using Nethereum.Contracts;
using System.Threading;

namespace WBNB
{


    public class WbnbConsole
    {
        public static async Task Main()
        {
            var url = "http://testchain.nethereum.com:8545";
            //var url = "https://mainnet.infura.io";
            var privateKey = "0x7580e7fb49df1c861f0050fae31c2224c6aba908e116b8da44ee8cd927b990b0";
            var account = new Nethereum.Web3.Accounts.Account(privateKey);
            var web3 = new Web3(account, url);

            string contractAddress = "";
            /* Deployment 
           var wbnbDeployment = new WbnbDeployment();

           var transactionReceiptDeployment = await web3.Eth.GetContractDeploymentHandler<WbnbDeployment>().SendRequestAndWaitForReceiptAsync(wbnbDeployment);
           var contractAddress = transactionReceiptDeployment.ContractAddress;
            */
            var contractHandler = web3.Eth.GetContractHandler(contractAddress);

            /** Function: name**/
            /*
            var nameFunctionReturn = await contractHandler.QueryAsync<NameFunction, string>();
            */


            /** Function: approve**/
            /*
            var approveFunction = new ApproveFunction();
            approveFunction.Guy = guy;
            approveFunction.Wad = wad;
            var approveFunctionTxnReceipt = await contractHandler.SendRequestAndWaitForReceiptAsync(approveFunction);
            */


            /** Function: totalSupply**/
            /*
            var totalSupplyFunctionReturn = await contractHandler.QueryAsync<TotalSupplyFunction, BigInteger>();
            */


            /** Function: transferFrom**/
            /*
            var transferFromFunction = new TransferFromFunction();
            transferFromFunction.Src = src;
            transferFromFunction.Dst = dst;
            transferFromFunction.Wad = wad;
            var transferFromFunctionTxnReceipt = await contractHandler.SendRequestAndWaitForReceiptAsync(transferFromFunction);
            */


            /** Function: withdraw**/
            /*
            var withdrawFunction = new WithdrawFunction();
            withdrawFunction.Wad = wad;
            var withdrawFunctionTxnReceipt = await contractHandler.SendRequestAndWaitForReceiptAsync(withdrawFunction);
            */


            /** Function: decimals**/
            /*
            var decimalsFunctionReturn = await contractHandler.QueryAsync<DecimalsFunction, byte>();
            */


            /** Function: balanceOf**/
            /*
            var balanceOfFunction = new BalanceOfFunction();
            balanceOfFunction.ReturnValue1 = returnValue1;
            var balanceOfFunctionReturn = await contractHandler.QueryAsync<BalanceOfFunction, BigInteger>(balanceOfFunction);
            */


            /** Function: symbol**/
            /*
            var symbolFunctionReturn = await contractHandler.QueryAsync<SymbolFunction, string>();
            */


            /** Function: transfer**/
            /*
            var transferFunction = new TransferFunction();
            transferFunction.Dst = dst;
            transferFunction.Wad = wad;
            var transferFunctionTxnReceipt = await contractHandler.SendRequestAndWaitForReceiptAsync(transferFunction);
            */


            /** Function: deposit**/
            /*
            var depositFunctionTxnReceipt = await contractHandler.SendRequestAndWaitForReceiptAsync<DepositFunction>();
            */


            /** Function: allowance**/
            /*
            var allowanceFunction = new AllowanceFunction();
            allowanceFunction.ReturnValue1 = returnValue1;
            allowanceFunction.ReturnValue2 = returnValue2;
            var allowanceFunctionReturn = await contractHandler.QueryAsync<AllowanceFunction, BigInteger>(allowanceFunction);
            */
        }

    }

    public partial class WbnbDeployment : WbnbDeploymentBase
    {
        public WbnbDeployment() : base(BYTECODE) { }
        public WbnbDeployment(string byteCode) : base(byteCode) { }
    }

    public class WbnbDeploymentBase : ContractDeploymentMessage
    {
        public static string BYTECODE = @"{ 	""linkReferences"": {}, 	""object"": ""60c0604052600b60808190527f5772617070656420424e4200000000000000000000000000000000000000000060a090815261003e91600091906100a3565b506040805180820190915260048082527f57424e42000000000000000000000000000000000000000000000000000000006020909201918252610083916001916100a3565b506002805460ff1916601217905534801561009d57600080fd5b5061013e565b828054600181600116156101000203166002900490600052602060002090601f016020900481019282601f106100e457805160ff1916838001178555610111565b82800160010185558215610111579182015b828111156101115782518255916020019190600101906100f6565b5061011d929150610121565b5090565b61013b91905b8082111561011d5760008155600101610127565b90565b6106728061014d6000396000f3006080604052600436106100ae5763ffffffff7c010000000000000000000000000000000000000000000000000000000060003504166306fdde0381146100b8578063095ea7b31461014257806318160ddd1461017a57806323b872dd146101a15780632e1a7d4d146101cb578063313ce567146101e357806370a082311461020e57806395d89b411461022f578063a9059cbb14610244578063d0e30db0146100ae578063dd62ed3e14610268575b6100b661028f565b005b3480156100c457600080fd5b506100cd6102de565b6040805160208082528351818301528351919283929083019185019080838360005b838110156101075781810151838201526020016100ef565b50505050905090810190601f1680156101345780820380516001836020036101000a031916815260200191505b509250505060405180910390f35b34801561014e57600080fd5b50610166600160a060020a036004351660243561036c565b604080519115158252519081900360200190f35b34801561018657600080fd5b5061018f6103d2565b60408051918252519081900360200190f35b3480156101ad57600080fd5b50610166600160a060020a03600435811690602435166044356103d7565b3480156101d757600080fd5b506100b660043561050b565b3480156101ef57600080fd5b506101f86105a0565b6040805160ff9092168252519081900360200190f35b34801561021a57600080fd5b5061018f600160a060020a03600435166105a9565b34801561023b57600080fd5b506100cd6105bb565b34801561025057600080fd5b50610166600160a060020a0360043516602435610615565b34801561027457600080fd5b5061018f600160a060020a0360043581169060243516610629565b33600081815260036020908152604091829020805434908101909155825190815291517fe1fffcc4923d04b559f4d29a8bfc6cda04eb5b0d3c460751c2402c5c5cc9109c9281900390910190a2565b6000805460408051602060026001851615610100026000190190941693909304601f810184900484028201840190925281815292918301828280156103645780601f1061033957610100808354040283529160200191610364565b820191906000526020600020905b81548152906001019060200180831161034757829003601f168201915b505050505081565b336000818152600460209081526040808320600160a060020a038716808552908352818420869055815186815291519394909390927f8c5be1e5ebec7d5bd14f71427d1e84f3dd0314c0f7b2291e5b200ac8c7c3b925928290030190a350600192915050565b303190565b600160a060020a0383166000908152600360205260408120548211156103fc57600080fd5b600160a060020a038416331480159061043a5750600160a060020a038416600090815260046020908152604080832033845290915290205460001914155b1561049a57600160a060020a038416600090815260046020908152604080832033845290915290205482111561046f57600080fd5b600160a060020a03841660009081526004602090815260408083203384529091529020805483900390555b600160a060020a03808516600081815260036020908152604080832080548890039055938716808352918490208054870190558351868152935191937fddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef929081900390910190a35060019392505050565b3360009081526003602052604090205481111561052757600080fd5b33600081815260036020526040808220805485900390555183156108fc0291849190818181858888f19350505050158015610566573d6000803e3d6000fd5b5060408051828152905133917f7fcf532c15f0a6db0bd6d0e038bea71d30d808c7d98cb3bf7268a95bf5081b65919081900360200190a250565b60025460ff1681565b60036020526000908152604090205481565b60018054604080516020600284861615610100026000190190941693909304601f810184900484028201840190925281815292918301828280156103645780601f1061033957610100808354040283529160200191610364565b60006106223384846103d7565b9392505050565b6004602090815260009283526040808420909152908252902054815600a165627a7a7230582010d159c39a4730a343e0e303a180cc4565cfa34d41ec11698406d16ea9b50d9a0029"", 	""opcodes"": ""PUSH1 0xC0 PUSH1 0x40 MSTORE PUSH1 0xB PUSH1 0x80 DUP2 SWAP1 MSTORE PUSH32 0x5772617070656420424E42000000000000000000000000000000000000000000 PUSH1 0xA0 SWAP1 DUP2 MSTORE PUSH2 0x3E SWAP2 PUSH1 0x0 SWAP2 SWAP1 PUSH2 0xA3 JUMP JUMPDEST POP PUSH1 0x40 DUP1 MLOAD DUP1 DUP3 ADD SWAP1 SWAP2 MSTORE PUSH1 0x4 DUP1 DUP3 MSTORE PUSH32 0x57424E4200000000000000000000000000000000000000000000000000000000 PUSH1 0x20 SWAP1 SWAP3 ADD SWAP2 DUP3 MSTORE PUSH2 0x83 SWAP2 PUSH1 0x1 SWAP2 PUSH2 0xA3 JUMP JUMPDEST POP PUSH1 0x2 DUP1 SLOAD PUSH1 0xFF NOT AND PUSH1 0x12 OR SWAP1 SSTORE CALLVALUE DUP1 ISZERO PUSH2 0x9D JUMPI PUSH1 0x0 DUP1 REVERT JUMPDEST POP PUSH2 0x13E JUMP JUMPDEST DUP3 DUP1 SLOAD PUSH1 0x1 DUP2 PUSH1 0x1 AND ISZERO PUSH2 0x100 MUL SUB AND PUSH1 0x2 SWAP1 DIV SWAP1 PUSH1 0x0 MSTORE PUSH1 0x20 PUSH1 0x0 KECCAK256 SWAP1 PUSH1 0x1F ADD PUSH1 0x20 SWAP1 DIV DUP2 ADD SWAP3 DUP3 PUSH1 0x1F LT PUSH2 0xE4 JUMPI DUP1 MLOAD PUSH1 0xFF NOT AND DUP4 DUP1 ADD OR DUP6 SSTORE PUSH2 0x111 JUMP JUMPDEST DUP3 DUP1 ADD PUSH1 0x1 ADD DUP6 SSTORE DUP3 ISZERO PUSH2 0x111 JUMPI SWAP2 DUP3 ADD JUMPDEST DUP3 DUP2 GT ISZERO PUSH2 0x111 JUMPI DUP3 MLOAD DUP3 SSTORE SWAP2 PUSH1 0x20 ADD SWAP2 SWAP1 PUSH1 0x1 ADD SWAP1 PUSH2 0xF6 JUMP JUMPDEST POP PUSH2 0x11D SWAP3 SWAP2 POP PUSH2 0x121 JUMP JUMPDEST POP SWAP1 JUMP JUMPDEST PUSH2 0x13B SWAP2 SWAP1 JUMPDEST DUP1 DUP3 GT ISZERO PUSH2 0x11D JUMPI PUSH1 0x0 DUP2 SSTORE PUSH1 0x1 ADD PUSH2 0x127 JUMP JUMPDEST SWAP1 JUMP JUMPDEST PUSH2 0x672 DUP1 PUSH2 0x14D PUSH1 0x0 CODECOPY PUSH1 0x0 RETURN STOP PUSH1 0x80 PUSH1 0x40 MSTORE PUSH1 0x4 CALLDATASIZE LT PUSH2 0xAE JUMPI PUSH4 0xFFFFFFFF PUSH29 0x100000000000000000000000000000000000000000000000000000000 PUSH1 0x0 CALLDATALOAD DIV AND PUSH4 0x6FDDE03 DUP2 EQ PUSH2 0xB8 JUMPI DUP1 PUSH4 0x95EA7B3 EQ PUSH2 0x142 JUMPI DUP1 PUSH4 0x18160DDD EQ PUSH2 0x17A JUMPI DUP1 PUSH4 0x23B872DD EQ PUSH2 0x1A1 JUMPI DUP1 PUSH4 0x2E1A7D4D EQ PUSH2 0x1CB JUMPI DUP1 PUSH4 0x313CE567 EQ PUSH2 0x1E3 JUMPI DUP1 PUSH4 0x70A08231 EQ PUSH2 0x20E JUMPI DUP1 PUSH4 0x95D89B41 EQ PUSH2 0x22F JUMPI DUP1 PUSH4 0xA9059CBB EQ PUSH2 0x244 JUMPI DUP1 PUSH4 0xD0E30DB0 EQ PUSH2 0xAE JUMPI DUP1 PUSH4 0xDD62ED3E EQ PUSH2 0x268 JUMPI JUMPDEST PUSH2 0xB6 PUSH2 0x28F JUMP JUMPDEST STOP JUMPDEST CALLVALUE DUP1 ISZERO PUSH2 0xC4 JUMPI PUSH1 0x0 DUP1 REVERT JUMPDEST POP PUSH2 0xCD PUSH2 0x2DE JUMP JUMPDEST PUSH1 0x40 DUP1 MLOAD PUSH1 0x20 DUP1 DUP3 MSTORE DUP4 MLOAD DUP2 DUP4 ADD MSTORE DUP4 MLOAD SWAP2 SWAP3 DUP4 SWAP3 SWAP1 DUP4 ADD SWAP2 DUP6 ADD SWAP1 DUP1 DUP4 DUP4 PUSH1 0x0 JUMPDEST DUP4 DUP2 LT ISZERO PUSH2 0x107 JUMPI DUP2 DUP2 ADD MLOAD DUP4 DUP3 ADD MSTORE PUSH1 0x20 ADD PUSH2 0xEF JUMP JUMPDEST POP POP POP POP SWAP1 POP SWAP1 DUP2 ADD SWAP1 PUSH1 0x1F AND DUP1 ISZERO PUSH2 0x134 JUMPI DUP1 DUP3 SUB DUP1 MLOAD PUSH1 0x1 DUP4 PUSH1 0x20 SUB PUSH2 0x100 EXP SUB NOT AND DUP2 MSTORE PUSH1 0x20 ADD SWAP2 POP JUMPDEST POP SWAP3 POP POP POP PUSH1 0x40 MLOAD DUP1 SWAP2 SUB SWAP1 RETURN JUMPDEST CALLVALUE DUP1 ISZERO PUSH2 0x14E JUMPI PUSH1 0x0 DUP1 REVERT JUMPDEST POP PUSH2 0x166 PUSH1 0x1 PUSH1 0xA0 PUSH1 0x2 EXP SUB PUSH1 0x4 CALLDATALOAD AND PUSH1 0x24 CALLDATALOAD PUSH2 0x36C JUMP JUMPDEST PUSH1 0x40 DUP1 MLOAD SWAP2 ISZERO ISZERO DUP3 MSTORE MLOAD SWAP1 DUP2 SWAP1 SUB PUSH1 0x20 ADD SWAP1 RETURN JUMPDEST CALLVALUE DUP1 ISZERO PUSH2 0x186 JUMPI PUSH1 0x0 DUP1 REVERT JUMPDEST POP PUSH2 0x18F PUSH2 0x3D2 JUMP JUMPDEST PUSH1 0x40 DUP1 MLOAD SWAP2 DUP3 MSTORE MLOAD SWAP1 DUP2 SWAP1 SUB PUSH1 0x20 ADD SWAP1 RETURN JUMPDEST CALLVALUE DUP1 ISZERO PUSH2 0x1AD JUMPI PUSH1 0x0 DUP1 REVERT JUMPDEST POP PUSH2 0x166 PUSH1 0x1 PUSH1 0xA0 PUSH1 0x2 EXP SUB PUSH1 0x4 CALLDATALOAD DUP2 AND SWAP1 PUSH1 0x24 CALLDATALOAD AND PUSH1 0x44 CALLDATALOAD PUSH2 0x3D7 JUMP JUMPDEST CALLVALUE DUP1 ISZERO PUSH2 0x1D7 JUMPI PUSH1 0x0 DUP1 REVERT JUMPDEST POP PUSH2 0xB6 PUSH1 0x4 CALLDATALOAD PUSH2 0x50B JUMP JUMPDEST CALLVALUE DUP1 ISZERO PUSH2 0x1EF JUMPI PUSH1 0x0 DUP1 REVERT JUMPDEST POP PUSH2 0x1F8 PUSH2 0x5A0 JUMP JUMPDEST PUSH1 0x40 DUP1 MLOAD PUSH1 0xFF SWAP1 SWAP3 AND DUP3 MSTORE MLOAD SWAP1 DUP2 SWAP1 SUB PUSH1 0x20 ADD SWAP1 RETURN JUMPDEST CALLVALUE DUP1 ISZERO PUSH2 0x21A JUMPI PUSH1 0x0 DUP1 REVERT JUMPDEST POP PUSH2 0x18F PUSH1 0x1 PUSH1 0xA0 PUSH1 0x2 EXP SUB PUSH1 0x4 CALLDATALOAD AND PUSH2 0x5A9 JUMP JUMPDEST CALLVALUE DUP1 ISZERO PUSH2 0x23B JUMPI PUSH1 0x0 DUP1 REVERT JUMPDEST POP PUSH2 0xCD PUSH2 0x5BB JUMP JUMPDEST CALLVALUE DUP1 ISZERO PUSH2 0x250 JUMPI PUSH1 0x0 DUP1 REVERT JUMPDEST POP PUSH2 0x166 PUSH1 0x1 PUSH1 0xA0 PUSH1 0x2 EXP SUB PUSH1 0x4 CALLDATALOAD AND PUSH1 0x24 CALLDATALOAD PUSH2 0x615 JUMP JUMPDEST CALLVALUE DUP1 ISZERO PUSH2 0x274 JUMPI PUSH1 0x0 DUP1 REVERT JUMPDEST POP PUSH2 0x18F PUSH1 0x1 PUSH1 0xA0 PUSH1 0x2 EXP SUB PUSH1 0x4 CALLDATALOAD DUP2 AND SWAP1 PUSH1 0x24 CALLDATALOAD AND PUSH2 0x629 JUMP JUMPDEST CALLER PUSH1 0x0 DUP2 DUP2 MSTORE PUSH1 0x3 PUSH1 0x20 SWAP1 DUP2 MSTORE PUSH1 0x40 SWAP2 DUP3 SWAP1 KECCAK256 DUP1 SLOAD CALLVALUE SWAP1 DUP2 ADD SWAP1 SWAP2 SSTORE DUP3 MLOAD SWAP1 DUP2 MSTORE SWAP2 MLOAD PUSH32 0xE1FFFCC4923D04B559F4D29A8BFC6CDA04EB5B0D3C460751C2402C5C5CC9109C SWAP3 DUP2 SWAP1 SUB SWAP1 SWAP2 ADD SWAP1 LOG2 JUMP JUMPDEST PUSH1 0x0 DUP1 SLOAD PUSH1 0x40 DUP1 MLOAD PUSH1 0x20 PUSH1 0x2 PUSH1 0x1 DUP6 AND ISZERO PUSH2 0x100 MUL PUSH1 0x0 NOT ADD SWAP1 SWAP5 AND SWAP4 SWAP1 SWAP4 DIV PUSH1 0x1F DUP2 ADD DUP5 SWAP1 DIV DUP5 MUL DUP3 ADD DUP5 ADD SWAP1 SWAP3 MSTORE DUP2 DUP2 MSTORE SWAP3 SWAP2 DUP4 ADD DUP3 DUP3 DUP1 ISZERO PUSH2 0x364 JUMPI DUP1 PUSH1 0x1F LT PUSH2 0x339 JUMPI PUSH2 0x100 DUP1 DUP4 SLOAD DIV MUL DUP4 MSTORE SWAP2 PUSH1 0x20 ADD SWAP2 PUSH2 0x364 JUMP JUMPDEST DUP3 ADD SWAP2 SWAP1 PUSH1 0x0 MSTORE PUSH1 0x20 PUSH1 0x0 KECCAK256 SWAP1 JUMPDEST DUP2 SLOAD DUP2 MSTORE SWAP1 PUSH1 0x1 ADD SWAP1 PUSH1 0x20 ADD DUP1 DUP4 GT PUSH2 0x347 JUMPI DUP3 SWAP1 SUB PUSH1 0x1F AND DUP3 ADD SWAP2 JUMPDEST POP POP POP POP POP DUP2 JUMP JUMPDEST CALLER PUSH1 0x0 DUP2 DUP2 MSTORE PUSH1 0x4 PUSH1 0x20 SWAP1 DUP2 MSTORE PUSH1 0x40 DUP1 DUP4 KECCAK256 PUSH1 0x1 PUSH1 0xA0 PUSH1 0x2 EXP SUB DUP8 AND DUP1 DUP6 MSTORE SWAP1 DUP4 MSTORE DUP2 DUP5 KECCAK256 DUP7 SWAP1 SSTORE DUP2 MLOAD DUP7 DUP2 MSTORE SWAP2 MLOAD SWAP4 SWAP5 SWAP1 SWAP4 SWAP1 SWAP3 PUSH32 0x8C5BE1E5EBEC7D5BD14F71427D1E84F3DD0314C0F7B2291E5B200AC8C7C3B925 SWAP3 DUP3 SWAP1 SUB ADD SWAP1 LOG3 POP PUSH1 0x1 SWAP3 SWAP2 POP POP JUMP JUMPDEST ADDRESS BALANCE SWAP1 JUMP JUMPDEST PUSH1 0x1 PUSH1 0xA0 PUSH1 0x2 EXP SUB DUP4 AND PUSH1 0x0 SWAP1 DUP2 MSTORE PUSH1 0x3 PUSH1 0x20 MSTORE PUSH1 0x40 DUP2 KECCAK256 SLOAD DUP3 GT ISZERO PUSH2 0x3FC JUMPI PUSH1 0x0 DUP1 REVERT JUMPDEST PUSH1 0x1 PUSH1 0xA0 PUSH1 0x2 EXP SUB DUP5 AND CALLER EQ DUP1 ISZERO SWAP1 PUSH2 0x43A JUMPI POP PUSH1 0x1 PUSH1 0xA0 PUSH1 0x2 EXP SUB DUP5 AND PUSH1 0x0 SWAP1 DUP2 MSTORE PUSH1 0x4 PUSH1 0x20 SWAP1 DUP2 MSTORE PUSH1 0x40 DUP1 DUP4 KECCAK256 CALLER DUP5 MSTORE SWAP1 SWAP2 MSTORE SWAP1 KECCAK256 SLOAD PUSH1 0x0 NOT EQ ISZERO JUMPDEST ISZERO PUSH2 0x49A JUMPI PUSH1 0x1 PUSH1 0xA0 PUSH1 0x2 EXP SUB DUP5 AND PUSH1 0x0 SWAP1 DUP2 MSTORE PUSH1 0x4 PUSH1 0x20 SWAP1 DUP2 MSTORE PUSH1 0x40 DUP1 DUP4 KECCAK256 CALLER DUP5 MSTORE SWAP1 SWAP2 MSTORE SWAP1 KECCAK256 SLOAD DUP3 GT ISZERO PUSH2 0x46F JUMPI PUSH1 0x0 DUP1 REVERT JUMPDEST PUSH1 0x1 PUSH1 0xA0 PUSH1 0x2 EXP SUB DUP5 AND PUSH1 0x0 SWAP1 DUP2 MSTORE PUSH1 0x4 PUSH1 0x20 SWAP1 DUP2 MSTORE PUSH1 0x40 DUP1 DUP4 KECCAK256 CALLER DUP5 MSTORE SWAP1 SWAP2 MSTORE SWAP1 KECCAK256 DUP1 SLOAD DUP4 SWAP1 SUB SWAP1 SSTORE JUMPDEST PUSH1 0x1 PUSH1 0xA0 PUSH1 0x2 EXP SUB DUP1 DUP6 AND PUSH1 0x0 DUP2 DUP2 MSTORE PUSH1 0x3 PUSH1 0x20 SWAP1 DUP2 MSTORE PUSH1 0x40 DUP1 DUP4 KECCAK256 DUP1 SLOAD DUP9 SWAP1 SUB SWAP1 SSTORE SWAP4 DUP8 AND DUP1 DUP4 MSTORE SWAP2 DUP5 SWAP1 KECCAK256 DUP1 SLOAD DUP8 ADD SWAP1 SSTORE DUP4 MLOAD DUP7 DUP2 MSTORE SWAP4 MLOAD SWAP2 SWAP4 PUSH32 0xDDF252AD1BE2C89B69C2B068FC378DAA952BA7F163C4A11628F55A4DF523B3EF SWAP3 SWAP1 DUP2 SWAP1 SUB SWAP1 SWAP2 ADD SWAP1 LOG3 POP PUSH1 0x1 SWAP4 SWAP3 POP POP POP JUMP JUMPDEST CALLER PUSH1 0x0 SWAP1 DUP2 MSTORE PUSH1 0x3 PUSH1 0x20 MSTORE PUSH1 0x40 SWAP1 KECCAK256 SLOAD DUP2 GT ISZERO PUSH2 0x527 JUMPI PUSH1 0x0 DUP1 REVERT JUMPDEST CALLER PUSH1 0x0 DUP2 DUP2 MSTORE PUSH1 0x3 PUSH1 0x20 MSTORE PUSH1 0x40 DUP1 DUP3 KECCAK256 DUP1 SLOAD DUP6 SWAP1 SUB SWAP1 SSTORE MLOAD DUP4 ISZERO PUSH2 0x8FC MUL SWAP2 DUP5 SWAP2 SWAP1 DUP2 DUP2 DUP2 DUP6 DUP9 DUP9 CALL SWAP4 POP POP POP POP ISZERO DUP1 ISZERO PUSH2 0x566 JUMPI RETURNDATASIZE PUSH1 0x0 DUP1 RETURNDATACOPY RETURNDATASIZE PUSH1 0x0 REVERT JUMPDEST POP PUSH1 0x40 DUP1 MLOAD DUP3 DUP2 MSTORE SWAP1 MLOAD CALLER SWAP2 PUSH32 0x7FCF532C15F0A6DB0BD6D0E038BEA71D30D808C7D98CB3BF7268A95BF5081B65 SWAP2 SWAP1 DUP2 SWAP1 SUB PUSH1 0x20 ADD SWAP1 LOG2 POP JUMP JUMPDEST PUSH1 0x2 SLOAD PUSH1 0xFF AND DUP2 JUMP JUMPDEST PUSH1 0x3 PUSH1 0x20 MSTORE PUSH1 0x0 SWAP1 DUP2 MSTORE PUSH1 0x40 SWAP1 KECCAK256 SLOAD DUP2 JUMP JUMPDEST PUSH1 0x1 DUP1 SLOAD PUSH1 0x40 DUP1 MLOAD PUSH1 0x20 PUSH1 0x2 DUP5 DUP7 AND ISZERO PUSH2 0x100 MUL PUSH1 0x0 NOT ADD SWAP1 SWAP5 AND SWAP4 SWAP1 SWAP4 DIV PUSH1 0x1F DUP2 ADD DUP5 SWAP1 DIV DUP5 MUL DUP3 ADD DUP5 ADD SWAP1 SWAP3 MSTORE DUP2 DUP2 MSTORE SWAP3 SWAP2 DUP4 ADD DUP3 DUP3 DUP1 ISZERO PUSH2 0x364 JUMPI DUP1 PUSH1 0x1F LT PUSH2 0x339 JUMPI PUSH2 0x100 DUP1 DUP4 SLOAD DIV MUL DUP4 MSTORE SWAP2 PUSH1 0x20 ADD SWAP2 PUSH2 0x364 JUMP JUMPDEST PUSH1 0x0 PUSH2 0x622 CALLER DUP5 DUP5 PUSH2 0x3D7 JUMP JUMPDEST SWAP4 SWAP3 POP POP POP JUMP JUMPDEST PUSH1 0x4 PUSH1 0x20 SWAP1 DUP2 MSTORE PUSH1 0x0 SWAP3 DUP4 MSTORE PUSH1 0x40 DUP1 DUP5 KECCAK256 SWAP1 SWAP2 MSTORE SWAP1 DUP3 MSTORE SWAP1 KECCAK256 SLOAD DUP2 JUMP STOP LOG1 PUSH6 0x627A7A723058 KECCAK256 LT 0xd1 MSIZE 0xc3 SWAP11 0x47 ADDRESS LOG3 NUMBER 0xe0 0xe3 SUB LOG1 DUP1 0xcc GASLIMIT PUSH6 0xCFA34D41EC11 PUSH10 0x8406D16EA9B50D9A0029 "", 	""sourceMap"": ""189:38:0:-;168:1826;189:38;;168:1826;189:38;;;;;;;;;;-1:-1:-1;;189:38:0;;:::i;:::-;-1:-1:-1;234:31:0;;;;;;;;;;;;;;;;;;;;;;;;;;:::i;:::-;-1:-1:-1;272:27:0;;;-1:-1:-1;;272:27:0;297:2;272:27;;;168:1826;5:2:-1;;;;30:1;27;20:12;5:2;168:1826:0;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;-1:-1:-1;168:1826:0;;;-1:-1:-1;168:1826:0;:::i;:::-;;;:::o;:::-;;;;;;;;;;;;;;;;;;;;:::o;:::-;;;;;;;"" }";
        public WbnbDeploymentBase() : base(BYTECODE) { }
        public WbnbDeploymentBase(string byteCode) : base(byteCode) { }

    }

    public partial class NameFunction : NameFunctionBase { }

    [Function("name", "string")]
    public class NameFunctionBase : FunctionMessage
    {

    }

    public partial class ApproveFunction : ApproveFunctionBase { }

    [Function("approve", "bool")]
    public class ApproveFunctionBase : FunctionMessage
    {
        [Parameter("address", "guy", 1)]
        public virtual string Guy { get; set; }
        [Parameter("uint256", "wad", 2)]
        public virtual BigInteger Wad { get; set; }
    }

    public partial class TotalSupplyFunction : TotalSupplyFunctionBase { }

    [Function("totalSupply", "uint256")]
    public class TotalSupplyFunctionBase : FunctionMessage
    {

    }

    public partial class TransferFromFunction : TransferFromFunctionBase { }

    [Function("transferFrom", "bool")]
    public class TransferFromFunctionBase : FunctionMessage
    {
        [Parameter("address", "src", 1)]
        public virtual string Src { get; set; }
        [Parameter("address", "dst", 2)]
        public virtual string Dst { get; set; }
        [Parameter("uint256", "wad", 3)]
        public virtual BigInteger Wad { get; set; }
    }

    public partial class WithdrawFunction : WithdrawFunctionBase { }

    [Function("withdraw")]
    public class WithdrawFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "wad", 1)]
        public virtual BigInteger Wad { get; set; }
    }

    public partial class DecimalsFunction : DecimalsFunctionBase { }

    [Function("decimals", "uint8")]
    public class DecimalsFunctionBase : FunctionMessage
    {

    }

    public partial class BalanceOfFunction : BalanceOfFunctionBase { }

    [Function("balanceOf", "uint256")]
    public class BalanceOfFunctionBase : FunctionMessage
    {
        [Parameter("address", "", 1)]
        public virtual string ReturnValue1 { get; set; }
    }

    public partial class SymbolFunction : SymbolFunctionBase { }

    [Function("symbol", "string")]
    public class SymbolFunctionBase : FunctionMessage
    {

    }

    public partial class TransferFunction : TransferFunctionBase { }

    [Function("transfer", "bool")]
    public class TransferFunctionBase : FunctionMessage
    {
        [Parameter("address", "dst", 1)]
        public virtual string Dst { get; set; }
        [Parameter("uint256", "wad", 2)]
        public virtual BigInteger Wad { get; set; }
    }

    public partial class DepositFunction : DepositFunctionBase { }

    [Function("deposit")]
    public class DepositFunctionBase : FunctionMessage
    {

    }

    public partial class AllowanceFunction : AllowanceFunctionBase { }

    [Function("allowance", "uint256")]
    public class AllowanceFunctionBase : FunctionMessage
    {
        [Parameter("address", "", 1)]
        public virtual string ReturnValue1 { get; set; }
        [Parameter("address", "", 2)]
        public virtual string ReturnValue2 { get; set; }
    }

    public partial class ApprovalEventDTO : ApprovalEventDTOBase { }

    [Event("Approval")]
    public class ApprovalEventDTOBase : IEventDTO
    {
        [Parameter("address", "src", 1, true)]
        public virtual string Src { get; set; }
        [Parameter("address", "guy", 2, true)]
        public virtual string Guy { get; set; }
        [Parameter("uint256", "wad", 3, false)]
        public virtual BigInteger Wad { get; set; }
    }

    public partial class TransferEventDTO : TransferEventDTOBase { }

    [Event("Transfer")]
    public class TransferEventDTOBase : IEventDTO
    {
        [Parameter("address", "src", 1, true)]
        public virtual string Src { get; set; }
        [Parameter("address", "dst", 2, true)]
        public virtual string Dst { get; set; }
        [Parameter("uint256", "wad", 3, false)]
        public virtual BigInteger Wad { get; set; }
    }

    public partial class DepositEventDTO : DepositEventDTOBase { }

    [Event("Deposit")]
    public class DepositEventDTOBase : IEventDTO
    {
        [Parameter("address", "dst", 1, true)]
        public virtual string Dst { get; set; }
        [Parameter("uint256", "wad", 2, false)]
        public virtual BigInteger Wad { get; set; }
    }

    public partial class WithdrawalEventDTO : WithdrawalEventDTOBase { }

    [Event("Withdrawal")]
    public class WithdrawalEventDTOBase : IEventDTO
    {
        [Parameter("address", "src", 1, true)]
        public virtual string Src { get; set; }
        [Parameter("uint256", "wad", 2, false)]
        public virtual BigInteger Wad { get; set; }
    }

    public partial class NameOutputDTO : NameOutputDTOBase { }

    [FunctionOutput]
    public class NameOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("string", "", 1)]
        public virtual string ReturnValue1 { get; set; }
    }



    public partial class TotalSupplyOutputDTO : TotalSupplyOutputDTOBase { }

    [FunctionOutput]
    public class TotalSupplyOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }





    public partial class DecimalsOutputDTO : DecimalsOutputDTOBase { }

    [FunctionOutput]
    public class DecimalsOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("uint8", "", 1)]
        public virtual byte ReturnValue1 { get; set; }
    }

    public partial class BalanceOfOutputDTO : BalanceOfOutputDTOBase { }

    [FunctionOutput]
    public class BalanceOfOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public partial class SymbolOutputDTO : SymbolOutputDTOBase { }

    [FunctionOutput]
    public class SymbolOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("string", "", 1)]
        public virtual string ReturnValue1 { get; set; }
    }





    public partial class AllowanceOutputDTO : AllowanceOutputDTOBase { }

    [FunctionOutput]
    public class AllowanceOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }
}
