using System;
using StackExchange.Redis;

namespace Test.MainTest.Redis
{
    public sealed class RedisConnectOperation
    {
        private static readonly Lazy<ConnectionMultiplexer> lazyConnection = new(() =>
        {
            DotNetEnv.Env.TraversePath().Load();
            return ConnectionMultiplexer.Connect(DotNetEnv.Env.GetString("RedisConnection")); 
        });

        public static ConnectionMultiplexer Connection
        {
            get 
            {
                return lazyConnection.Value;
            }
        }
    }
}