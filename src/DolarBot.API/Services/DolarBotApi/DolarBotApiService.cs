using DolarBot.API.Attributes;
using DolarBot.API.Cache;
using DolarBot.API.Enums;
using DolarBot.API.Models;
using DolarBot.Util.Extensions;
using Microsoft.Extensions.Configuration;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;

namespace DolarBot.API.Services.DolarBotApi
{
    [Description("https://github.com/guidospadavecchia/DolarBot-Api")]
    public class DolarBotApiService
    {
        #region Constants
        private const string API_KEY_NAME = "DOLARBOT_APIKEY";
        private const string RIESGO_PAIS_CACHE_KEY = "RiesgoPais";
        private const string CRYPTO_CURRENCIES_LIST_KEY = "CryptoCurrenciesList";
        private const string WORLD_CURRENCIES_LIST_KEY = "WorldCurrenciesList";
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
        /// A cache of in-memory objects.
        /// </summary>
        private readonly ResponseCache Cache;

        /// <summary>
        /// An action to execute in case of error.
        /// </summary>
        private readonly Action<RestResponse> OnError;
        #endregion

        /// <summary>
        /// Creats a <see cref="DolarBotApiService"/> object using the provided configuration, cache and error action.
        /// </summary>
        /// <param name="configuration">An <see cref="IConfiguration"/> object to access application settings.</param>
        /// <param name="cache">A cache of in-memory objects.</param>
        /// <param name="onError">An action to execute in case of error.</param>
        internal DolarBotApiService(IConfiguration configuration, ResponseCache cache, Action<RestResponse> onError)
        {
            Configuration = configuration;
            Cache = cache;
            OnError = onError;

            RestClientOptions options = new(Configuration["apiUrl"]);
            if (int.TryParse(Configuration["apiRequestTimeout"], out int timeoutSeconds) && timeoutSeconds > 0)
            {
                options.Timeout = Convert.ToInt32(TimeSpan.FromSeconds(timeoutSeconds).TotalMilliseconds);
            }
            Client = new RestClient(options);
            Client.UseNewtonsoftJson();
            Client.AddDefaultHeader(API_KEY_NAME, !string.IsNullOrEmpty(Configuration["apiKey"]) ? Configuration["apiKey"] : Environment.GetEnvironmentVariable(API_KEY_NAME));
        }

        /// <summary>
        /// Gets the <see cref="DolarBotApiService"/> current culture.
        /// </summary>
        /// <returns>A <see cref="CultureInfo"/> object that represents the API culture.</returns>
        public static CultureInfo GetApiCulture() => CultureInfo.GetCultureInfo("en-US");

        /// <summary>
        /// Queries the API and returns its current status.
        /// </summary>
        /// <returns></returns>
        public async Task<HttpStatusCode?> GetApiStatus()
        {
            RestRequest request = new(InfoEndpoints.Status.GetDescription());
            var response = await Client.ExecuteGetAsync(request);
            return response?.StatusCode;
        }

        /// <summary>
        /// Queries the API and returns the list of world currency codes.
        /// </summary>
        /// <returns>A task that contains a collection of <see cref="WorldCurrencyCodeResponse"/> objects.</returns>
        public async Task<List<WorldCurrencyCodeResponse>> GetWorldCurrenciesList()
        {
            List<WorldCurrencyCodeResponse> cachedResponse = Cache.GetObject<List<WorldCurrencyCodeResponse>>(WORLD_CURRENCIES_LIST_KEY);

            if (cachedResponse != null)
            {
                return cachedResponse;
            }
            else
            {
                RestRequest request = new(WorldCurrencyEndpoints.List.GetDescription());
                RestResponse<List<WorldCurrencyCodeResponse>> response = await Client.ExecuteGetAsync<List<WorldCurrencyCodeResponse>>(request);
                if (response.IsSuccessful)
                {
                    Cache.SaveObject(WORLD_CURRENCIES_LIST_KEY, response.Data, Cache.GetCurrencyListExpiration());
                    return response.Data;
                }
                else
                {
                    OnError(response);
                    return null;
                }
            }
        }

