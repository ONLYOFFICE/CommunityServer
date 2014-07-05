/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.IO;
using System.Reflection;
using System.Threading;
using PushSharp;
using PushSharp.Android;
using PushSharp.Apple;
using PushSharp.Core;
using log4net;

namespace ASC.PushService
{
    internal static class PushBrokerProvider
    {
        private static readonly object Lock = new object();
        private static readonly ILog Log = LogManager.GetLogger(typeof(PushBrokerProvider));

        private static DateTime _lastRestartTime = DateTime.MinValue;

        private static PushBroker _instance;
        public static PushBroker Instance
        {
            get
            {
                lock (Lock)
                {
                    return _instance ?? (_instance = CreateInstance());
                }
            }
        }

        private static PushBroker CreateInstance()
        {
            Log.Info("initializing PushBroker instance");

            var config = PushServiceConfiguration.GetSection();

            var pushBroker = new PushBroker();

            SubscribeOnEvents(pushBroker, config);
            RegisterApnsService(pushBroker, config);
            RegisterGcmService(pushBroker, config);

            return pushBroker;
        }

        private static void SubscribeOnEvents(PushBroker pushBroker, PushServiceConfiguration config)
        {
            pushBroker.OnChannelCreated += (sender, channel) => Log.DebugFormat("channel ({0}) created", channel);
            pushBroker.OnChannelDestroyed += sender => Log.Debug("channel destroyed");
            pushBroker.OnChannelException += (sender, channel, exception) => Log.ErrorFormat("channel ({0}) exception: {1}", channel, exception);
            pushBroker.OnServiceException += (sender, error) => Log.Error("service exception", error);
            pushBroker.OnNotificationRequeue += (sender, args) => Log.DebugFormat("notification ({0}) requeue: cancel = {1}", args.Notification, args.Cancel);
            pushBroker.OnNotificationSent += (sender, notification) => Log.DebugFormat("notification ({0}) sent", notification);

            pushBroker.OnNotificationFailed += (sender, notification, error) =>
                {
                    Log.ErrorFormat("notification ({0}) failed: {1}", notification, error);
                    if (error is MaxSendAttemptsReachedException)
                    {
                        if (Monitor.TryEnter(Lock))
                        {
                            try
                            {
                                if (_lastRestartTime + config.RestartInterval < DateTime.UtcNow)
                                {
                                    _instance.StopAllServices(false);
                                    _instance = null;
                                    _lastRestartTime = DateTime.UtcNow;
                                }
                            }
                            catch (Exception restartError)
                            {
                                Log.Error("can't restart service", restartError);
                            }
                            finally
                            {
                                Monitor.Exit(Lock);
                            }
                        }
                    }
                };

            pushBroker.OnDeviceSubscriptionChanged += (sender, id, subscriptionId, notification) =>
                {
                    Log.DebugFormat("device ({0}) subscription changed to {1}. notification is ({2})", id, subscriptionId, notification);
                    new DeviceDao().UpdateToken(id, subscriptionId);
                };

            pushBroker.OnDeviceSubscriptionExpired += (sender, id, utc, notification) =>
                {
                    Log.DebugFormat("device ({0}) subscription expired. notification is ({1}) and utc is ({2})", id, notification, utc);
                    new DeviceDao().Delete(id);
                };
        }

        private static void RegisterApnsService(PushBroker pushBroker, PushServiceConfiguration config)
        {
            if (!config.Apns.ElementInformation.IsPresent)
                return;

            try
            {
                Log.Debug("registering apns service");

                string certPath = config.Apns.CertificatePath;
                if (!Path.IsPathRooted(certPath))
                    certPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), certPath);

                var appleCert = File.ReadAllBytes(certPath);
                pushBroker.RegisterAppleService(
                    new ApplePushChannelSettings(!config.Apns.IsDevelopmentMode, appleCert, config.Apns.CertificatePassword)
                        {
                            FeedbackIntervalMinutes = config.Apns.FeedbackIntervalMinutes
                        });
            }
            catch (Exception error)
            {
                Log.Error("couldn't register apns service", error);
            }
        }

        private static void RegisterGcmService(PushBroker pushBroker, PushServiceConfiguration config)
        {
            if (!config.Gcm.ElementInformation.IsPresent)
                return;

            try
            {
                Log.Debug("registering gcm service");
                pushBroker.RegisterGcmService(new GcmPushChannelSettings(config.Gcm.AuthorizationKey));
            }
            catch (Exception error)
            {
                Log.Error("couldn't register gcm service", error);
            }
        }
    }
}
