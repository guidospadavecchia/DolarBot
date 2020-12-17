using DolarBot.API;
using DolarBot.Services.Banking;
using Microsoft.Extensions.Configuration;

namespace DolarBot.Services.Base
{
    public abstract class BaseService
    {
        #region Vars

        /// <summary>
        /// Provides access to application settings.
        /// </summary>
        protected readonly IConfiguration Configuration;

        /// <summary>
        /// Provides access to the different APIs.
        /// </summary>
        protected readonly ApiCalls Api;

        #endregion

        #region Constructors

        /// <summary>
        /// Base constructor for all services.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        public BaseService(IConfiguration configuration, ApiCalls api)
        {
            Configuration = configuration;
            Api = api;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Retrieves the thumbnail URL for a particular <see cref="Banks"/> object.
        /// </summary>
        /// <param name="bank">The value to retrieve.</param>
        /// <returns>The banks thumbnail URL.</returns>
        public string GetBankThumbnailUrl(Banks bank)
        {
            return bank switch
            {
                Banks.Nacion => Configuration.GetSection("images").GetSection("banks")["nacion"],
                Banks.BBVA => Configuration.GetSection("images").GetSection("banks")["bbva"],
                Banks.Piano => Configuration.GetSection("images").GetSection("banks")["piano"],
                Banks.Hipotecario => Configuration.GetSection("images").GetSection("banks")["hipotecario"],
                Banks.Galicia => Configuration.GetSection("images").GetSection("banks")["galicia"],
                Banks.Santander => Configuration.GetSection("images").GetSection("banks")["santander"],
                Banks.Ciudad => Configuration.GetSection("images").GetSection("banks")["ciudad"],
                Banks.Supervielle => Configuration.GetSection("images").GetSection("banks")["supervielle"],
                Banks.Patagonia => Configuration.GetSection("images").GetSection("banks")["patagonia"],
                Banks.Comafi => Configuration.GetSection("images").GetSection("banks")["comafi"],
                Banks.BIND => Configuration.GetSection("images").GetSection("banks")["bind"],
                Banks.Bancor => Configuration.GetSection("images").GetSection("banks")["bancor"],
                Banks.Chaco => Configuration.GetSection("images").GetSection("banks")["chaco"],
                Banks.Pampa => Configuration.GetSection("images").GetSection("banks")["pampa"],
                _ => string.Empty,
            };
        }

        #endregion
    }
}