        /// <summary>
        /// Queries the API and returns the value of a world currency by it's unique code.
        /// </summary>
        /// <param name="currencyCode">The currency 3-digit code.</param>
        /// <returns>A task that contains a normalized <see cref="WorldCurrencyResponse"/> object.</returns>
        public async Task<WorldCurrencyResponse> GetWorldCurrencyValue(string currencyCode)
        {
            string endpoint = $"{WorldCurrencyEndpoints.Base.GetDescription()}/{currencyCode.ToUpper()}";
            WorldCurrencyResponse cachedResponse = Cache.GetObject<WorldCurrencyResponse>(endpoint);
            if (cachedResponse != null)
            {
                return cachedResponse;
            }
            else
            {
                RestRequest request = new(endpoint);
                RestResponse<WorldCurrencyResponse> response = await Client.ExecuteGetAsync<WorldCurrencyResponse>(request);
                if (response.IsSuccessful)
                {
                    WorldCurrencyResponse data = response.Data;
                    data.Code = currencyCode.ToUpper().Trim();
                    Cache.SaveObject(endpoint, data);
                    return data;
                }
                else
                {
                    OnError(response);
                    return null;
                }
            }
        }

        /// <summary>
        /// Queries the API and returns the historical values for a particular <paramref name="currencyCode"/>.
        /// </summary>
        /// <param name="currencyCode">The currency 3-digit code.</param>
        /// <returns>A task that contains a collection of normalized <see cref="WorldCurrencyResponse"/> objects.</returns>
        public async Task<List<WorldCurrencyResponse>> GetWorldCurrencyHistoricalValues(string currencyCode)
        {
            string endpoint = $"{WorldCurrencyEndpoints.Historical.GetDescription()}/{currencyCode.ToUpper()}";
            List<WorldCurrencyResponse> cachedResponse = Cache.GetObject<List<WorldCurrencyResponse>>(endpoint);
            if (cachedResponse != null)
            {
                return cachedResponse;
            }
            else
            {
                RestRequest request = new(endpoint);
                RestResponse<List<WorldCurrencyResponse>> response = await Client.ExecuteGetAsync<List<WorldCurrencyResponse>>(request);
                if (response.IsSuccessful)
                {
                    List<WorldCurrencyResponse> data = response.Data;
                    data.ForEach(x => x.Code = currencyCode.ToUpper().Trim());
                    Cache.SaveObject(endpoint, data, Cache.GetCurrencyListExpiration());
                    return data;
                }
                else
                {
                    OnError(response);
                    return null;
                }
            }
        }

        /// <summary>
        /// Queries an API endpoint asynchronously and returs its result.
        /// </summary>
        /// <param name="type">The type of dollar (endpoint) to query.</param>
        /// <returns>A task that contains a normalized <see cref="DollarResponse"/> object.</returns>
        public async Task<DollarResponse> GetDollarRate(DollarEndpoints type)
        {
            DollarResponse cachedResponse = Cache.GetObject<DollarResponse>(type);
            if (cachedResponse != null)
            {
                return cachedResponse;
            }
            else
            {
                string endpoint = type.GetDescription();
                RestRequest request = new(endpoint);
                RestResponse<DollarResponse> response = await Client.ExecuteGetAsync<DollarResponse>(request);
                if (response.IsSuccessful)
                {
                    DollarResponse dolarResponse = response.Data;
                    dolarResponse.Type = type;
                    Cache.SaveObject(type, dolarResponse);

                    return dolarResponse;
                }
                else
                {
                    OnError(response);
                    return null;
                }
            }
        }

        /// <summary>
        /// Queries an API endpoint asynchronously and returs its result.
        /// </summary>
        /// <param name="type">The type of euro (endpoint) to query.</param>
        /// <returns>A task that contains a normalized <see cref="EuroResponse"/> object.</returns>
        public async Task<EuroResponse> GetEuroRate(EuroEndpoints type)
        {
            EuroResponse cachedResponse = Cache.GetObject<EuroResponse>(type);
            if (cachedResponse != null)
            {
                return cachedResponse;
            }
            else
            {
                string endpoint = type.GetDescription();
                RestRequest request = new(endpoint);
                RestResponse<EuroResponse> response = await Client.ExecuteGetAsync<EuroResponse>(request);
                if (response.IsSuccessful)
                {
                    EuroResponse euroResponse = response.Data;
                    euroResponse.Type = type;
                    Cache.SaveObject(type, euroResponse);

                    return euroResponse;
                }
                else
                {
                    OnError(response);
                    return null;
                }
            }
        }

