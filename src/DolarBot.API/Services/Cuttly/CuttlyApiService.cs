using DolarBot.API.Models.Cuttly;
using Microsoft.Extensions.Configuration;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace DolarBot.API.Services.Cuttly
{
    [Description("https://cutt.ly/cuttly-api")]
    public class CuttlyApiService
    {
        #region Constants
        private const string ENV_API_KEY_NAME = "CUTTLY_API_KEY";
        private const int CUTTLY_STATUS_OK = 7;
        #endregion

        #region Vars
        /// <summary>
        /// An object to execute REST calls to the API.
        /// </summary>
        private readonly RestClient Client;
        /// <summary>
        /// Allows access to application settings.
        /// </summary>
        private readonly IConfiguration Configuration;
        /// <summary>
        /// Allows access to application settings.
        /// </summary>
        private readonly string ApiKey;
        #endregion

        /// <summary>
        /// Creats a <see cref="CuttlyApiService"/> object using the provided configuration.
        /// </summary>
        /// <param name="configuration">An <see cref="IConfiguration"/> object to access application settings.</param>
        internal CuttlyApiService(IConfiguration configuration)
        {
            Configuration = configuration;
            ApiKey = Configuration["cuttlyApiKey"];

            RestClientOptions options = new(Configuration["cuttlyBaseUrl"]);
            if (int.TryParse(Configuration["cuttlyRequestTimeout"], out int timeoutSeconds) && timeoutSeconds > 0)
            {
                options.MaxTimeout = Convert.ToInt32(TimeSpan.FromSeconds(timeoutSeconds).TotalMilliseconds);
            }
            Client = new RestClient(options).UseNewtonsoftJson();
        }

        /// <summary>
        /// Shortens a link using the cutt.ly API.
        /// </summary>
        /// <param name="url">The URL to shorten.</param>
        /// <returns>A shortened URL if successful, otherwise returns <paramref name="url"/>.</returns>
        public async Task<string> ShortenUrl(string url)
        {
            string apiKey = !string.IsNullOrWhiteSpace(ApiKey) ? ApiKey : Environment.GetEnvironmentVariable(ENV_API_KEY_NAME);

            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                RestRequest request = new()
                {
                    RequestFormat = DataFormat.Json,
                };
                request.AddQueryParameter("key", apiKey).AddQueryParameter("short", url);
                RestResponse<CuttlyResponse> response = await Client.ExecuteGetAsync<CuttlyResponse>(request);

                if (response.IsSuccessful)
                {
                    CuttlyResponse cuttlyResponse = response.Data;
                    return cuttlyResponse.Url.Status == CUTTLY_STATUS_OK ? cuttlyResponse.Url.ShortLink : url;
                }
                else
                {
                    return url;
                }
            }

            return url;
        }
    }
}
