namespace Twitterizer.Commands
{
    [Core.AuthorizedCommand]
    internal class DestroyListSubscriber : Core.TwitterCommand<TwitterList>
    {
        /// <summary>
        /// Gets or sets the list id.
        /// </summary>
        /// <value>The list id.</value>
        /// <remarks></remarks>
        public decimal ListId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DestroyListSubscriber"/> class.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="listId">The list id.</param>
        /// <param name="options">The options.</param>
        /// <remarks></remarks>
        public DestroyListSubscriber(OAuthTokens tokens, decimal listId, OptionalProperties options)
            : base(HTTPVerb.POST, "lists/subscribers/destroy.json", tokens, options)
        {
            this.ListId = listId;
        }

        /// <summary>
        /// Inits this instance.
        /// </summary>
        /// <remarks></remarks>
        public override void Init()
        {
            this.RequestParameters.Add("list_id", this.ListId.ToString("#"));
        }
    }
}
