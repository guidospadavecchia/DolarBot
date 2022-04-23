using DolarBot.API.Attributes;
using System.ComponentModel;

namespace DolarBot.API.Enums
{
    /// <summary>
    /// Represents the different API endpoints for cryptocurrencies rates.
    /// </summary>
    public enum CryptoCurrencies
    {
        [Description("/api/crypto/binancecoin")]
        [CryptoCode("binancecoin", "BNB")]
        BinanceCoin,
        [Description("/api/crypto/bitcoin")]
        [CryptoCode("bitcoin", "BTC")]
        Bitcoin,
        [Description("/api/crypto/bitcoincash")]
        [CryptoCode("bitcoin-cash", "BCH")]
        BitcoinCash,
        [Description("/api/crypto/cardano")]
        [CryptoCode("cardano", "ADA")]
        Cardano,
        [Description("/api/crypto/chainlink")]
        [CryptoCode("chainlink", "LINK")]
        Chainlink,
        [Description("/api/crypto/dai")]
        [CryptoCode("dai", "DAI")]
        DAI,
        [Description("/api/crypto/dash")]
        [CryptoCode("dash", "DASH")]
        Dash,
        [Description("/api/crypto/dogecoin")]
        [CryptoCode("dogecoin", "DOGE")]
        DogeCoin,
        [Description("/api/crypto/ethereum")]
        [CryptoCode("ethereum", "ETH")]
        Ethereum,
        [Description("/api/crypto/monero")]
        [CryptoCode("monero", "XMR")]
        Monero,
        [Description("/api/crypto/litecoin")]
        [CryptoCode("litecoin", "LTC")]
        Litecoin,
        [Description("/api/crypto/polkadot")]
        [CryptoCode("polkadot", "DOT")]
        Polkadot,
        [Description("/api/crypto/ripple")]
        [CryptoCode("ripple", "XRP")]
        Ripple,
        [Description("/api/crypto/stellar")]
        [CryptoCode("stellar", "XLM")]
        Stellar,
        [Description("/api/crypto/tether")]
        [CryptoCode("tether", "USDT")]
        Tether,
        [Description("/api/crypto/theta-token")]
        [CryptoCode("theta-token", "THETA")]
        Theta,
        [Description("/api/crypto/uniswap")]
        [CryptoCode("uniswap", "UNI")]
        Uniswap
    }
}