        /// <summary>
        /// Queries an API endpoint asynchronously and returs its result.
        /// </summary>
        /// <param name="type">The type of Real (endpoint) to query.</param>
        /// <returns>A task that contains a normalized <see cref="RealResponse"/> object.</returns>
        public async Task<RealResponse> GetRealRate(RealEndpoints type)
        {
            RealResponse cachedResponse = Cache.GetObject<RealResponse>(type);
            if (cachedResponse != null)
            {
                return cachedResponse;
            }
            else
            {
                string endpoint = type.GetDescription();
                RestRequest request = new(endpoint);
                RestResponse<RealResponse> response = await Client.ExecuteGetAsync<RealResponse>(request);
                if (response.IsSuccessful)
                {
                    RealResponse realResponse = response.Data;
                    realResponse.Type = type;
                    Cache.SaveObject(type, realResponse);

                    return realResponse;
                }
                else
                {
                    OnError(response);
                    return null;
                }
            }
        }

        /// <summary>
        /// Queries the API endpoint asynchronously and returns a <see cref="CountryRiskResponse"/> object.
        /// </summary>
        /// <returns>A task that contains a normalized <see cref="CountryRiskResponse"/> object.</returns>
        public async Task<CountryRiskResponse> GetCountryRiskValue()
        {
            CountryRiskResponse cachedResponse = Cache.GetObject<CountryRiskResponse>(RIESGO_PAIS_CACHE_KEY);
            if (cachedResponse != null)
            {
                return cachedResponse;
            }
            else
            {
                RestRequest request = new(BcraIndicatorsEndpoints.RiesgoPais.GetDescription());
                RestResponse<CountryRiskResponse> response = await Client.ExecuteGetAsync<CountryRiskResponse>(request);
                if (response.IsSuccessful)
                {
                    Cache.SaveObject(RIESGO_PAIS_CACHE_KEY, response.Data);
                    return response.Data;
                }
                else
                {
                    OnError(response);
                    return null;
                }
            }
        }

        /// <summary>
        /// Queries the API endpoint asynchronously and returns a <see cref="BcraResponse"/> object.
        /// </summary>
        /// <returns>A task that contains a normalized <see cref="BcraResponse"/> object.</returns>
        public async Task<BcraResponse> GetBcraValue(BcraIndicatorsEndpoints bcraValue)
        {
            BcraResponse cachedResponse = Cache.GetObject<BcraResponse>(bcraValue);
            if (cachedResponse != null)
            {
                return cachedResponse;
            }
            else
            {
                string endpoint = bcraValue.GetDescription();
                RestRequest request = new(endpoint);
                RestResponse<BcraResponse> response = await Client.ExecuteGetAsync<BcraResponse>(request);
                if (response.IsSuccessful)
                {
                    Cache.SaveObject(bcraValue, response.Data);
                    return response.Data;
                }
                else
                {
                    OnError(response);
                    return null;
                }
            }
        }

        /// <summary>
        /// Queries the API endpoint asynchronously and returns a <see cref="MetalResponse"/> object.
        /// </summary>
        /// <returns>A task that contains a normalized <see cref="MetalResponse"/> object.</returns>
        public async Task<MetalResponse> GetMetalRate(MetalEndpoints metal)
        {
            MetalResponse cachedResponse = Cache.GetObject<MetalResponse>(metal);
            if (cachedResponse != null)
            {
                return cachedResponse;
            }
            else
            {
                string endpoint = metal.GetDescription();
                RestRequest request = new(endpoint);
                RestResponse<MetalResponse> response = await Client.ExecuteGetAsync<MetalResponse>(request);
                if (response.IsSuccessful)
                {
                    MetalResponse metalResponse = response.Data;
                    metalResponse.Type = metal;
                    Cache.SaveObject(metal, metalResponse);

                    return metalResponse;
                }
                else
                {
                    OnError(response);
                    return null;
                }
            }
        }

        /// <summary>
        /// Queries the API and returns the list of crypto currency codes.
        /// </summary>
        /// <returns>A task that contains a collection of <see cref="CryptoCodeResponse"/> objects.</returns>
        public async Task<List<CryptoCodeResponse>> GetCryptoCurrenciesList()
        {
            List<CryptoCodeResponse> cachedResponse = Cache.GetObject<List<CryptoCodeResponse>>(CRYPTO_CURRENCIES_LIST_KEY);

            if (cachedResponse != null)
            {
                return cachedResponse;
            }
            else
            {
                RestRequest request = new(CryptoEndpoints.List.GetDescription());
                RestResponse<List<CryptoCodeResponse>> response = await Client.ExecuteGetAsync<List<CryptoCodeResponse>>(request);
                if (response.IsSuccessful)
                {
                    Cache.SaveObject(CRYPTO_CURRENCIES_LIST_KEY, response.Data, Cache.GetCryptoListExpiration());
                    return response.Data;
                }
                else
                {
                    OnError(response);
                    return null;
                }
            }
        }

