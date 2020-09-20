using DolarBot.API.Cache;
using DolarBot.API.Models;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using log4net;
using Microsoft.Extensions.Configuration;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;

namespace DolarBot.API
{
    public sealed class ApiCalls
    {
        private readonly ILog logger;
        private readonly ResponseCache cache;

        #region Apis
        public DolarArgentinaApi DolarArgentina { get; private set; }
        #endregion

        public ApiCalls(IConfiguration configuration, ILog logger)
        {
            this.logger = logger;
            cache = new ResponseCache(configuration);
            DolarArgentina = new DolarArgentinaApi(configuration, cache, LogError);
        }

        private void LogError(IRestResponse response)
        {
            if (response.ErrorException != null)
            {
                logger.Error($"API error. Endpoint returned {response.StatusCode}: {response.StatusDescription}", response.ErrorException);
            }
            else
            {
                logger.Error($"API error. Endpoint returned {response.StatusCode}: {response.StatusDescription}");
            }
        }

        [Description("https://github.com/Castrogiovanni20/api-dolar-argentina")]
        public class DolarArgentinaApi
        {
            private const string DOLAR_OFICIAL_ENDPOINT = "/api/dolaroficial";
            private const string DOLAR_BLUE_ENDPOINT = "/api/dolarblue";
            private const string DOLAR_CONTADO_LIQUI_ENDPOINT = "/api/contadoliqui";
            private const string DOLAR_PROMEDIO_ENDPOINT = "/api/dolarpromedio";
            private const string DOLAR_BOLSA_ENDPOINT = "/api/dolarbolsa";
            private const string RIESGO_PAIS_ENDPOINT = "/api/riesgopais";
            private const string RIESGO_PAIS_CACHE_KEY = "RiesgoPais";

            private readonly RestClient client;
            private readonly IConfiguration configuration;
            private readonly ResponseCache cache;
            private readonly Action<IRestResponse> OnError;

            public enum DollarType
            {
                [Description(DOLAR_OFICIAL_ENDPOINT)]
                Oficial,
                [Description(DOLAR_OFICIAL_ENDPOINT)]
                Ahorro,
                [Description(DOLAR_BLUE_ENDPOINT)]
                Blue,
                [Description(DOLAR_CONTADO_LIQUI_ENDPOINT)]
                ContadoConLiqui,
                [Description(DOLAR_PROMEDIO_ENDPOINT)]
                Promedio,
                [Description(DOLAR_BOLSA_ENDPOINT)]
                Bolsa
            }

            public DolarArgentinaApi(IConfiguration configuration, ResponseCache cache, Action<IRestResponse> onError)
            {
                this.configuration = configuration;
                this.cache = cache;
                OnError = onError;

                client = new RestClient(this.configuration["apiUrl"]);
                client.UseNewtonsoftJson();
            }

            public CultureInfo GetApiCulture() => CultureInfo.GetCultureInfo("en-US");

            public async Task<DolarResponse> GetDollarPrice(DollarType type)
            {
                DolarResponse cachedResponse = cache.GetObject<DolarResponse>(type);
                if (cachedResponse != null)
                {
                    return cachedResponse;
                }
                else
                {
                    string endpoint = type.GetDescription();

                    RestRequest request = new RestRequest(endpoint, DataFormat.Json);
                    IRestResponse<DolarResponse> response = await client.ExecuteGetAsync<DolarResponse>(request);
                    if (response.IsSuccessful)
                    {
                        DolarResponse dolarResponse = response.Data;
                        dolarResponse.Type = type;
                        if (type == DollarType.Ahorro)
                        {
                            CultureInfo apiCulture = GetApiCulture();
                            decimal taxPercent = (decimal.Parse(configuration["dollarTaxPercent"]) / 100) + 1;
                            if (decimal.TryParse(dolarResponse.Venta, NumberStyles.Any, apiCulture, out decimal venta))
                            {
                                dolarResponse.Venta = Convert.ToDecimal(venta * taxPercent, apiCulture).ToString("F", apiCulture);
                            }
                        }

                        cache.SaveObject(type, dolarResponse);
                        return response.Data;
                    }
                    else
                    {
                        OnError(response);
                        return null;
                    }
                }
            }

            public async Task<RiesgoPaisResponse> GetRiesgoPais()
            {
                RiesgoPaisResponse cachedResponse = cache.GetObject<RiesgoPaisResponse>(RIESGO_PAIS_CACHE_KEY);
                if (cachedResponse != null)
                {
                    return cachedResponse;
                }
                else
                {
                    RestRequest request = new RestRequest(RIESGO_PAIS_ENDPOINT, DataFormat.Json);
                    IRestResponse<RiesgoPaisResponse> response = await client.ExecuteGetAsync<RiesgoPaisResponse>(request);
                    if (response.IsSuccessful)
                    {
                        cache.SaveObject(RIESGO_PAIS_CACHE_KEY, response.Data);
                        return response.Data;
                    }
                    else
                    {
                        OnError(response);
                        return null;
                    }
                }
            }
        }
    }
}
