namespace DolarBot.Services.Banking.Interfaces
{
    /// <summary>
    /// Provides a contract for currency related services.
    /// </summary>
    public interface IBankCurrencyService
    {
        /// <summary>
        /// Returns a collection of valid banks for the currency.
        /// </summary>
        /// <returns>Collection of valid banks.</returns>
        public Banks[] GetValidBanks();
    }
}
