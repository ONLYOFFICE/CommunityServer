using System;
using System.Extensions;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenStack.Compute.v2_1.Serialization;
using OpenStack.Images.v2;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1
{
    /// <summary>
    /// A collection of files for a specific operating system (OS) that you use to create or rebuild a server.
    /// </summary>
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "image")]
    public class Image : ImageSummary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Image"/> class.
        /// </summary>
        public Image()
        {
            Metadata = new ImageMetadata();
        }

        /// <summary>
        /// The date and time when the resource was created.
        /// </summary>
        [JsonProperty("created")]
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// The date and time when the resource was updated.
        /// </summary>
        [JsonProperty("updated")]
        public DateTimeOffset Updated { get; set; }

        /// <summary>
        /// The minimum disk size in GB that is required to boot the image.
        /// </summary>
        [JsonProperty("minDisk")]
        public Int64 MinimumDiskSize { get; set; }

        /// <summary>
        /// The minimum amount of RAM in MB that is required to boot the image.
        /// </summary>
        [JsonProperty("minRam")]
        public Int64 MinimumMemorySize { get; set; }

        /// <summary>
        /// The size of the image data, in bytes.
        /// </summary>
        [JsonProperty("OS-EXT-IMG-SIZE:size")]
        public Int64? Size { get; set; }

        /// <summary>
        /// The build completion progress, as a percentage.
        /// </summary>
        [JsonProperty("progress")]
        public int Progress { get; set; }

        /// <summary>
        /// The image status.
        /// </summary>
        [JsonProperty("status")]
        public ImageStatus Status { get; set; }

        /// <summary>
        /// The associated server.
        /// </summary>
        [JsonProperty("server")]
        public ServerReference Server { get; set; }

        /// <summary>
        /// Metadata key pairs containing information about the image.
        /// </summary>
        [JsonProperty("metadata")]
        public ImageMetadata Metadata { get; set; }

        /// <summary>
        /// Indicates whether the image is built-in (base) or custom (snapshot).
        /// </summary>
        [JsonIgnore]
        public ImageType Type
        {
            get
            {
                string type;
                if (Metadata != null && Metadata.TryGetValue("image_type", out type))
                    return StringEnumeration.FromDisplayName<ImageType>(type);

                return ImageType.Base;
            }
        }

        /// <inheritdoc cref="ComputeApi.WaitForImageStatusAsync{TImage,TStatus}" />
        /// <exception cref="InvalidOperationException">When this instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public async Task WaitForStatus(ImageStatus status, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var owner = this.GetOwnerOrThrow<ComputeApi>();
            var result = await owner.WaitForImageStatusAsync<Image, ImageStatus>(Id, status, refreshDelay, timeout, progress, cancellationToken).ConfigureAwait(false);
            result.CopyProperties(this);
        }

        /// <summary>
        /// Wait until the image is active.
        /// </summary>
        /// <exception cref="InvalidOperationException">When this instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public Task WaitUntilActiveAsync(TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return WaitForStatus(ImageStatus.Active, refreshDelay, timeout, progress, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.WaitUntilImageIsDeletedAsync{TImage,TStatus}" />
        /// <exception cref="InvalidOperationException">When this instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public override async Task WaitUntilDeletedAsync(TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.WaitUntilDeletedAsync(refreshDelay, timeout, progress, cancellationToken).ConfigureAwait(false);
            Status = ImageStatus.Deleted;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            Metadata.SetParent(this);
        }
    }
}
