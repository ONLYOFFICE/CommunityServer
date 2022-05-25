namespace net.openstack.Core.Domain
{
    using System;
    using net.openstack.Core.Exceptions;
    using net.openstack.Core.Exceptions.Response;
    using net.openstack.Core.Providers;
    using Newtonsoft.Json;

    /// <summary>
    /// Provides basic information about an image.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Images-d1e4427.html">Images (OpenStack Compute API v2 and Extensions Reference - API v2)</seealso>
    /// <seealso href="http://docs.rackspace.com/servers/api/v2/cs-devguide/content/Images-d1e4427.html">Images (Rackspace Next Generation Cloud Servers Developer Guide  - API v2)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    public class SimpleServerImage : ProviderStateBase<IComputeProvider>
    {
        /// <summary>
        /// Gets the unique identifier for the image.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        [JsonProperty]
        public string Id { get; private set; }

        /// <summary>
        /// Gets a collection of links related to the current image.
        /// </summary>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/LinksReferences.html">Links and References (OpenStack Compute API v2 and Extensions Reference - API v2)</seealso>
        [JsonProperty]
        public Link[] Links { get; private set; }

        /// <summary>
        /// Gets the name of the image.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        [JsonProperty]
        public string Name { get; private set; }

        /// <summary>
        /// Waits for the image to enter the <see cref="ImageState.Active"/> state.
        /// </summary>
        /// <remarks>
        /// When the method returns, the current instance is updated to reflect the state
        /// of the image at the end of the operation.
        ///
        /// <note type="caller">
        /// This is a blocking operation and will not return until the image enters either the
        /// <see cref="ImageState.Active"/> state, an error state, or the retry count is exceeded.
        /// </note>
        /// </remarks>
        /// <param name="refreshCount">Number of times to poll the image's status.</param>
        /// <param name="refreshDelay">The time to wait between polling requests for the image status. If this value is <see langword="null"/>, the default is 2.4 seconds.</param>
        /// <param name="progressUpdatedCallback">A callback delegate to execute each time the <see cref="ServerImage.Progress"/> value increases. If this value is <see langword="null"/>, progress updates are not reported.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="refreshCount"/> is less than 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="refreshDelay"/> is negative.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso cref="IComputeProvider.WaitForImageActive"/>
        public void WaitForActive(int refreshCount = 600, TimeSpan? refreshDelay = null, Action<int> progressUpdatedCallback = null)
        {
            if (refreshCount < 0)
                throw new ArgumentOutOfRangeException("refreshCount");
            if (refreshDelay < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("refreshDelay");

            var details = Provider.WaitForImageActive(Id, refreshCount, refreshDelay, progressUpdatedCallback, Region, Identity);
            UpdateThis(details);
        }

        /// <summary>
        /// Waits for the image to enter the <see cref="ImageState.Deleted"/> state or to be removed.
        /// </summary>
        /// <remarks>
        /// <note type="warning">
        /// This is a blocking operation and will not return until the image enters either the
        /// <see cref="ImageState.Deleted"/> state, an error state, is removed, or the retry count is exceeded.
        /// </note>
        /// </remarks>
        /// <param name="refreshCount">Number of times to poll the image's status.</param>
        /// <param name="refreshDelay">The time to wait between polling requests for the image status. If this value is <see langword="null"/>, the default is 2.4 seconds.</param>
        /// <param name="progressUpdatedCallback">A callback delegate to execute each time the <see cref="ServerImage.Progress"/> value increases. If this value is <see langword="null"/>, progress updates are not reported.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="refreshCount"/> is less than 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="refreshDelay"/> is negative.</para>
        /// </exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso cref="IComputeProvider.WaitForImageDeleted"/>
        public void WaitForDelete(int refreshCount = 600, TimeSpan? refreshDelay = null, Action<int> progressUpdatedCallback = null)
        {
            if (refreshCount < 0)
                throw new ArgumentOutOfRangeException("refreshCount");
            if (refreshDelay < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("refreshDelay");

            Provider.WaitForImageDeleted(Id, refreshCount, refreshDelay, progressUpdatedCallback, Region, Identity);
        }

        /// <summary>
        /// Waits for the image to enter a specified state.
        /// </summary>
        /// <remarks>
        /// When the method returns, the current instance is updated to reflect the state
        /// of the image at the end of the operation.
        ///
        /// <note type="caller">
        /// This is a blocking operation and will not return until the image enters either the expected state, an error state, or the retry count is exceeded.
        /// </note>
        /// </remarks>
        /// <param name="expectedState">The expected state.</param>
        /// <param name="errorStates">The error state(s) in which to throw an exception if the image enters.</param>
        /// <param name="refreshCount">Number of times to poll the image's status.</param>
        /// <param name="refreshDelay">The time to wait between polling requests for the image status. If this value is <see langword="null"/>, the default is 2.4 seconds.</param>
        /// <param name="progressUpdatedCallback">A callback delegate to execute each time the <see cref="ServerImage.Progress"/> value increases. If this value is <see langword="null"/>, progress updates are not reported.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="expectedState"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="errorStates"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="refreshCount"/> is less than 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="refreshDelay"/> is negative.</para>
        /// </exception>
        /// <exception cref="ImageEnteredErrorStateException">If the method returned due to the image entering one of the <paramref name="errorStates"/>.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso cref="IComputeProvider.WaitForImageState(string, ImageState, ImageState[], int, TimeSpan?, Action{int}, string, CloudIdentity)"/>
        public void WaitForState(ImageState expectedState, ImageState[] errorStates, int refreshCount = 600, TimeSpan? refreshDelay = null, Action<int> progressUpdatedCallback = null)
        {
            if (expectedState == null)
                throw new ArgumentNullException("expectedState");
            if (errorStates == null)
                throw new ArgumentNullException("errorStates");
            if (refreshCount < 0)
                throw new ArgumentOutOfRangeException("refreshCount");
            if (refreshDelay < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("refreshDelay");

            var details = Provider.WaitForImageState(Id, expectedState, errorStates, refreshCount, refreshDelay, progressUpdatedCallback, Region, Identity);
            UpdateThis(details);
        }

        /// <summary>
        /// Waits for the image to enter any one of a set of specified states.
        /// </summary>
        /// <remarks>
        /// When the method returns, the current instance is updated to reflect the state
        /// of the image at the end of the operation.
        ///
        /// <note type="caller">
        /// This is a blocking operation and will not return until the image enters either an expected state, an error state, or the retry count is exceeded.
        /// </note>
        /// </remarks>
        /// <param name="expectedStates">The expected state(s).</param>
        /// <param name="errorStates">The error state(s) in which to throw an exception if the image enters.</param>
        /// <param name="refreshCount">Number of times to poll the image's status.</param>
        /// <param name="refreshDelay">The time to wait between polling requests for the image status. If this value is <see langword="null"/>, the default is 2.4 seconds.</param>
        /// <param name="progressUpdatedCallback">A callback delegate to execute each time the <see cref="ServerImage.Progress"/> value increases. If this value is <see langword="null"/>, progress updates are not reported.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="expectedStates"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="errorStates"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">If <paramref name="expectedStates"/> is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="refreshCount"/> is less than 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="refreshDelay"/> is negative.</para>
        /// </exception>
        /// <exception cref="ImageEnteredErrorStateException">If the method returned due to the image entering one of the <paramref name="errorStates"/>.</exception>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso cref="IComputeProvider.WaitForImageState(string, ImageState[], ImageState[], int, TimeSpan?, Action{int}, string, CloudIdentity)"/>
        public void WaitForState(ImageState[] expectedStates, ImageState[] errorStates, int refreshCount = 600, TimeSpan? refreshDelay = null, Action<int> progressUpdatedCallback = null)
        {
            if (expectedStates == null)
                throw new ArgumentNullException("expectedStates");
            if (errorStates == null)
                throw new ArgumentNullException("errorStates");
            if (expectedStates.Length == 0)
                throw new ArgumentException("expectedStates cannot be empty");
            if (refreshCount < 0)
                throw new ArgumentOutOfRangeException("refreshCount");
            if (refreshDelay < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("refreshDelay");

            var details = Provider.WaitForImageState(Id, expectedStates, errorStates, refreshCount, refreshDelay, progressUpdatedCallback, Region, Identity);
            UpdateThis(details);
        }

        /// <summary>
        /// Updates the current instance to match the information in <paramref name="serverImage"/>.
        /// </summary>
        /// <remarks>
        /// <note type="implement">
        /// This method should be overridden in derived types to ensure all properties
        /// for the current instance are updated.
        /// </note>
        /// </remarks>
        /// <param name="serverImage">The updated information for the current image.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="serverImage"/> is <see langword="null"/>.</exception>
        protected virtual void UpdateThis(SimpleServerImage serverImage)
        {
            if (serverImage == null)
                throw new ArgumentNullException("serverImage");

            Id = serverImage.Id;
            Links = serverImage.Links;
            Name = serverImage.Name;
        }

        /// <summary>
        /// Deletes the specified image.
        /// </summary>
        /// <returns><see langword="true"/> if the image was successfully deleted; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ResponseException">If the REST API request failed.</exception>
        /// <seealso cref="IComputeProvider.DeleteImage"/>
        /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Delete_Image-d1e4957.html">Delete Image (OpenStack Compute API v2 and Extensions Reference)</seealso>
        public bool Delete()
        {
            return Provider.DeleteImage(Id, Region, Identity);
        }
    }
}
