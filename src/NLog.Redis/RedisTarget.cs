using System;
using NLog.Config;
using System.Linq;

namespace NLog.Targets
{
    [Target("Redis")]
    public class RedisTarget : TargetWithLayout
    {
        protected const string ListDataType = "list";
        protected const string ChannelDataType = "channel";

        /// <summary>
        /// Sets the host name or IP Address of the redis server
        /// </summary>
        [Obsolete("Use hosts instead")]
        public string Host { get; set; }

        /// <summary>
        /// Sets the port number redis is running on
        /// </summary>
        [Obsolete("Use hosts instead")]
        public int Port { get; set; }

        /// <summary>
        /// Sets the hosts names or IP Addresses and ports of the redis servers
        /// </summary>
        public string Hosts { get; set; }
        
        /// <summary>
        /// Sets the key to be used for either the list or the pub/sub channel in redis
        /// </summary>
        [RequiredParameter]
        public string Key { get; set; }

        /// <summary>
        /// Sets what redis data type to use, either "list" or "channel"
        /// </summary>
        [RequiredParameter]
        public string DataType { get; set; }

        /// <summary>
        /// Sets the database id to be used in redis if the log entries are sent to a list. Defaults to 0
        /// </summary>
        public int Db { get; set; }
        
        /// <summary>
        /// Sets the password to be used when accessing Redis with authentication required
        /// </summary>
        public string Password { get; set; }

        private RedisConnectionManager _redisConnectionManager;

        public RedisTarget()
        {
        }

        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            if (!string.IsNullOrWhiteSpace(Host))
                _redisConnectionManager = new RedisConnectionManager(Host, Port, Db, Password);
            else if (!string.IsNullOrWhiteSpace(Hosts))
                _redisConnectionManager = new RedisConnectionManager(Hosts.Split(',').ToList(), Db, Password);
            else
                throw new ArgumentException("At least a host must be set");
        }
        
        protected override void CloseTarget()
        {
            if (_redisConnectionManager != null)
            {
                _redisConnectionManager.Dispose();    
            }
            
            base.CloseTarget();
        }

        protected override void Write(LogEventInfo logEvent)
        {
            var message = this.Layout.Render(logEvent);
            var redisDatabase = _redisConnectionManager.GetDatabase();
            switch (DataType.ToLower())
            {
                case ListDataType:
                    redisDatabase.ListRightPush(Key, message);
                    break;
                case ChannelDataType:
                    redisDatabase.Publish(Key, message);
                    break;
                default:
                    throw new Exception("no data type defined for redis");
            }
        }

        protected override void Write(Common.AsyncLogEventInfo logEvent)
        {
            var message = this.Layout.Render(logEvent.LogEvent);
            var redisDatabase = _redisConnectionManager.GetDatabase();
            switch (DataType.ToLower())
            {
                case ListDataType:
                    redisDatabase.ListRightPushAsync(Key, message);
                    break;
                case ChannelDataType:
                    redisDatabase.PublishAsync(Key, message);
                    break;
                default:
                    throw new Exception("no data type defined for redis");
            }
        }

    }
}