        /// <summary>
        /// Queries the API endpoint asynchronously and returns a <see cref="CryptoResponse"/> object.
        /// </summary>
        /// <returns>A task that contains a normalized <see cref="CryptoResponse"/> object.</returns>
        public async Task<CryptoResponse> GetCryptoCurrencyRate(CryptoCurrencies cryptoCurrency)
        {
            CryptoResponse cachedResponse = Cache.GetObject<CryptoResponse>(cryptoCurrency);
            if (cachedResponse != null)
            {
                return cachedResponse;
            }
            else
            {
                string endpoint = cryptoCurrency.GetDescription();
                RestRequest request = new(endpoint);
                RestResponse<CryptoResponse> response = await Client.ExecuteGetAsync<CryptoResponse>(request);
                if (response.IsSuccessful)
                {
                    CryptoResponse cryptoResponse = response.Data;
                    cryptoResponse.Currency = cryptoCurrency;
                    Cache.SaveObject(cryptoCurrency, cryptoResponse, Cache.GetCryptoExpiration());

                    return cryptoResponse;
                }
                else
                {
                    OnError(response);
                    return null;
                }
            }
        }

        /// <summary>
        /// Queries the API endpoint asynchronously and returns a <see cref="CryptoResponse"/> object.
        /// </summary>
        /// <returns>A task that contains a normalized <see cref="CryptoResponse"/> object.</returns>
        public async Task<CryptoResponse> GetCryptoCurrencyRate(string cryptoCurrencyCode)
        {
            string endpoint = $"{CryptoEndpoints.Crypto.GetDescription()}/{cryptoCurrencyCode.ToUpper()}";
            CryptoResponse cachedResponse = Cache.GetObject<CryptoResponse>(endpoint);
            if (cachedResponse != null)
            {
                return cachedResponse;
            }
            else
            {
                RestRequest request = new(endpoint);
                RestResponse<CryptoResponse> response = await Client.ExecuteGetAsync<CryptoResponse>(request);
                if (response.IsSuccessful)
                {
                    CryptoResponse cryptoResponse = response.Data;
                    Cache.SaveObject(endpoint, cryptoResponse, Cache.GetCryptoExpiration());
                    return cryptoResponse;
                }
                else
                {
                    OnError(response);
                    return null;
                }
            }
        }

        /// <summary>
        /// Queries the API endpoint asynchronously and returns a <see cref="VzlaResponse"/> object.
        /// </summary>
        /// <returns>A task that contains a normalized <see cref="VzlaResponse"/> object.</returns>
        public async Task<VzlaResponse> GetVzlaRates(VenezuelaEndpoints type)
        {
            VzlaResponse cachedResponse = Cache.GetObject<VzlaResponse>(type);
            if (cachedResponse != null)
            {
                return cachedResponse;
            }
            else
            {
                string endpoint = type.GetDescription();
                RestRequest request = new(endpoint);
                RestResponse<VzlaResponse> response = await Client.ExecuteGetAsync<VzlaResponse>(request);
                if (response.IsSuccessful)
                {
                    VzlaResponse vzlaResponse = response.Data;
                    vzlaResponse.Type = type;
                    Cache.SaveObject(type, vzlaResponse);

                    return vzlaResponse;
                }
                else
                {
                    OnError(response);
                    return null;
                }
            }
        }

        /// <summary>
        /// Queries the API endpoint asynchronously and returns a <see cref="HistoricalRatesResponse"/> object.
        /// </summary>
        /// <returns>A task that contains a normalized <see cref="HistoricalRatesResponse"/> object.</returns>
        public async Task<HistoricalRatesResponse> GetHistoricalRates(HistoricalRatesParamEndpoints historicalRatesParam)
        {
            HistoricalRatesResponse cachedResponse = Cache.GetObject<HistoricalRatesResponse>(historicalRatesParam);
            if (cachedResponse != null)
            {
                return cachedResponse;
            }
            else
            {
                string endpoint = historicalRatesParam.GetDescription();
                RestRequest request = new(endpoint);
                RestResponse<HistoricalRatesResponse> response = await Client.ExecuteGetAsync<HistoricalRatesResponse>(request);
                if (response.IsSuccessful)
                {
                    Cache.SaveObject(historicalRatesParam, response.Data);
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
