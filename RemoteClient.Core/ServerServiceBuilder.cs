using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteClient.Core
{
    public sealed class ServerServiceBuilder
    {
        private static volatile ServerService uniqueInstance;

        private static readonly object locker = new object();

        private ServerServiceBuilder()
        {
        }

        public static void Init(string key, string iv)
        {
            if (uniqueInstance == null)
            {
                lock (locker)
                {
                    if (uniqueInstance == null)
                    {
                        uniqueInstance = new ServerService(key, iv);
                    }
                }
            }
        }

        public static ServerService GetInstance()
        {
            if (uniqueInstance == null)
            {
                throw new ArgumentNullException("need call Init(string key, string iv) first.");
            }
            return uniqueInstance;
        }
    }

}
