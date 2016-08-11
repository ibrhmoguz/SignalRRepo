using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using DAL;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Caching;

namespace CurrencyRates.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public async Task<ActionResult> JqueryCurrency()
        {
            //Add 30 seconds cache for performance
            if (HttpContext.Cache["CurrencyData"] == null)
            {
                using (var client = new HttpClient())
                {
                    List<CurrencyReport> currencyData = null;
                    client.BaseAddress = new Uri(ConfigurationManager.AppSettings["baseApiAddress"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await client.GetAsync("api/Currency");
                    if (response.IsSuccessStatusCode)
                    {
                        currencyData = await response.Content.ReadAsAsync<List<CurrencyReport>>();
                        HttpContext.Cache.Insert("CurrencyData", currencyData, null, DateTime.UtcNow.AddSeconds(30), System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.High, null);
                    }
                    return View(currencyData);
                }
            }
            else
            {
                var data = (List<CurrencyReport>)HttpContext.Cache["CurrencyData"];
                return View(data);
            }     
        }
        public ActionResult SignalrCurrency()
        {
            return View();
        }
        //Url'den erişilememesi için yapılmıştır..
        
        public async Task<PartialViewResult> CurrencyList()
        {
            //Add 30 seconds cache for performance
            if (HttpContext.Cache["CurrencyData"] == null)
            {
                using (var client = new HttpClient())
                {
                    List<CurrencyReport> currencyData = null;
                    client.BaseAddress = new Uri(ConfigurationManager.AppSettings["baseApiAddress"]);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await client.GetAsync("api/Currency");
                    if (response.IsSuccessStatusCode)
                    {
                        currencyData = await response.Content.ReadAsAsync<List<CurrencyReport>>();
                        HttpContext.Cache.Insert("CurrencyData", currencyData, null, DateTime.UtcNow.AddSeconds(30), System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.High, null);
                    }
                    return PartialView(currencyData);
                }
            }
            else
            {
                var data = (List<CurrencyReport>)HttpContext.Cache["CurrencyData"];
                return PartialView(data);
            }
        }
    }
    public class Currency : Hub
    {
        public override async Task OnConnected()
        {
            using (var client = new HttpClient())
            {
                List<CurrencyReport> currencyData = null;
                client.BaseAddress = new Uri(ConfigurationManager.AppSettings["baseApiAddress"]);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync("api/Currency");
                if (response.IsSuccessStatusCode)
                {
                    currencyData = await response.Content.ReadAsAsync<List<CurrencyReport>>();
                }
                Clients.Caller.getAllData(currencyData);
            }
        }
        public async Task UpdateData(List<CurrencyReport> datas)
        {
            await Clients.All.UpdateData(datas);
        }
        public async Task FillData(List<CurrencyReport> datas)
        {
            await Clients.All.getAllData(datas);
        }

    }
}