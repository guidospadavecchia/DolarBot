using DolarBot.API.Services.Topgg.Model;
using DolarBot.Util;
using log4net;
using Microsoft.Extensions.Configuration;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace DolarBot.API.Services.Topgg
{
    [Description("https://docs.top.gg/api/bot")]
    public class TopggApiService
    {
        #region Constants
        private const string BASE_URL = "https://top.gg/api";
        private const string AUTH_HEADER = "Authorization";
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
        /// Log4net logger.
        /// </summary>
        private readonly ILog Logger;
        #endregion

        /// <summary>
        /// Creats a <see cref="TopggApiService"/> object.
        /// </summary>
        /// <param name="configuration">An <see cref="IConfiguration"/> object to access application settings.</param>
        /// <param name="logger">The log4net logger.</param>
        internal TopggApiService(IConfiguration configuration, ILog logger)
        {
            Configuration = configuration;
            Logger = logger;

            RestClientOptions options = new(BASE_URL);
            if (int.TryParse(Configuration["apiRequestTimeout"], out int timeoutSeconds) && timeoutSeconds > 0)
            {
                options.MaxTimeout = Convert.ToInt32(TimeSpan.FromSeconds(timeoutSeconds).TotalMilliseconds);
            }
            Client = new RestClient(options, configureSerialization: x => x.UseNewtonsoftJson());
            Client.AddDefaultHeader(AUTH_HEADER, GlobalConfiguration.GetDblToken(Configuration));
        }

        /// <summary>
        /// Posts the server count value to the Top.gg API.
        /// </summary>
        /// <param name="count">The amount of servers the bot is in.</param>
        /// <returns>True if posted successfully, otherwise false.</returns>
        public async Task<bool> PostServerCountAsync(int count)
        {
            bool isConfigured = ulong.TryParse(Configuration["botDiscordId"], out ulong botDiscordId);
            if (isConfigured)
            {
                string endpointUrl = $"{BASE_URL}/bots/{botDiscordId}/stats";
                RestRequest request = new(endpointUrl, Method.Post);
                request.AddBody(new PostServerCountBody()
                {
                    Count = count
                });

                RestResponse response = await Client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    return true;
                }
                else
                {
                    if (response.ErrorException != null)
                    {
                        Logger.Error(response.ErrorException);
                    }
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
