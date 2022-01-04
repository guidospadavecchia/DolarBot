﻿using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using DolarBot.Util;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmbedPage = Discord.Addons.Interactive.PaginatedMessage.Page;

namespace DolarBot.Modules.Commands.Base
{
    /// <summary>
    /// Base class for interactive modules.
    /// </summary>
    public class BaseModule : InteractiveBase<SocketCommandContext>
    {
        #region Constants
        protected const string REQUEST_ERROR_MESSAGE = "**Error**: No se pudo obtener la cotización ya que el servicio se encuentra inaccesible. Intente nuevamente en más tarde.";
        #endregion

        #region Vars
        /// <summary>
        /// Provides access to application settings.
        /// </summary>
        protected readonly IConfiguration Configuration;
        /// <summary>
        /// The log4net logger.
        /// </summary>
        protected readonly ILog Logger;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes the object using the <see cref="IConfiguration"/> object.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        public BaseModule(IConfiguration configuration, ILog logger)
        {
            Configuration = configuration;
            Logger = logger;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Sends a reply indicating an error has occurred.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        protected async Task SendErrorReply(Exception ex)
        {
            await ReplyAsync(GlobalConfiguration.GetGenericErrorMessage(Configuration["supportServerUrl"]));
            Logger.Error("Error al ejecutar comando.", ex);
        }

        /// <summary>
        /// Sends a paged embed reply with default reactions.
        /// </summary>
        /// <param name="pages">The embed pages.</param>
        /// <param name="includeFirstLast">Indicates wether to include reactions for first and last page.</param>
        protected async Task SendPagedReplyAsync(IEnumerable<PaginatedMessage.Page> pages, bool includeFirstLast = false)
        {
            await PagedReplyAsync(new PaginatedMessage { Pages = pages }, new ReactionList { Forward = pages.Count() > 1, Backward = pages.Count() > 1, First = pages.Count() > 2 && includeFirstLast, Last = pages.Count() > 2 && includeFirstLast, Info = false, Jump = false, Trash = false });
        }

        /// <summary>
        /// Sends a paged embed reply with default reactions, from a collection of <see cref="EmbedBuilder"/> objects.
        /// </summary>
        /// <param name="embeds">A collection of embeds, representing a page each.</param>
        /// <param name="includeFirstLast">Indicates wether to include reactions for first and last page.</param>
        /// <returns></returns>
        protected async Task SendPagedReplyAsync(IEnumerable<EmbedBuilder> embeds, bool includeFirstLast = false)
        {
            List<EmbedPage> pages = new();
            foreach (EmbedBuilder embed in embeds)
            {
                pages.Add(new EmbedPage
                {
                    Title = embed.Title,
                    Description = embed.Description,
                    Fields = embed.Fields,
                    Color = embed.Color,
                    FooterOverride = embed.Footer,
                    ThumbnailUrl = embed.ThumbnailUrl,
                });
            }
            await SendPagedReplyAsync(pages, includeFirstLast);
        }

        #endregion
    }
}