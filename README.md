# 游戏服务器

本项目使用C#语言编写，主要利用了C#的await/async语法糖，将传统的异步回调的写法改为异步同步的写法。

服务主要有两个线程，网络线程和逻辑线程，线程间通过无锁队列通信。

项目列表：

1. **Frame**

   服务器的框架，包含服务器的通用基础类：

   ① LockFreeQueue 无锁队列

   ② Dispatcher 协议派发类

   ③ FSM 有限状态机

   ④ NetworkMngr 网络管理类

   ⑤ SingleThreadSynchronizationContext C#的异步调度上下文

   ⑥ ServerApp 服务器类的基类，实现新服务器只需要包含此类即可

2. **DataBase**

   数据库访问api, 数据库使用的是mysql, 实现了一个K-V模式。

   并发冲突使用乐观锁：每条记录都有版本号字段，不能直接更新数据，必须先取出原数据然后再修改数据，修改时需要和数据库中版本号对比，如果相同则允许修改，如果不同则进制修改。修改成功后记录的版本号+1。

   数据库操作的函数均以异步任务实现。

   <u>尚未完善:</u>:  

   ​	① 数据的二进制需要分段，目前整个二进制都会传输，导致数据变大时数据库操作效率不高。

   ​	② 自动建表

   ​	③ 做专门数据库服务进行缓存，计划是一个GameServer对应一个数据库缓存Server

3. **GameServer**

   游戏主逻辑的服务器，客户端将直连此服务。

   已完成功能：登录，创角
   
4. **DirServer **(未完成)

   目录服务器，所有服务将把自己的服务器类型, 服务器大区(Zone), 服务器id以及ip和端口号注册到此服务。

5. **DirServerApi** (未完成)

   调用目录服务器的接口

6. **ClientTest**

   模拟客户端发协议，来测试GameServer接口

7. **UnitTest**

   单元测试



# 规划功能

1. ProxyServer：所有服务间通信通过此服务转发
2. CacheServer：mysql数据库缓存服务，目前各个服务通过DataBase库直接访问mysql，以后需要做数据缓存

