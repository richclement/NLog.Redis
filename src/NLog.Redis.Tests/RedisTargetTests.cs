﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog.Config;
using NLog.Targets;
using NUnit.Framework;
using StackExchange.Redis;

namespace NLog.Redis.Tests
{
    /// <summary>
    /// These unit tests are marked ignore since they have a requirement of having a redis server running locally.
    /// </summary>
    public class RedisTargetTests
    {
        protected const string RedisKey = "testkey";
        protected const string RedisHosts = "localhost:6379";
        protected const string RedisPassword = "testingpassword";
        protected string Password = null;

        protected bool ActionRun = false;
        public void ListenForMessage(RedisChannel channel, RedisValue value)
        {
            Assert.AreEqual(RedisKey, channel.ToString());
            Assert.IsFalse(!value.HasValue || value.IsNullOrEmpty);
            Assert.AreEqual("INFO test pub/sub message", value.ToString());
            ActionRun = true;
        }

        public void NLogRedisConfiguration(string dataType, bool usePassword = false)
        {
            // create config
            var config = new LoggingConfiguration();

            // create target
            var redisTarget = new RedisTarget();
            config.AddTarget("redis", redisTarget);

            // set target properties
            redisTarget.Layout = "${uppercase:${level}} ${message}";
            redisTarget.Hosts = RedisHosts;
            redisTarget.Key = RedisKey;
            redisTarget.Db = 0;
            redisTarget.DataType = dataType;
            if (usePassword) redisTarget.Password = RedisPassword;

            // setup rules
            var rule1 = new LoggingRule("*", LogLevel.Info, redisTarget);
            config.LoggingRules.Add(rule1);

            LogManager.Configuration = config;
        }

        public ConnectionMultiplexer GetRedisConnection(bool usePassword = false)
        {
            var connectionOptions = new ConfigurationOptions
            {
                AbortOnConnectFail = false,
                SyncTimeout = 3000,
                ConnectTimeout = 3000,
                ConnectRetry = 3,
                KeepAlive = 5
            };
            if (usePassword) connectionOptions.Password = RedisPassword;

            foreach(var host in RedisHosts.Split(','))
                connectionOptions.EndPoints.Add(host);

            return ConnectionMultiplexer.Connect(connectionOptions);
        }
    }
}
