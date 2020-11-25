using System.ComponentModel;

namespace DolarBot.Modules.Services.Banking
{
    /// <summary>
    /// Represents the available bank parameters for dollar command.
    /// </summary>
    public enum Banks
    {
        [Description("Todos los bancos")]
        Bancos,
        [Description("Banco Nación")]
        Nacion,
        [Description("Banco BBVA")]
        BBVA,
        [Description("Banco Piano")]
        Piano,
        [Description("Banco Hipotecario")]
        Hipotecario,
        [Description("Banco Galicia")]
        Galicia,
        [Description("Banco Santander")]
        Santander,
        [Description("Banco Ciudad")]
        Ciudad,
        [Description("Banco Supervielle")]
        Supervielle,
        [Description("Banco Patagonia")]
        Patagonia,
        [Description("Banco Comafi")]
        Comafi,
        [Description("Banco Industrial")]
        BIND,
        [Description("Banco de Córdoba")]
        Bancor,
        [Description("NBC (Chaco)")]
        Chaco,
        [Description("Banco de La Pampa")]
        Pampa
    }
}
