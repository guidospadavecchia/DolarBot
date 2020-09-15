using DolarBot.API.Models;
using DolarBot.Util.Extensions;
using log4net;
using Microsoft.Extensions.Configuration;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace DolarBot.API
{
    public sealed class ApiCalls
    {
        private readonly ILog logger;

        #region Apis
        public DolarArgentinaApi DolarArgentina { get; private set; }
        #endregion

        public ApiCalls(IConfiguration configuration, ILog logger)
        {
            this.logger = logger;
            DolarArgentina = new DolarArgentinaApi(configuration, LogError);
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


            private readonly RestClient client;
            private readonly IConfiguration configuration;
            private readonly Action<IRestResponse> OnError;

            public enum DollarType
            {
                [Description(DOLAR_OFICIAL_ENDPOINT)]
                Oficial,
                [Description(DOLAR_BLUE_ENDPOINT)]
                Blue,
                [Description(DOLAR_CONTADO_LIQUI_ENDPOINT)]
                ContadoConLiqui,
                [Description(DOLAR_PROMEDIO_ENDPOINT)]
                Promedio,
                [Description(DOLAR_BOLSA_ENDPOINT)]
                Bolsa
            }

            public DolarArgentinaApi(IConfiguration configuration, Action<IRestResponse> onError)
            {
                this.configuration = configuration;
                OnError = onError;

                client = new RestClient(this.configuration["apiUrl"]);
                client.UseNewtonsoftJson();
            }

            public async Task<DolarResponse> GetDollarPrice(DollarType type)
            {
                string endpoint = type.GetDescription();

                var request = new RestRequest(endpoint, DataFormat.Json);
                var response = await client.ExecuteGetAsync<DolarResponse>(request);
                if (response.IsSuccessful)
                {
                    return response.Data;
                }
                else
                {
                    OnError(response);
                    return null;
                }
            }

            public async Task<RiesgoPaisResponse> GetRiesgoPais()
            {
                var request = new RestRequest(RIESGO_PAIS_ENDPOINT, DataFormat.Json);
                var response = await client.ExecuteGetAsync<RiesgoPaisResponse>(request);
                if (response.IsSuccessful)
                {
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
