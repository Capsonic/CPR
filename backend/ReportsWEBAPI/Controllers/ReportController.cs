using ReportsMain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace Reports_WEB_API.Controllers
{
    [Authorize]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class ReportController : ApiController
    {
        protected bool isValidJSValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value == "null" || value == "undefined")
            {
                return false;
            }

            return true;
        }

        protected bool isValidParam(string param)
        {
            //reserved and invalid params:
            if (new string[] {
                "perPage",
                "page",
                "filterGeneral",
                "itemsCount",
                "noCache",
                "totalItems",
                "parentKey",
                "parentField"
            }.Contains(param))
                return false;

            return true;
        }

        [HttpGet, Route("api/Report/GrossMarginFutureByFiscalPeriod/{userName}")]
        public HttpResponseMessage GrossMarginFutureByFiscalPeriod(string userName)
        {
            GrossMarginFutureByFiscalPeriod report = new GrossMarginFutureByFiscalPeriod(userName);

            HttpResponseMessage result = null;
            try
            {

                string paramDateFrom = HttpContext.Current.Request["date_from"];
                if (isValidJSValue(paramDateFrom))
                {
                    report.paramDateFrom = new DateTime(1970, 1, 1).AddTicks(long.Parse(paramDateFrom) * 10000);
                    report.paramDateFrom = report.paramDateFrom.AddHours(-6); //Standarized to 12:00 AM
                }

                result = Request.CreateResponse(HttpStatusCode.OK);
                result.Content = new StreamContent(new MemoryStream(report.generate()));
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                result.Content.Headers.ContentDisposition.FileName = "GrossMarginFutureByFiscalPeriod.xlsx";
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                return result;
            }
            catch (Exception ex)
            {
                HttpError err = new HttpError("Helper: " + report.DebugErrorHelper + " " + ex.ToString());
                return Request.CreateResponse(HttpStatusCode.InternalServerError, err);
            }
        }
    }
}
