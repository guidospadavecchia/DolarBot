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
        public DolarArgentinaApi DolarArgentina { get; private set; }
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
            DolarArgentina = new DolarArgentinaApi(configuration, cache, LogError);
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
        public class DolarArgentinaApi
        {
            #region Constants
            //Dólar
            private const string DOLAR_OFICIAL_ENDPOINT = "/api/dolaroficial";
            private const string DOLAR_BLUE_ENDPOINT = "/api/dolarblue";
            private const string DOLAR_CONTADO_LIQUI_ENDPOINT = "/api/contadoliqui";
            private const string DOLAR_PROMEDIO_ENDPOINT = "/api/dolarpromedio";
            private const string DOLAR_BOLSA_ENDPOINT = "/api/dolarbolsa";
            private const string DOLAR_NACION_ENDPOINT = "/api/nacion";
            private const string DOLAR_BBVA_ENDPOINT = "/api/bbva";
            private const string DOLAR_PIANO_ENDPOINT = "/api/piano";
            private const string DOLAR_HIPOTECARIO_ENDPOINT = "/api/hipotecario";
            private const string DOLAR_GALICIA_ENDPOINT = "/api/galicia";
            private const string DOLAR_SANTANDER_ENDPOINT = "/api/santander";
            private const string DOLAR_CIUDAD_ENDPOINT = "/api/ciudad";
            private const string DOLAR_SUPERVIELLE_ENDPOINT = "/api/supervielle";
            private const string DOLAR_PATAGONIA_ENDPOINT = "/api/patagonia";
            private const string DOLAR_COMAFI_ENDPOINT = "/api/comafi";
            private const string DOLAR_BIND_ENDPOINT = "/api/bind";
            private const string DOLAR_BANCOR_ENDPOINT = "/api/bancor";
            private const string DOLAR_CHACO_ENDPOINT = "/api/chaco";
            private const string DOLAR_PAMPA_ENDPOINT = "/api/pampa";

            //Euro
            private const string EURO_NACION_ENDPOINT = "/api/euro/nacion";
            private const string EURO_GALICIA_ENDPOINT = "/api/euro/galicia";
            private const string EURO_BBVA_ENDPOINT = "/api/euro/bbva";
            private const string EURO_HIPOTECARIO_ENDPOINT = "/api/euro/hipotecario";
            private const string EURO_CHACO_ENDPOINT = "/api/euro/chaco";
            private const string EURO_PAMPA_ENDPOINT = "/api/euro/pampa";

            //Real
            private const string REAL_NACION_ENDPOINT = "/api/real/nacion";
            private const string REAL_BBVA_ENDPOINT = "/api/real/bbva";
            private const string REAL_CHACO_ENDPOINT = "/api/real/chaco";

            //BCRA
            private const string RESERVAS_ENDPOINT = "/api/bcra/reservas";
            private const string CIRCULANTE_ENDPOINT = "/api/bcra/circulante";
            private const string RIESGO_PAIS_ENDPOINT = "/api/riesgopais";
            private const string RIESGO_PAIS_CACHE_KEY = "RiesgoPais";

            //Historical Rates
            private const string EVOLUCION_DOLAR_OFICIAL_ENDPOINT = "/api/evolucion/dolaroficial";
            private const string EVOLUCION_DOLAR_BLUE_ENDPOINT = "/api/evolucion/dolarblue";
            private const string EVOLUCION_EURO_OFICIAL_ENDPOINT = "/api/evolucion/eurooficial";
            private const string EVOLUCION_REAL_OFICIAL_ENDPOINT = "/api/evolucion/realoficial";
            #endregion

            #region Vars
            /// <summary>
            /// An object to execute REST calls to the API.
            /// </summary>
            private readonly RestClient client;
            
            /// <summary>
            /// Allows access to application settings.
            /// </summary>
            private readonly IConfiguration configuration;

            /// <summary>
            /// A cache of in-memory objects.
            /// </summary>
            private readonly ResponseCache cache;

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
                [Description(DOLAR_OFICIAL_ENDPOINT)]
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
            /// Represents the different API endpoints for historical rates.
            /// </summary>
            public enum HistoricalRatesParams
            {
                [Description(EVOLUCION_DOLAR_OFICIAL_ENDPOINT)]
                Dolar,
                [Description(EVOLUCION_DOLAR_BLUE_ENDPOINT)]
                DolarBlue,
                [Description(EVOLUCION_EURO_OFICIAL_ENDPOINT)]
                Euro,
                [Description(EVOLUCION_REAL_OFICIAL_ENDPOINT)]
                Real
            }
            #endregion

            /// <summary>
            /// Creats a <see cref="DolarArgentinaApi"/> object using the provided configuration, cache and error action.
            /// </summary>
            /// <param name="configuration">An <see cref="IConfiguration"/> object to access application settings.</param>
            /// <param name="cache">A cache of in-memory objects.</param>
            /// <param name="onError">An action to execute in case of error.</param>
            internal DolarArgentinaApi(IConfiguration configuration, ResponseCache cache, Action<IRestResponse> onError)
            {
                this.configuration = configuration;
                this.cache = cache;
                OnError = onError;

                client = new RestClient(this.configuration["apiUrl"]);
                client.UseNewtonsoftJson();
            }

            /// <summary>
            /// Gets the <see cref="DolarArgentinaApi"/> current culture.
            /// </summary>
            /// <returns>A <see cref="CultureInfo"/> object that represents the API culture.</returns>
            public CultureInfo GetApiCulture() => CultureInfo.GetCultureInfo("en-US");

            /// <summary>
            /// Querys an API endpoint asynchronously and returs its result.
            /// </summary>
            /// <param name="type">The type of dollar (endpoint) to query.</param>
            /// <returns>A task that contains a normalized <see cref="DolarResponse"/> object.</returns>
            public async Task<DolarResponse> GetDollarPrice(DollarTypes type)
            {
                DolarResponse cachedResponse = cache.GetObject<DolarResponse>(type);
                if (cachedResponse != null)
                {
                    return cachedResponse;
                }
                else
                {
                    string endpoint = type.GetDescription();
                    RestRequest request = new RestRequest(endpoint, DataFormat.Json);
                    IRestResponse<DolarResponse> response = await client.ExecuteGetAsync<DolarResponse>(request).ConfigureAwait(false);
                    if (response.IsSuccessful)
                    {
                        DolarResponse dolarResponse = response.Data;
                        dolarResponse.Type = type;
                        cache.SaveObject(type, dolarResponse);

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
            /// Querys an API endpoint asynchronously and returs its result.
            /// </summary>
            /// <param name="type">The type of euro (endpoint) to query.</param>
            /// <returns>A task that contains a normalized <see cref="EuroResponse"/> object.</returns>
            public async Task<EuroResponse> GetEuroPrice(EuroTypes type)
            {
                EuroResponse cachedResponse = cache.GetObject<EuroResponse>(type);
                if (cachedResponse != null)
                {
                    return cachedResponse;
                }
                else
                {
                    string endpoint = type.GetDescription();
                    RestRequest request = new RestRequest(endpoint, DataFormat.Json);
                    IRestResponse<EuroResponse> response = await client.ExecuteGetAsync<EuroResponse>(request).ConfigureAwait(false);
                    if (response.IsSuccessful)
                    {
                        EuroResponse euroResponse = response.Data;
                        euroResponse.Type = type;
                        cache.SaveObject(type, euroResponse);
                        
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
            /// Querys an API endpoint asynchronously and returs its result.
            /// </summary>
            /// <param name="type">The type of Real (endpoint) to query.</param>
            /// <returns>A task that contains a normalized <see cref="RealResponse"/> object.</returns>
            public async Task<RealResponse> GetRealPrice(RealTypes type)
            {
                RealResponse cachedResponse = cache.GetObject<RealResponse>(type);
                if (cachedResponse != null)
                {
                    return cachedResponse;
                }
                else
                {
                    string endpoint = type.GetDescription();
                    RestRequest request = new RestRequest(endpoint, DataFormat.Json);
                    IRestResponse<RealResponse> response = await client.ExecuteGetAsync<RealResponse>(request).ConfigureAwait(false);
                    if (response.IsSuccessful)
                    {
                        RealResponse realResponse = response.Data;
                        realResponse.Type = type;
                        cache.SaveObject(type, realResponse);
                        
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
            /// Querys the API endpoint asynchronously and returns a <see cref="RiesgoPaisResponse"/> object.
            /// </summary>
            /// <returns>A task that contains a normalized <see cref="RiesgoPaisResponse"/> object.</returns>
            public async Task<RiesgoPaisResponse> GetRiesgoPais()
            {
                RiesgoPaisResponse cachedResponse = cache.GetObject<RiesgoPaisResponse>(RIESGO_PAIS_CACHE_KEY);
                if (cachedResponse != null)
                {
                    return cachedResponse;
                }
                else
                {
                    RestRequest request = new RestRequest(RIESGO_PAIS_ENDPOINT, DataFormat.Json);
                    IRestResponse<RiesgoPaisResponse> response = await client.ExecuteGetAsync<RiesgoPaisResponse>(request).ConfigureAwait(false);
                    if (response.IsSuccessful)
                    {
                        cache.SaveObject(RIESGO_PAIS_CACHE_KEY, response.Data);
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
                BcraResponse cachedResponse = cache.GetObject<BcraResponse>(bcraValue);
                if (cachedResponse != null)
                {
                    return cachedResponse;
                }
                else
                {
                    string endpoint = bcraValue.GetDescription();
                    RestRequest request = new RestRequest(endpoint, DataFormat.Json);
                    IRestResponse<BcraResponse> response = await client.ExecuteGetAsync<BcraResponse>(request).ConfigureAwait(false);
                    if (response.IsSuccessful)
                    {
                        cache.SaveObject(bcraValue, response.Data);
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
            /// Querys the API endpoint asynchronously and returns a <see cref="HistoricalRatesResponse"/> object.
            /// </summary>
            /// <returns>A task that contains a normalized <see cref="HistoricalRatesResponse"/> object.</returns>
            public async Task<HistoricalRatesResponse> GetHistoricalRates(HistoricalRatesParams historicalRatesParam)
            {
                HistoricalRatesResponse cachedResponse = cache.GetObject<HistoricalRatesResponse>(historicalRatesParam);
                if (cachedResponse != null)
                {
                    return cachedResponse;
                }
                else
                {
                    string endpoint = historicalRatesParam.GetDescription();
                    RestRequest request = new RestRequest(endpoint, DataFormat.Json);
                    IRestResponse<HistoricalRatesResponse> response = await client.ExecuteGetAsync<HistoricalRatesResponse>(request).ConfigureAwait(false);
                    if (response.IsSuccessful)
                    {
                        cache.SaveObject(historicalRatesParam, response.Data);
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
