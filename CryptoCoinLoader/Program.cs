using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;
using System.IO;
using System.Net;
using System.Configuration;
using ADOW;

namespace CryptoCoinLoader
{
    class Program
    {
        static void Main(string[] args)
        {
            args = new string[1];
            args[0] = "1";

            if (args.Length == 1) {
                MarketCoinLoader mcl = new MarketCoinLoader();
                mcl.LoadData();
            }

            //if (args.Length > 0) {
            //    CoinCaIoLoder mcl = new CoinCaIoLoder();
            //    mcl.LoadData();
            //}
        }
    }
}
