using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;
using System.IO;
using System.Net;
using System.Configuration;
using ADOW;
using System;
using System.Collections;

namespace CryptoCoinLoader
{
    class MarketCoinLoader
    {
        public void LoadData() {
            string strDataFile = AppDomain.CurrentDomain.BaseDirectory + "coincap.dat";
            string strLogFile = AppDomain.CurrentDomain.BaseDirectory + "coincap.log";
            string DL = "|";
            //** Get IBM Date
            CommUtil cm = new CommUtil();
            string strDt = cm.GetDateInt();
            string strTimeHR = DateTime.Now.TimeOfDay.Hours.ToString();
            string strTimeMN = DateTime.Now.TimeOfDay.Minutes.ToString();
            if (strTimeHR.Equals("0")) strTimeHR = "";
            if (Int16.Parse(strTimeMN) < 10) strTimeMN = "0" + strTimeMN;
            string strTm = strTimeHR + strTimeMN;

            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            //DateTime dtDateTime = new DateTime(2017, 5, 6, 0, 0, 0, 0, System.DateTimeKind.Utc);
            DateTime dtDateTimeAdj = dtDateTime.AddSeconds(1500176348).ToLocalTime();
            string strUpdt = GetStandardDt(dtDateTime);

            //** Get Json data
            //string input = "https://api.coinmarketcap.com/v1/ticker/";
            string input = "https://api.coinmarketcap.com/v1/ticker/?limit=2000";
            WebClient client = new WebClient();
            string jsonInput = client.DownloadString(input);
            Dictionary<string, string>[] ccurData = JsonConvert.DeserializeObject<Dictionary<string, string>[]>(jsonInput);

            StringBuilder sb = new StringBuilder();
            string strLine = "";
            string strElem = "";
            //int intUTCSec = 0;

            for (int i = 0; i < ccurData.Length; i++)
            {
                strLine = ccurData[i]["rank"] + DL + strDt + DL + strTm + DL + ccurData[i]["id"] + DL + ccurData[i]["symbol"] + DL;

                strElem = ccurData[i]["price_usd"];
                if (strElem == null) strElem = "0";
                strLine += strElem + DL;

                strElem = ccurData[i]["price_btc"];
                if (strElem == null) strElem = "0";
                strLine += strElem + DL;

                strElem = ccurData[i]["market_cap_usd"];
                if (strElem == null) strElem = "0";
                else strElem = GetFormattedElem(strElem);
                strLine += strElem + DL;

                strElem = ccurData[i]["available_supply"];
                if (strElem == null) strElem = "0";
                else strElem = GetFormattedElem(strElem);
                strLine += strElem + DL;

                strElem = ccurData[i]["24h_volume_usd"];
                if (strElem == null) strElem = "0";
                else strElem = GetFormattedElem(strElem);

                strLine += strElem + "\n";

                //strElem = ccurData[i]["total_supply"];
                //if (strElem == null) strElem = "0";
                //else strElem = GetFormattedElem(strElem);
                //strLine += strElem + DL;

                //strElem = ccurData[i]["percent_change_1h"];
                //if (strElem == null) strElem = "0";
                //strLine += strElem + DL;

                //strElem = ccurData[i]["percent_change_24h"];
                //if (strElem == null) strElem = "0";
                //strLine += strElem + DL;

                //strElem = ccurData[i]["percent_change_7d"];
                //if (strElem == null) strElem = "0";
                //strLine += strElem + DL;

                //intUTCSec = Int32.Parse(ccurData[i]["last_updated"]);
                //dtDateTimeAdj = dtDateTime.AddSeconds(intUTCSec).ToLocalTime();
                //strUpdt = GetStandardDt(dtDateTimeAdj);

                //strLine += strUpdt + "\n";
                sb.Append(strLine);

            }

            StreamWriter st = new StreamWriter(strLogFile, true);
            st.WriteLine("***************  COINCAP-MARKET LOG " + strDt + " at " + strTm + " ***************************");

            try { File.WriteAllText(strDataFile, sb.ToString()); }
            catch (Exception ex)
            {
                st.WriteLine(ex.Message);
                st.Close();
                return;
            }


            SqlClientBuilder sq = new SqlClientBuilder(ConfigurationManager.AppSettings["conn"], false);
            if (sq.ErrNum > 0)
            {
                st.WriteLine(sq.ErrMes);
                return;
            }

            Hashtable htParms = new Hashtable();
            htParms.Add("@table", "coincaplist");
            htParms.Add("@filepath", strDataFile);
            htParms.Add("@fieldbreak", DL);
            htParms.Add("@rowbreak", "\n");
            htParms.Add("@truncate", 0);
            sq.SetData("load_table", htParms, false);
            if (sq.ErrNum > 0)
            {
                st.WriteLine(sq.ErrMes);
                st.Close();
                return;
            }

            //** Add the coins that do not exist in info table yet 
            sq.SetData("refresh_coincapinfo", true);
            if (sq.ErrNum > 0)
            {
                st.WriteLine(sq.ErrMes);
                st.Close();
                return;
            }

            st.WriteLine("Sucsess - creating file and loading to database with " + ccurData.Length.ToString() + " records");
            st.Close();
        }

        static string GetFormattedElem(string elem)
        {
            string strEl = elem;
            int intPos = strEl.IndexOf(".");
            if (intPos > 0) strEl = strEl.Substring(0, intPos);
            return strEl;
        }

        static string GetStandardDt(DateTime dt)
        {
            string strYear = dt.Year.ToString();
            string strMonth = "";
            int intMonth = dt.Month;
            if (intMonth < 10) strMonth = "0" + intMonth.ToString();
            else strMonth = intMonth.ToString();
            string strDay = "";
            int intDay = dt.Day;
            if (intDay < 10) strDay = "0" + intDay.ToString();
            else strDay = intDay.ToString();
            string strRetDate = strYear + "-" + strMonth + "-" + strDay;
            return strRetDate;
        }
    }
}
