using DolarBot.API.Models;
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
            private readonly RestClient client;
            private readonly IConfiguration configuration;
            private readonly Action<IRestResponse> OnError;

            public DolarArgentinaApi(IConfiguration configuration, Action<IRestResponse> onError)
            {
                this.configuration = configuration;
                OnError = onError;

                client = new RestClient(this.configuration["apiUrl"]);
                client.UseNewtonsoftJson();
            }

            public async Task<DolarResponse> GetDolarOficial()
            {
                var request = new RestRequest("/api/dolaroficial", DataFormat.Json);
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
        }
    }
}
