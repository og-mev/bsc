# bsc
## 套利策略逻辑
![image](https://github.com/og-mev/bsc/blob/master/images/mev-arbitrage.jpg)
token的reverse部分通过参考三明治逻辑的分部署数据库形式  
reverse 的数据结构为：  
```
{
    key:"tokenA:tokenB:pancake"
    value:[reverse0,reverse1]
}
```
通过DFS算法，预先将搜索出来的路径存储在cache中。  
DFS 算法参考：  
https://github.com/ccyanxyz/uniswap-arbitrage-analysis  
path cache的数据结构为：  
```
{
    key:"tokanA:tokenB"
    value:[
        [reversKey1,reverseKey2],
        [reversKey1,reverseKey3,reverseKey9],
        [reversKey5,reverseKey5,reverseKey3,reverseKey4]
        ...more
    ]
}
```

1.通过IPC监听链上TX的消息  
2.发现交易时，匹配path cache  计算最优解。  
3.离线签名并发送交易  



## 三明治逻辑
![image](https://github.com/og-mev/bsc/blob/master/images/mev-sandwitch.png?raw=true)



## 优化部分
合约优化参考：https://github.com/libevm/subway/blob/master/contracts/src/Sandwich.sol  
