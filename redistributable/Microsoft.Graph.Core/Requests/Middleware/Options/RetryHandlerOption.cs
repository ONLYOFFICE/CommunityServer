// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Net.Http;

    /// <summary>
    /// The retry middleware option class
    /// </summary>
    public class RetryHandlerOption : IMiddlewareOption
    {
        internal const int DEFAULT_DELAY = 3;
        internal const int DEFAULT_MAX_RETRY = 3;
        internal const int MAX_MAX_RETRY = 10;
        internal const int MAX_DELAY = 180;

        /// <summary>
        /// Constructs a new <see cref="RetryHandlerOption"/>
        /// </summary>
        public RetryHandlerOption()
        {
        }

        private int _delay = DEFAULT_DELAY;
        /// <summary>
        /// The waiting time in seconds before retrying a request with a maximum value of 180 seconds. This defaults to 3 seconds.
        /// </summary>
        public int Delay
        {
            get { return _delay; }
            set
            {
                if (value > MAX_DELAY)
                {
                    throw new ServiceException(
                        new Error
                        {
                            Code = ErrorConstants.Codes.MaximumValueExceeded,
                            Message = string.Format(ErrorConstants.Messages.MaximumValueExceeded, "Delay", MAX_DELAY)
                        });
                }

                _delay = value;
            }
        }

        private int _maxRetry = DEFAULT_MAX_RETRY;
        /// <summary>
        /// The maximum number of retries for a request with a maximum value of 10. This defaults to 3.
        /// </summary>
        public int MaxRetry
        {
            get
            {
                return _maxRetry;
            }
            set
            {
                if (value > MAX_MAX_RETRY)
                {
                    throw new ServiceException(
                        new Error
                        {
                            Code = ErrorConstants.Codes.MaximumValueExceeded,
                            Message = string.Format(ErrorConstants.Messages.MaximumValueExceeded, "MaxRetry", MAX_MAX_RETRY)
                        });
                }
                _maxRetry = value;
            }
        }

        /// <summary>
        /// The maximum time allowed for request retries.
        /// </summary>
        public TimeSpan RetriesTimeLimit { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// A delegate that's called to determine whether a request should be retried or not.
        /// The delegate method should accept a delay time in seconds of, number of retry attempts and <see cref="HttpResponseMessage"/> as it's parameters and return a <see cref="bool"/>. This defaults to true
        /// </summary>
        public Func<int, int, HttpResponseMessage, bool> ShouldRetry { get; set; } = (delay, attempt, response) => true;
    }
}
