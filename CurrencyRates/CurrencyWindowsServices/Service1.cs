using DAL;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Transports;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;

namespace CurrencyWindowsServices
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
            //GetCurrencyData();
        }
        public void OnDebug()
        {
            OnStart(null);
        }
        protected override void OnStart(string[] args)
        {
            CurrencyTimer.Start();
        }

        protected override void OnStop()
        {
        }

        private Timer currencyTimer;

        private Timer CurrencyTimer
        {
            get
            {
                if (currencyTimer == null)
                {
                    currencyTimer = new Timer();
                    currencyTimer.Interval = 60000;
                    currencyTimer.Enabled = true;
                    currencyTimer.Elapsed += CurrencyTimer_Elapsed;
                }

                return currencyTimer;
            }
            set { currencyTimer = value; }
        }

        public async Task GetCurrencyData()
        {
            try
            {
                //We takes all currencyData from service
                Uri uri = new Uri("http://www.tcmb.gov.tr/kurlar/today.xml");

                string xmlStr;
                using (var wc = new WebClient())
                {
                    xmlStr = await wc.DownloadStringTaskAsync(uri);
                }
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlStr);
                //I Could make here with Xpath. But this is more easy..
                DataSet ds = new DataSet();
                ds.ReadXml(new XmlNodeReader(xmlDoc));
                //We took all data---------------------------

                //We will check is there any data on DB
                bool anyDataOnDb = false;
                using (CurrencyContext entity = new CurrencyContext())
                {
                    anyDataOnDb = entity.CurrencyReport.Any();

                    //If any data on db we will check for it is Updated
                    if (anyDataOnDb)
                    {
                        if (cacheDatas == null) { FillCache(); }
                        changeDatas.Clear();
                        foreach (DataRow drow in ds.Tables[1].Rows)
                        {
                            if (drow["Isim"].ToString().Trim() != "")
                            {
                                CurrencyReport report = new CurrencyReport();

                                report.CurrencyName = drow["Isim"].ToString();
                                report.ForexBuying = Decimal.Parse(drow["ForexBuying"].ToString() != "" ? drow["ForexBuying"].ToString().Replace(".", ",") : "0");
                                report.ForexSelling = Decimal.Parse(drow["ForexSelling"].ToString() != "" ? drow["ForexSelling"].ToString().Replace(".", ",") : "0");
                                report.BanknoteBuying = Decimal.Parse(drow["BanknoteBuying"].ToString() != "" ? drow["BanknoteBuying"].ToString().Replace(".", ",") : "0");
                                report.BanknoteSelling = Decimal.Parse(drow["BanknoteSelling"].ToString() != "" ? drow["BanknoteSelling"].ToString().Replace(".", ",") : "0");
                                report.CrossRateUSD = Decimal.Parse(drow["CrossRateUSD"].ToString() != "" ? drow["CrossRateUSD"].ToString().Replace(".", ",") : "0");
                                if (!isDataChange(report))
                                {
                                    changeDatas.Add(report);
                                }
                            }
                        }
                        foreach (CurrencyReport report in changeDatas)
                        {
                            var changedData = entity.CurrencyReport.Find(report.CurrencyName);
                            changedData.ForexBuying = report.ForexBuying;
                            changedData.ForexSelling = report.ForexSelling;
                            changedData.BanknoteBuying = report.BanknoteBuying;
                            changedData.BanknoteSelling = report.BanknoteSelling;
                            changedData.CrossRateUSD = report.CrossRateUSD;
                            changedData.CreatedDate = DateTime.Now;
                        }
                        if (changeDatas.Any())
                        {
                            entity.SaveChanges();
                            //Triger All Clients By SignalR
                            HubConnection hubConnection = new HubConnection("http://localhost:1646");
                            IHubProxy hubProxy = hubConnection.CreateHubProxy("Currency");
                            await hubConnection.Start(new LongPollingTransport());
                            hubProxy.Invoke("UpdateData", changeDatas);
                            //-----------------
                            FillCache();                           
                        }
                    }
                    else
                    {
                        List<CurrencyReport> NewDatas = new List<CurrencyReport>();
                        //If there is no data on DB we will insert all Datas.........
                        foreach (DataRow drow in ds.Tables[1].Rows)
                        {
                            if (drow["Isim"].ToString().Trim() != "")
                            {
                                CurrencyReport report = new CurrencyReport();
                                report.CurrencyName = drow["Isim"].ToString();
                                report.ForexBuying = Decimal.Parse(drow["ForexBuying"].ToString() != "" ? drow["ForexBuying"].ToString().Replace(".", ",") : "0");
                                report.ForexSelling = Decimal.Parse(drow["ForexSelling"].ToString() != "" ? drow["ForexSelling"].ToString().Replace(".", ",") : "0");
                                report.BanknoteBuying = Decimal.Parse(drow["BanknoteBuying"].ToString() != "" ? drow["BanknoteBuying"].ToString().Replace(".", ",") : "0");
                                report.BanknoteSelling = Decimal.Parse(drow["BanknoteSelling"].ToString() != "" ? drow["BanknoteSelling"].ToString().Replace(".", ",") : "0");
                                report.CrossRateUSD = Decimal.Parse(drow["CrossRateUSD"].ToString() != "" ? drow["CrossRateUSD"].ToString().Replace(".", ",") : "0");
                                report.CreatedDate = DateTime.Now;
                                entity.CurrencyReport.Add(report);
                                NewDatas.Add(report);
                            }
                        }
                        entity.SaveChanges();
                        //Triger All Clients By SignalR [Show All New Datas to All Client]
                        HubConnection hubConnection = new HubConnection("http://localhost:1646");
                        IHubProxy hubProxy = hubConnection.CreateHubProxy("Currency");
                        await hubConnection.Start(new LongPollingTransport());
                        hubProxy.Invoke("FillData", NewDatas);
                        //-----------------
                    }
                }
            }
            catch (Exception ex)
            {
                int i = 0;
            }
        }
        /// <summary>
        /// We collect chaned data to this changeDatas list. After all we will update them.
        /// </summary>
        public List<CurrencyReport> changeDatas;
        /// <summary>
        /// We will check all data's value from this cache. And if there is anyChanges we will refresh it.
        /// </summary>
        public List<CurrencyReport> cacheDatas;
        public void FillCache()
        {
            changeDatas = new List<CurrencyReport>();
            if (cacheDatas == null)
            {
                cacheDatas = new List<CurrencyReport>();
            }
            else
            {
                cacheDatas.Clear();
            }
            using (CurrencyContext entity = new CurrencyContext())
            {
                cacheDatas = entity.CurrencyReport.ToList();
            }
        }

        /// <summary>
        /// We checked here is the current Currency Data is change or not
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool isDataChange(CurrencyReport data)
        {
            return cacheDatas.Any(cd => cd.CurrencyName == data.CurrencyName && cd.BanknoteBuying == data.BanknoteBuying && cd.BanknoteSelling == data.BanknoteSelling && cd.ForexBuying == data.ForexBuying && cd.ForexSelling == data.ForexSelling);
        }

        private async void CurrencyTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            CurrencyTimer.Stop();
            try
            {
                await GetCurrencyData();
            }
            catch (Exception ex)
            {

            }
            CurrencyTimer.Start();
        }
    }
}
