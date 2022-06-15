using System;
using System.Collections.Generic;
using System.Text;
using StackExchange.Redis;
using Newtonsoft.Json;
using System.Threading;
using Tools;

namespace arbitrage_CSharp.Tools
{
    public static class RedisDB
    {
        private static ConnectionMultiplexer connection;
        private static IDatabase instance;
        private static string configStr = null;

        static RedisDB()
        {
            ThreadPool.SetMinThreads(200, 200);
        }

        public static IDatabase Instance
        {
            get
            {
                if (connection == null || !connection.IsConnected)
                {
                    if (RedisDB.configStr == null || RedisDB.configStr.Equals(string.Empty))
                        throw new Exception("请先调用 Init");
                    Init(RedisDB.configStr);
                }
                return instance;
            }
        }
        public static void Init(string configStr)
        {
            RedisDB.configStr = configStr;
            if (connection == null || !connection.IsConnected)
            {
                connection = ConnectionMultiplexer.Connect(RedisDB.configStr);
                
                instance = connection.GetDatabase();
            }
        }


        public static T StringGet<T>(this IDatabase database, RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            string res = database.StringGet(key, flags);
            Logger.Debug($"res {res}");
            if (res == null)
                return default(T);
            return JsonConvert.DeserializeObject<T>(res);
        }
        public static bool StringSet(this IDatabase database, RedisKey key, object value, TimeSpan? expiry = null, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            var val = JsonConvert.SerializeObject(value);
            return database.StringSet(key, val, expiry, when, flags);
        }

//         public static T HashScan<T>(this IDatabase database, RedisKey key, RedisValue pattern = default, int pageSize = 250, long cursor = 0, int pageOffset = 0, CommandFlags flags = CommandFlags.None)
//         {
//             var res = database.HashScan(key, pattern, pageSize,  cursor, pageOffset, flags);
//             Logger.Debug($"res {res}");
//             if (res == null)
//                 return default(T);
//             return JsonConvert.DeserializeObject<T>(res);
//         }
    }

}