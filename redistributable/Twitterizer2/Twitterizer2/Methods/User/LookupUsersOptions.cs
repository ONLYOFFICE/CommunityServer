namespace Twitterizer
{
    using System.Collections.ObjectModel;

    /// <summary>
    /// Provides optional parameters for user lookup methods.
    /// </summary>
    public class LookupUsersOptions : OptionalProperties
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LookupUsersOptions"/> class.
        /// </summary>
        public LookupUsersOptions()
        {
            this.ScreenNames = new Collection<string>();
            this.UserIds = new TwitterIdCollection();
        }

        /// <summary>
        /// Gets or sets the screen names.
        /// </summary>
        /// <value>The screen names.</value>
        public Collection<string> ScreenNames { get; set; }

        /// <summary>
        /// Gets or sets the user ids.
        /// </summary>
        /// <value>The user ids.</value>
        public TwitterIdCollection UserIds { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [include entities].
        /// </summary>
        /// <value><c>true</c> if [include entities]; otherwise, <c>false</c>.</value>
        public bool IncludeEntities { get; set; }
    }
}
