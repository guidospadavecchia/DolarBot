using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace DolarBot.Modules.Services.Quotes
{
    /// <summary>
    /// Contains methods for retrieving famous argentine dollar-related quotes.
    /// </summary>
    public static class QuoteService
    {
        #region Constants
        private const string QUOTES_FILE_PATH = "Resources/quotes.json";
        #endregion

        #region Vars

        /// <summary>
        /// Random generator.
        /// </summary>
        private static readonly Random random = new Random();

        /// <summary>
        /// A collection of famous quotes.
        /// </summary>
        private static List<Quote> Quotes;

        #endregion

        #region Methods

        /// <summary>
        /// Loads the quotes into memory.
        /// </summary>
        public static bool TryLoadQuotes()
        {
            try
            {
                string text = File.ReadAllText(QUOTES_FILE_PATH);
                Quotes = JsonConvert.DeserializeObject<List<Quote>>(text);

                return Quotes != null && Quotes.Count > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Retrieves a random quote.
        /// </summary>
        /// <returns>A random <see cref="Quote"/>, or null of no quotes were found.</returns>
        public static Quote GetRandomQuote()
        {
            if (Quotes == null)
            {
                TryLoadQuotes();
            }

            if (Quotes.Count > 0)
            {
                int randomIndex = random.Next(0, Quotes.Count);
                return Quotes[randomIndex];
            }
            else
            {
                return null;
            }
        }

        #endregion

    }
}
