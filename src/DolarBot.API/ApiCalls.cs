using DolarBot.API.Cache;
using DolarBot.API.Services.Cuttly;
using DolarBot.API.Services.DolarBotApi;
using DolarBot.API.Services.Topgg;
using log4net;
using Microsoft.Extensions.Configuration;
using RestSharp;

namespace DolarBot.API
{
    /// <summary>
    /// This class centralizes all API calls in a single entity.
    /// </summary>
    public sealed class ApiCalls
    {
        /// <summary>
        /// Log4net logger.
        /// </summary>
        private readonly ILog Logger;

        /// <summary>
        /// A cache of in-memory objects.
        /// </summary>
        private readonly ResponseCache Cache;

        #region Apis        
        public DolarBotApiService DolarBot { get; private set; }
        public CuttlyApiService Cuttly { get; private set; }
        public TopggApiService Topgg { get; private set; }
        #endregion

        /// <summary>
        /// Creates an ApiCalls object and instantiates the available API objects.
        /// </summary>
        /// <param name="configuration">An <see cref="IConfiguration"/> object to access application settings.</param>
        /// <param name="logger">The log4net logger.</param>
        public ApiCalls(IConfiguration configuration, ILog logger)
        {
            Logger = logger;
            Cache = new ResponseCache(configuration);
            DolarBot = new DolarBotApiService(configuration, Cache, LogError);
            Cuttly = new CuttlyApiService(configuration);
            Topgg = new TopggApiService(configuration, logger);
        }

        /// <summary>
        /// Logs an error from a REST response using log4net <see cref="ILog"/> object.
        /// </summary>
        /// <param name="response"></param>
        private void LogError(RestResponse response)
        {
            if (response.ErrorException != null)
            {
                Logger.Error($"API error. Endpoint returned {response.StatusCode}: {response.StatusDescription}", response.ErrorException);
            }
            else
            {
                Logger.Error($"API error. Endpoint returned {response.StatusCode}: {response.StatusDescription}");
            }
        }
    }
}
