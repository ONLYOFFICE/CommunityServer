using System;

using ASC.Api.Attributes;
using ASC.Api.Impl;
using ASC.Api.Interfaces;
using ASC.Web.Studio.Core;

namespace ASC.Specific.WarmupApi
{
    public class WarmUpEntryPoint : IApiEntryPoint
    {
        /// <summary>
        /// Entry point name
        /// </summary>
        public string Name
        {
            get { return "warmup"; }
        }

        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="context"></param>
        public WarmUpEntryPoint(ApiContext context)
        {
        }

        /// <summary>
        /// Request of warmup progress
        /// </summary>
        /// <visible>false</visible>
        [Read(@"progress", false, false)] //NOTE: this method doesn't requires auth!!!
        public string GetWarmupProgress()
        {
            try
            {
                return WarmUp.Instance.GetSerializedProgress();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <visible>false</visible>
        [Read(@"restart", false, false)] //NOTE: this method doesn't requires auth!!!
        public string Restart()
        {
            try
            {
                WarmUp.Instance.Restart();
                return "Ok";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
