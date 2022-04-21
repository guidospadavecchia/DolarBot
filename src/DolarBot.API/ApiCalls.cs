﻿using DolarBot.API.Attributes;
using DolarBot.API.Cache;
using DolarBot.API.Models;
using DolarBot.API.Models.Cuttly;
using DolarBot.Util.Extensions;
using log4net;
using Microsoft.Extensions.Configuration;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;

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
        public DolarBotApi DolarBot { get; private set; }
        public CuttlyApi Cuttly { get; private set; }
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
            DolarBot = new DolarBotApi(configuration, Cache, LogError);
            Cuttly = new CuttlyApi(configuration);
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

        [Description("https://github.com/guidospadavecchia/DolarBot-Api")]
        public class DolarBotApi
        {
            #region Constants
            private const string API_KEY_NAME = "DOLARBOT_APIKEY";
            private const string ROOT_ENDPOINT = "/api/status";

            //Dólar
            private const string DOLAR_OFICIAL_ENDPOINT = "/api/dolar/oficial";
            private const string DOLAR_AHORRO_ENDPOINT = "/api/dolar/ahorro";
            private const string DOLAR_BLUE_ENDPOINT = "/api/dolar/blue";
            private const string DOLAR_CONTADO_LIQUI_ENDPOINT = "/api/dolar/contadoliqui";
            private const string DOLAR_PROMEDIO_ENDPOINT = "/api/dolar/promedio";
            private const string DOLAR_BOLSA_ENDPOINT = "/api/dolar/bolsa";
            private const string DOLAR_NACION_ENDPOINT = "/api/dolar/bancos/nacion";
            private const string DOLAR_BBVA_ENDPOINT = "/api/dolar/bancos/bbva";
            private const string DOLAR_PIANO_ENDPOINT = "/api/dolar/bancos/piano";
            private const string DOLAR_HIPOTECARIO_ENDPOINT = "/api/dolar/bancos/hipotecario";
            private const string DOLAR_GALICIA_ENDPOINT = "/api/dolar/bancos/galicia";
            private const string DOLAR_SANTANDER_ENDPOINT = "/api/dolar/bancos/santander";
            private const string DOLAR_CIUDAD_ENDPOINT = "/api/dolar/bancos/ciudad";
            private const string DOLAR_SUPERVIELLE_ENDPOINT = "/api/dolar/bancos/supervielle";
            private const string DOLAR_PATAGONIA_ENDPOINT = "/api/dolar/bancos/patagonia";
            private const string DOLAR_COMAFI_ENDPOINT = "/api/dolar/bancos/comafi";
            private const string DOLAR_BANCOR_ENDPOINT = "/api/dolar/bancos/bancor";
            private const string DOLAR_CHACO_ENDPOINT = "/api/dolar/bancos/chaco";
            private const string DOLAR_PAMPA_ENDPOINT = "/api/dolar/bancos/pampa";
            private const string DOLAR_PROVINCIA_ENDPOINT = "/api/dolar/bancos/provincia";
            private const string DOLAR_ICBC_ENDPOINT = "/api/dolar/bancos/icbc";
            private const string DOLAR_REBANKING_ENDPOINT = "/api/dolar/bancos/reba";
            private const string DOLAR_ROELA_ENDPOINT = "/api/dolar/bancos/roela";

            //Euro
            private const string EURO_OFICIAL_ENDPOINT = "/api/euro/oficial";
            private const string EURO_AHORRO_ENDPOINT = "/api/euro/ahorro";
            private const string EURO_BLUE_ENDPOINT = "/api/euro/blue";
            private const string EURO_NACION_ENDPOINT = "/api/euro/bancos/nacion";
            private const string EURO_GALICIA_ENDPOINT = "/api/euro/bancos/galicia";
            private const string EURO_BBVA_ENDPOINT = "/api/euro/bancos/bbva";
            private const string EURO_HIPOTECARIO_ENDPOINT = "/api/euro/bancos/hipotecario";
            private const string EURO_CHACO_ENDPOINT = "/api/euro/bancos/chaco";
            private const string EURO_PAMPA_ENDPOINT = "/api/euro/bancos/pampa";
            private const string EURO_PIANO_ENDPOINT = "/api/euro/bancos/piano";
            private const string EURO_SANTANDER_ENDPOINT = "/api/euro/bancos/santander";
            private const string EURO_CIUDAD_ENDPOINT = "/api/euro/bancos/ciudad";
            private const string EURO_SUPERVIELLE_ENDPOINT = "/api/euro/bancos/supervielle";
            private const string EURO_PATAGONIA_ENDPOINT = "/api/euro/bancos/patagonia";
            private const string EURO_COMAFI_ENDPOINT = "/api/euro/bancos/comafi";
            private const string EURO_REBANKING_ENDPOINT = "/api/euro/bancos/reba";
            private const string EURO_ROELA_ENDPOINT = "/api/euro/bancos/roela";

            //Real
            private const string REAL_OFICIAL_ENDPOINT = "/api/real/oficial";
            private const string REAL_AHORRO_ENDPOINT = "/api/real/ahorro";
            private const string REAL_BLUE_ENDPOINT = "/api/real/blue";
            private const string REAL_NACION_ENDPOINT = "/api/real/bancos/nacion";
            private const string REAL_BBVA_ENDPOINT = "/api/real/bancos/bbva";
            private const string REAL_CHACO_ENDPOINT = "/api/real/bancos/chaco";
            private const string REAL_PIANO_ENDPOINT = "/api/real/bancos/piano";
            private const string REAL_CIUDAD_ENDPOINT = "/api/real/bancos/ciudad";
            private const string REAL_SUPERVIELLE_ENDPOINT = "/api/real/bancos/supervielle";

            //Other currencies
            private const string WORLD_CURRENCY_ENDPOINT = "/api/monedas/valor";
            private const string WORLD_CURRENCIES_LIST_ENDPOINT = "/api/monedas/lista";
            private const string WORLD_CURRENCIES_HISTORICAL_ENDPOINT = "/api/monedas/historico";
            private const string WORLD_CURRENCIES_LIST_KEY = "WorldCurrenciesList";

            //BCRA
            private const string RESERVAS_ENDPOINT = "/api/bcra/reservas";
            private const string CIRCULANTE_ENDPOINT = "/api/bcra/circulante";
            private const string RIESGO_PAIS_ENDPOINT = "/api/bcra/riesgopais";
            private const string RIESGO_PAIS_CACHE_KEY = "RiesgoPais";

            //Metales
            private const string ORO_ENDPOINT = "/api/metales/oro";
            private const string PLATA_ENDPOINT = "/api/metales/plata";
            private const string COBRE_ENDPOINT = "/api/metales/cobre";

            //Crypto
            private const string CRYPTO_ENDPOINT = "/api/crypto";
            private const string BINANCECOIN_ENDPOINT = "/api/crypto/binancecoin";
            private const string BITCOIN_ENDPOINT = "/api/crypto/bitcoin";
            private const string BITCOINCASH_ENDPOINT = "/api/crypto/bitcoincash";
            private const string CARDANO_ENDPOINT = "/api/crypto/cardano";
            private const string CHAINLINK_ENDPOINT = "/api/crypto/chainlink";
            private const string DAI_ENDPOINT = "/api/crypto/dai";
            private const string DASH_ENDPOINT = "/api/crypto/dash";
            private const string DOGECOIN_ENDPOINT = "/api/crypto/dogecoin";
            private const string ETHEREUM_ENDPOINT = "/api/crypto/ethereum";
            private const string MONERO_ENDPOINT = "/api/crypto/monero";
            private const string LITECOIN_ENDPOINT = "/api/crypto/litecoin";
            private const string POLKADOT_ENDPOINT = "/api/crypto/polkadot";
            private const string RIPPLE_ENDPOINT = "/api/crypto/ripple";
            private const string STELLAR_ENDPOINT = "/api/crypto/stellar";
            private const string TETHER_ENDPOINT = "/api/crypto/tether";
            private const string THETA_ENDPOINT = "/api/crypto/theta-token";
            private const string UNISWAP_ENDPOINT = "/api/crypto/uniswap";
            private const string CRYPTO_CURRENCIES_LIST_ENDPOINT = "/api/crypto/list";
            private const string CRYPTO_CURRENCIES_LIST_KEY = "CryptoCurrenciesList";

            //Venezuela
            private const string VZLA_DOLAR_ENDOPOINT = "/api/vzla/dolar";
            private const string VZLA_EURO_ENDPOINT = "/api/vzla/euro";

            //Historical Rates
            private const string EVOLUCION_DOLAR_OFICIAL_ENDPOINT = "/api/evolucion/dolar/oficial";
            private const string EVOLUCION_DOLAR_AHORRO_ENDPOINT = "/api/evolucion/dolar/ahorro";
            private const string EVOLUCION_DOLAR_BLUE_ENDPOINT = "/api/evolucion/dolar/blue";
            private const string EVOLUCION_EURO_OFICIAL_ENDPOINT = "/api/evolucion/euro/oficial";
            private const string EVOLUCION_EURO_AHORRO_ENDPOINT = "/api/evolucion/euro/ahorro";
            private const string EVOLUCION_REAL_OFICIAL_ENDPOINT = "/api/evolucion/real/oficial";
            private const string EVOLUCION_REAL_AHORRO_ENDPOINT = "/api/evolucion/real/ahorro";
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

            #region Enums
            /// <summary>
            /// Represents the different API endpoints for dollar rates.
            /// </summary>
            public enum DollarTypes
            {
                [Description(DOLAR_OFICIAL_ENDPOINT)]
                Oficial,
                [Description(DOLAR_AHORRO_ENDPOINT)]
                Ahorro,
                [Description(DOLAR_BLUE_ENDPOINT)]
                Blue,
                [Description(DOLAR_CONTADO_LIQUI_ENDPOINT)]
                ContadoConLiqui,
                [Description(DOLAR_PROMEDIO_ENDPOINT)]
                Promedio,
                [Description(DOLAR_BOLSA_ENDPOINT)]
                Bolsa,
                [Description(DOLAR_NACION_ENDPOINT)]
                Nacion,
                [Description(DOLAR_BBVA_ENDPOINT)]
                BBVA,
                [Description(DOLAR_PIANO_ENDPOINT)]
                Piano,
                [Description(DOLAR_HIPOTECARIO_ENDPOINT)]
                Hipotecario,
                [Description(DOLAR_GALICIA_ENDPOINT)]
                Galicia,
                [Description(DOLAR_SANTANDER_ENDPOINT)]
                Santander,
                [Description(DOLAR_CIUDAD_ENDPOINT)]
                Ciudad,
                [Description(DOLAR_SUPERVIELLE_ENDPOINT)]
                Supervielle,
                [Description(DOLAR_PATAGONIA_ENDPOINT)]
                Patagonia,
                [Description(DOLAR_COMAFI_ENDPOINT)]
                Comafi,
                [Description(DOLAR_BANCOR_ENDPOINT)]
                Bancor,
                [Description(DOLAR_CHACO_ENDPOINT)]
                Chaco,
                [Description(DOLAR_PAMPA_ENDPOINT)]
                Pampa,
                [Description(DOLAR_PROVINCIA_ENDPOINT)]
                Provincia,
                [Description(DOLAR_ICBC_ENDPOINT)]
                ICBC,
                [Description(DOLAR_REBANKING_ENDPOINT)]
                Reba,
                [Description(DOLAR_ROELA_ENDPOINT)]
                Roela,
            }

            /// <summary>
            /// Represents the different API endpoints for euro rates.
            /// </summary>
            public enum EuroTypes
            {
                [Description(EURO_OFICIAL_ENDPOINT)]
                Oficial,
                [Description(EURO_AHORRO_ENDPOINT)]
                Ahorro,
                [Description(EURO_BLUE_ENDPOINT)]
                Blue,
                [Description(EURO_NACION_ENDPOINT)]
                Nacion,
                [Description(EURO_GALICIA_ENDPOINT)]
                Galicia,
                [Description(EURO_BBVA_ENDPOINT)]
                BBVA,
                [Description(EURO_HIPOTECARIO_ENDPOINT)]
                Hipotecario,
                [Description(EURO_PAMPA_ENDPOINT)]
                Pampa,
                [Description(EURO_CHACO_ENDPOINT)]
                Chaco,
                [Description(EURO_PIANO_ENDPOINT)]
                Piano,
                [Description(EURO_SANTANDER_ENDPOINT)]
                Santander,
                [Description(EURO_CIUDAD_ENDPOINT)]
                Ciudad,
                [Description(EURO_SUPERVIELLE_ENDPOINT)]
                Supervielle,
                [Description(EURO_PATAGONIA_ENDPOINT)]
                Patagonia,
                [Description(EURO_COMAFI_ENDPOINT)]
                Comafi,
                [Description(EURO_REBANKING_ENDPOINT)]
                Reba,
                [Description(EURO_ROELA_ENDPOINT)]
                Roela
            }

            /// <summary>
            /// Represents the different API endpoints for Real rates.
            /// </summary>
            public enum RealTypes
            {
                [Description(REAL_OFICIAL_ENDPOINT)]
                Oficial,
                [Description(REAL_AHORRO_ENDPOINT)]
                Ahorro,
                [Description(REAL_BLUE_ENDPOINT)]
                Blue,
                [Description(REAL_NACION_ENDPOINT)]
                Nacion,
                [Description(REAL_BBVA_ENDPOINT)]
                BBVA,
                [Description(REAL_CHACO_ENDPOINT)]
                Chaco,
                [Description(REAL_PIANO_ENDPOINT)]
                Piano,
                [Description(REAL_CIUDAD_ENDPOINT)]
                Ciudad,
                [Description(REAL_SUPERVIELLE_ENDPOINT)]
                Supervielle
            }

            /// <summary>
            /// Represents the different API endpoints for BCRA values.
            /// </summary>
            public enum BcraValues
            {
                [Description(RESERVAS_ENDPOINT)]
                Reservas,
                [Description(CIRCULANTE_ENDPOINT)]
                Circulante
            }

            /// <summary>
            /// Represents the different API endpoints for precious metals rates.
            /// </summary>
            public enum Metals
            {
                [Description(ORO_ENDPOINT)]
                Gold,
                [Description(PLATA_ENDPOINT)]
                Silver,
                [Description(COBRE_ENDPOINT)]
                Copper
            }

            /// <summary>
            /// Represents the different API endpoints for cryptocurrencies rates.
            /// </summary>
            public enum CryptoCurrencies
            {
                [Description(BINANCECOIN_ENDPOINT)]
                [CryptoCode("binancecoin", "BNB")]
                BinanceCoin,
                [Description(BITCOIN_ENDPOINT)]
                [CryptoCode("bitcoin", "BTC")]
                Bitcoin,
                [Description(BITCOINCASH_ENDPOINT)]
                [CryptoCode("bitcoin-cash", "BCH")]
                BitcoinCash,
                [Description(CARDANO_ENDPOINT)]
                [CryptoCode("cardano", "ADA")]
                Cardano,
                [Description(CHAINLINK_ENDPOINT)]
                [CryptoCode("chainlink", "LINK")]
                Chainlink,
                [Description(DAI_ENDPOINT)]
                [CryptoCode("dai", "DAI")]
                DAI,
                [Description(DASH_ENDPOINT)]
                [CryptoCode("dash", "DASH")]
                Dash,
                [Description(DOGECOIN_ENDPOINT)]
                [CryptoCode("dogecoin", "DOGE")]
                DogeCoin,
                [Description(ETHEREUM_ENDPOINT)]
                [CryptoCode("ethereum", "ETH")]
                Ethereum,
                [Description(MONERO_ENDPOINT)]
                [CryptoCode("monero", "XMR")]
                Monero,
                [Description(LITECOIN_ENDPOINT)]
                [CryptoCode("litecoin", "LTC")]
                Litecoin,
                [Description(POLKADOT_ENDPOINT)]
                [CryptoCode("polkadot", "DOT")]
                Polkadot,
                [Description(RIPPLE_ENDPOINT)]
                [CryptoCode("ripple", "XRP")]
                Ripple,
                [Description(STELLAR_ENDPOINT)]
                [CryptoCode("stellar", "XLM")]
                Stellar,
                [Description(TETHER_ENDPOINT)]
                [CryptoCode("tether", "USDT")]
                Tether,
                [Description(THETA_ENDPOINT)]
                [CryptoCode("theta-token", "THETA")]
                Theta,
                [Description(UNISWAP_ENDPOINT)]
                [CryptoCode("uniswap", "UNI")]
                Uniswap
            }

            /// <summary>
            /// Represents the different API endpoints for historical rates.
            /// </summary>
            public enum HistoricalRatesParams
            {
                [Description(EVOLUCION_DOLAR_OFICIAL_ENDPOINT)]
                Dolar,
                [Description(EVOLUCION_DOLAR_BLUE_ENDPOINT)]
                DolarBlue,
                [Description(EVOLUCION_DOLAR_AHORRO_ENDPOINT)]
                DolarAhorro,
                [Description(EVOLUCION_EURO_OFICIAL_ENDPOINT)]
                Euro,
                [Description(EVOLUCION_EURO_AHORRO_ENDPOINT)]
                EuroAhorro,
                [Description(EVOLUCION_REAL_OFICIAL_ENDPOINT)]
                Real,
                [Description(EVOLUCION_REAL_AHORRO_ENDPOINT)]
                RealAhorro
            }

            /// <summary>
            /// Represents the different API endpoints for Venezuela (Bolivar to Dollar) rates.
            /// </summary>
            public enum VenezuelaTypes
            {
                [Description(VZLA_DOLAR_ENDOPOINT)]
                Dollar,
                [Description(VZLA_EURO_ENDPOINT)]
                Euro,
            }
            #endregion

            /// <summary>
            /// Creats a <see cref="DolarBotApi"/> object using the provided configuration, cache and error action.
            /// </summary>
            /// <param name="configuration">An <see cref="IConfiguration"/> object to access application settings.</param>
            /// <param name="cache">A cache of in-memory objects.</param>
            /// <param name="onError">An action to execute in case of error.</param>
            internal DolarBotApi(IConfiguration configuration, ResponseCache cache, Action<RestResponse> onError)
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
            /// Gets the <see cref="DolarBotApi"/> current culture.
            /// </summary>
            /// <returns>A <see cref="CultureInfo"/> object that represents the API culture.</returns>
            public static CultureInfo GetApiCulture() => CultureInfo.GetCultureInfo("en-US");

            /// <summary>
            /// Queries the API and returns its current status.
            /// </summary>
            /// <returns></returns>
            public async Task<HttpStatusCode?> GetApiStatus()
            {
                RestRequest request = new(ROOT_ENDPOINT);
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
                    RestRequest request = new(WORLD_CURRENCIES_LIST_ENDPOINT);
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
                string endpoint = $"{WORLD_CURRENCY_ENDPOINT}/{currencyCode.ToUpper()}";
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
                string endpoint = $"{WORLD_CURRENCIES_HISTORICAL_ENDPOINT}/{currencyCode.ToUpper()}";
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
            public async Task<DollarResponse> GetDollarRate(DollarTypes type)
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
            public async Task<EuroResponse> GetEuroRate(EuroTypes type)
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
            public async Task<RealResponse> GetRealRate(RealTypes type)
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
                    RestRequest request = new(RIESGO_PAIS_ENDPOINT);
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
            public async Task<BcraResponse> GetBcraValue(BcraValues bcraValue)
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
            public async Task<MetalResponse> GetMetalRate(Metals metal)
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
                    RestRequest request = new(CRYPTO_CURRENCIES_LIST_ENDPOINT);
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
                string endpoint = $"{CRYPTO_ENDPOINT}/{cryptoCurrencyCode.ToUpper()}";
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
            public async Task<VzlaResponse> GetVzlaRates(VenezuelaTypes type)
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
            public async Task<HistoricalRatesResponse> GetHistoricalRates(HistoricalRatesParams historicalRatesParam)
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

        [Description("https://cutt.ly/cuttly-api")]
        public class CuttlyApi
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
            /// Creats a <see cref="CuttlyApi"/> object using the provided configuration.
            /// </summary>
            /// <param name="configuration">An <see cref="IConfiguration"/> object to access application settings.</param>
            internal CuttlyApi(IConfiguration configuration)
            {
                Configuration = configuration;
                ApiKey = Configuration["cuttlyApiKey"];

                RestClientOptions options = new(Configuration["cuttlyBaseUrl"]);
                if (int.TryParse(Configuration["cuttlyRequestTimeout"], out int timeoutSeconds) && timeoutSeconds > 0)
                {
                    options.Timeout = Convert.ToInt32(TimeSpan.FromSeconds(timeoutSeconds).TotalMilliseconds);
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
}
