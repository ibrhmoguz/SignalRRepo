using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CurrencyService.Controllers
{
    public class CurrencyController : ApiController
    {
        // GET: api/Currency
        public IEnumerable<CurrencyReport> GetAllCurrency()
        {
            using (CurrencyContext entity = new CurrencyContext())
            {
                return entity.CurrencyReport.ToList();
            }
        }

        // GET: api/Currency/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Currency
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Currency/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Currency/5
        public void Delete(int id)
        {
        }
    }
}
