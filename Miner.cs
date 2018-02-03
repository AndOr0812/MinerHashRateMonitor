using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace MinerHashRateMonitor
{
    [Serializable]
    public class Miner
    {
        public string Name { get; set; }
        public MinerType Type { get; set; }
        public string Ip { get; set; }
        public int HashRate { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public bool IsOnline
        {
            get
            {
                bool status = false;
                PingReply pingReply = (new Ping()).Send(Ip);

                if (pingReply.Status == IPStatus.Success)
                {
                    status = true;
                }
                return status;
            }
        }
    }

    public enum MinerType
    {
        Bitcoin = 0,
        Litecoin = 1,
        Dash = 2
    }
}
