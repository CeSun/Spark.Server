# 游戏服务器

本项目是基于dotnet5平台开发的跨平台的游戏服务器。

目前包含三个项目:

1. Frame

   服务器的框架, 定义了服务器的基础类，以及状态机，协议派发器等通用类。

2. DataBase

   数据库访问api, 数据库使用的是mysql, 实现了一个K-V模式, 并发冲突使用乐观锁(version控制)

   数据库操作的函数均以异步任务实现

   待办:  做专门数据库服务进行缓存

3. GameServer

   游戏主逻辑的服务器，客户端将直连此服务。

   已完成功能：登录，创角
   
4. DirServer (未完成)

   目录服务器，所有服务将把自己的服务器类型, 服务器大区(Zone), 服务器id以及ip和端口号注册到此服务。

5. DirServerApi (未完成)

   调用目录服务器的接口

6. ClientTest

   模拟客户端发协议，来测试GameServer接口

7. UnitTest

   单元测试



# 规划功能

1. ProxyServer：所有服务间通信通过此服务转发
2. CacheServer：mysql数据库缓存服务，目前各个服务通过DataBase库直接访问mysql，以后需要做数据缓存

