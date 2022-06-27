
单个 dex逻辑

1 拉取redis 交易对数据,生成每个token的所有兑换路径,和每个token 的可以兑换tokens

2 监听tx

3 找到tx其中是 swap的方法

4 计算出全部有变化的 tokens

5 把变化的token 数量加到池子里面

6 计算上面有变化的tokens的所有路径是否有利润

7 把有利润的 tokens 签名发送


多个dex


1 拉取redis 交易对数据,生成每个token的所有兑换路径,和每个token 的可兑换tokens
	1.1 重写 单个交易所 字典为多个交易所字典
	1.2 修改计算路径时候的方法
		1.2.1 计算路径时候 记录全部的相同路径（eg: A-B biswap  和A-B pancakeswap）

2 监听tx

3 找到tx其中是 swap的方法

4 计算出全部有变化的 tokens

5 计算上面有变化的tokens的所有路径是否有利润
	5.1根据不同交易所相同路径来替换，获得所有路径（eg：A-B-C  ->  A-B(Pswap or BSwap)->B-C(Pswap or BSwap))）
		满足组合的算法
6 把有利润的 tokens 前面发送