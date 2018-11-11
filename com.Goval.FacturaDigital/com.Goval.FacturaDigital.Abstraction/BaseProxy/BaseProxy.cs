using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace com.Goval.FacturaDigital.Abstraction.BaseProxy
{
    public abstract class BaseProxy<Request, Response> 
    {
        const string MethodType = "POST";
        const string ContentType = @"application/json; charset=utf-8";
        //const string BaseDomainURL = "http://facturagovalservices.azurewebsites.net/";
        //const string BaseDomainURL ="http://fgomezvalverde-001-site1.dtempurl.com/";
        const string BaseDomainURL = "http://192.168.0.4:8081";
        public abstract string OperationoAddress
        { get; }

        private WebRequest CreateClient(string pBaseUrl)
        {
            //HttpClient vClient;
            WebRequest vClient = WebRequest.Create(pBaseUrl + OperationoAddress);

            vClient.Method = MethodType;
            vClient.ContentType = ContentType;

            return vClient;
        }

        public async Task<Response> GetDataAsync(Request pRequest, string pBaseUrl = BaseDomainURL)
        {
            try
            {
                var vClient = (HttpWebRequest) CreateClient(pBaseUrl);
                JsonSerializerSettings microsoftDateFormatSettings = new JsonSerializerSettings
                {
                    DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
                };
                var vJsonRequest = JsonConvert.SerializeObject(pRequest, microsoftDateFormatSettings);
                var data = new UTF8Encoding().GetBytes(vJsonRequest);
                using (var stream = await vClient.GetRequestStreamAsync())
                {
                    stream.Write(data, 0, data.Length);
                }

                var response = await vClient.GetResponseAsync();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    string pStringResult = reader.ReadToEnd();
                    return JsonConvert.DeserializeObject<Response>(pStringResult);
                }
            }
            catch (WebException ex)
            {
                string vError = ex.ToString();
                if (ex.Response != null)
                {
                    var vStream = ex.Response.GetResponseStream();
                    if (vStream != null)
                    {
                        vError = new StreamReader(vStream).ReadToEnd();
                    }
                }
                Debug.WriteLine("BaseProxy.GetDataAsync.WebException:" + vError);
                return default(Response);

            }
            catch (Exception ex)
            {
                Debug.WriteLine("BaseProxy.GetDataAsync.Exception:"+ex.ToString());
                return default(Response);
            }



        }

        private byte[] SerializeParameter(Request pObject)
        {
            JsonSerializerSettings vMicrosoftDateTime = new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
            };
            var vStringJson = JsonConvert.SerializeObject(pObject, vMicrosoftDateTime);
            return System.Text.Encoding.UTF8.GetBytes(vStringJson);
        }
    }
}
