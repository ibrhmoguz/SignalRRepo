using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using System.Data;
using System.Xml;
using DAL;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Transports;

namespace WorkerRole1
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        public override void Run()
        {
            Trace.TraceInformation("WorkerRole1 is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("WorkerRole1 has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("WorkerRole1 is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("WorkerRole1 has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");
                await GetCurrencyData();
                await Task.Delay(60000);
            }
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
    }
}
