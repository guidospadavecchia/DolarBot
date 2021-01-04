using DolarBot.API;
using DolarBot.API.Models.Base;
using DolarBot.Services.Banking;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;

namespace DolarBot.Services.Base
{
    /// <summary>
    /// Base class for currency related services
    /// </summary>
    public abstract class BaseCurrencyService : BaseService
    {
        #region Constructors

        /// <summary>
        /// Creates a new <see cref="BaseCurrencyService"/> object with the provided configuration and API object.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        public BaseCurrencyService(IConfiguration configuration, ApiCalls api) : base(configuration, api) { }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a collection of valid banks for the currency.
        /// </summary>
        /// <returns>Collection of valid banks.</returns>
        public abstract Banks[] GetValidBanks();

        /// <summary>
        /// Applies all taxes to the <see cref="CurrencyResponse"/> object.
        /// </summary>
        /// <param name="response">The response without taxes applied.</param>
        /// <returns>The response with taxes applied.</returns>
        public CurrencyResponse ApplyTaxes(CurrencyResponse response)
        {
            if (response != null)
            {
                CurrencyResponse clonedResponse = response.Clone() as CurrencyResponse;
                CultureInfo apiCulture = Api.DolarBot.GetApiCulture();
                decimal taxPercent = (decimal.Parse(Configuration["taxPercent"]) / 100) + 1;
                if (decimal.TryParse(clonedResponse.Venta, NumberStyles.Any, apiCulture, out decimal venta))
                {
                    clonedResponse.Venta = Convert.ToDecimal(venta * taxPercent, apiCulture).ToString("N2", apiCulture);
                }

                return clonedResponse;
            }
            else
            {
                return null;
            }
        }

        #endregion
    }
}
