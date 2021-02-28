using DolarBot.API.Cache;
using DolarBot.API.Models;
using DolarBot.Util.Extensions;
using log4net;
using Microsoft.Extensions.Configuration;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using System;
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
        private readonly ILog logger;

        /// <summary>
        /// A cache of in-memory objects.
        /// </summary>
        private readonly ResponseCache cache;

        #region Apis        
        public DolarBotApi DolarBot { get; private set; }
        #endregion

        /// <summary>
        /// Creates an ApiCalls object and instantiates the available API objects.
        /// </summary>
        /// <param name="configuration">An <see cref="IConfiguration"/> object to access application settings.</param>
        /// <param name="logger">The log4net logger.</param>
        public ApiCalls(IConfiguration configuration, ILog logger)
        {
            this.logger = logger;
            cache = new ResponseCache(configuration);
            DolarBot = new DolarBotApi(configuration, cache, LogError);
        }

        /// <summary>
        /// Logs an error from a REST response using log4net <see cref="ILog"/> object.
        /// </summary>
        /// <param name="response"></param>
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
            private const string DOLAR_BIND_ENDPOINT = "/api/dolar/bancos/bind";
            private const string DOLAR_BANCOR_ENDPOINT = "/api/dolar/bancos/bancor";
            private const string DOLAR_CHACO_ENDPOINT = "/api/dolar/bancos/chaco";
            private const string DOLAR_PAMPA_ENDPOINT = "/api/dolar/bancos/pampa";

            //Euro
            private const string EURO_NACION_ENDPOINT = "/api/euro/bancos/nacion";
            private const string EURO_GALICIA_ENDPOINT = "/api/euro/bancos/galicia";
            private const string EURO_BBVA_ENDPOINT = "/api/euro/bancos/bbva";
            private const string EURO_HIPOTECARIO_ENDPOINT = "/api/euro/bancos/hipotecario";
            private const string EURO_CHACO_ENDPOINT = "/api/euro/bancos/chaco";
            private const string EURO_PAMPA_ENDPOINT = "/api/euro/bancos/pampa";

            //Real
            private const string REAL_NACION_ENDPOINT = "/api/real/bancos/nacion";
            private const string REAL_BBVA_ENDPOINT = "/api/real/bancos/bbva";
            private const string REAL_CHACO_ENDPOINT = "/api/real/bancos/chaco";

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
            private const string BITCOIN_ENDPOINT = "/api/crypto/bitcoin";
            private const string BITCOINCASH_ENDPOINT = "/api/crypto/bitcoincash";
            private const string ETHEREUM_ENDPOINT = "/api/crypto/ethereum";
            private const string MONERO_ENDPOINT = "/api/crypto/monero";
            private const string LITECOIN_ENDPOINT = "/api/crypto/litecoin";
            private const string RIPPLE_ENDPOINT = "/api/crypto/ripple";
            private const string DASH_ENDPOINT = "/api/crypto/dash";

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
            private readonly Action<IRestResponse> OnError;
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
                [Description(DOLAR_BIND_ENDPOINT)]
                BIND,
                [Description(DOLAR_BANCOR_ENDPOINT)]
                Bancor,
                [Description(DOLAR_CHACO_ENDPOINT)]
                Chaco,
                [Description(DOLAR_PAMPA_ENDPOINT)]
                Pampa,
            }

            /// <summary>
            /// Represents the different API endpoints for euro rates.
            /// </summary>
            public enum EuroTypes
            {
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
                Chaco
            }

            /// <summary>
            /// Represents the different API endpoints for Real rates.
            /// </summary>
            public enum RealTypes
            {
                [Description(REAL_NACION_ENDPOINT)]
                Nacion,
                [Description(REAL_BBVA_ENDPOINT)]
                BBVA,
                [Description(REAL_CHACO_ENDPOINT)]
                Chaco
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
                [Description(BITCOIN_ENDPOINT)]
                Bitcoin,
                [Description(BITCOINCASH_ENDPOINT)]
                BitcoinCash,
                [Description(ETHEREUM_ENDPOINT)]
                Ethereum,
                [Description(MONERO_ENDPOINT)]
                Monero,
                [Description(LITECOIN_ENDPOINT)]
                Litecoin,
                [Description(RIPPLE_ENDPOINT)]
                Ripple,
                [Description(DASH_ENDPOINT)]
                Dash
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
            internal DolarBotApi(IConfiguration configuration, ResponseCache cache, Action<IRestResponse> onError)
            {
                Configuration = configuration;
                Cache = cache;
                OnError = onError;

                Client = new RestClient(Configuration["apiUrl"]);
                Client.UseNewtonsoftJson();
                Client.AddDefaultHeader(API_KEY_NAME, !string.IsNullOrEmpty(Configuration["apiKey"]) ? Configuration["apiKey"] : Environment.GetEnvironmentVariable(API_KEY_NAME));
                
                if (int.TryParse(Configuration["apiRequestTimeout"], out int timeoutSeconds) && timeoutSeconds > 0)
                {
                    Client.Timeout = Convert.ToInt32(TimeSpan.FromSeconds(timeoutSeconds).TotalMilliseconds);
                }
            }

            /// <summary>
            /// Gets the <see cref="DolarBotApi"/> current culture.
            /// </summary>
            /// <returns>A <see cref="CultureInfo"/> object that represents the API culture.</returns>
            public CultureInfo GetApiCulture() => CultureInfo.GetCultureInfo("en-US");

            /// <summary>
            /// Querys the API and returns its current status.
            /// </summary>
            /// <returns></returns>
            public async Task<HttpStatusCode?> GetApiStatus()
            {
                RestRequest request = new RestRequest(ROOT_ENDPOINT);
                var response = await Client.ExecuteGetAsync(request);
                return response?.StatusCode;
            }

            /// <summary>
            /// Querys an API endpoint asynchronously and returs its result.
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
                    RestRequest request = new RestRequest(endpoint, DataFormat.Json);
                    IRestResponse<DollarResponse> response = await Client.ExecuteGetAsync<DollarResponse>(request).ConfigureAwait(false);
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
            /// Querys an API endpoint asynchronously and returs its result.
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
                    RestRequest request = new RestRequest(endpoint, DataFormat.Json);
                    IRestResponse<EuroResponse> response = await Client.ExecuteGetAsync<EuroResponse>(request).ConfigureAwait(false);
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
            /// Querys an API endpoint asynchronously and returs its result.
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
                    RestRequest request = new RestRequest(endpoint, DataFormat.Json);
                    IRestResponse<RealResponse> response = await Client.ExecuteGetAsync<RealResponse>(request).ConfigureAwait(false);
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
            /// Querys the API endpoint asynchronously and returns a <see cref="CountryRiskResponse"/> object.
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
                    RestRequest request = new RestRequest(RIESGO_PAIS_ENDPOINT, DataFormat.Json);
                    IRestResponse<CountryRiskResponse> response = await Client.ExecuteGetAsync<CountryRiskResponse>(request).ConfigureAwait(false);
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
            /// Querys the API endpoint asynchronously and returns a <see cref="BcraResponse"/> object.
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
                    RestRequest request = new RestRequest(endpoint, DataFormat.Json);
                    IRestResponse<BcraResponse> response = await Client.ExecuteGetAsync<BcraResponse>(request).ConfigureAwait(false);
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
            /// Querys the API endpoint asynchronously and returns a <see cref="MetalResponse"/> object.
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
                    RestRequest request = new RestRequest(endpoint, DataFormat.Json);
                    IRestResponse<MetalResponse> response = await Client.ExecuteGetAsync<MetalResponse>(request).ConfigureAwait(false);
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
            /// Querys the API endpoint asynchronously and returns a <see cref="CryptoResponse"/> object.
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
                    RestRequest request = new RestRequest(endpoint, DataFormat.Json);
                    IRestResponse<CryptoResponse> response = await Client.ExecuteGetAsync<CryptoResponse>(request).ConfigureAwait(false);
                    if (response.IsSuccessful)
                    {
                        CryptoResponse cryptoResponse = response.Data;
                        cryptoResponse.Currency = cryptoCurrency;
                        Cache.SaveObject(cryptoCurrency, cryptoResponse);

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
            /// Querys the API endpoint asynchronously and returns a <see cref="VzlaResponse"/> object.
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
                    RestRequest request = new RestRequest(endpoint, DataFormat.Json);
                    IRestResponse<VzlaResponse> response = await Client.ExecuteGetAsync<VzlaResponse>(request).ConfigureAwait(false);
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
            /// Querys the API endpoint asynchronously and returns a <see cref="HistoricalRatesResponse"/> object.
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
                    RestRequest request = new RestRequest(endpoint, DataFormat.Json);
                    IRestResponse<HistoricalRatesResponse> response = await Client.ExecuteGetAsync<HistoricalRatesResponse>(request).ConfigureAwait(false);
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
}
