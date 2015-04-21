/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Web;

using ASC.Core;
using ASC.Core.Users;
using ASC.MessagingSystem;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Core.Users;

using AjaxPro;

namespace ASC.Web.Studio.Core
{
    [AjaxNamespace("EmailOperationService")]
    public class EmailOperationService
    {
        public class InvalidEmailException : Exception
        {
            public InvalidEmailException()
            {
            }

            public InvalidEmailException(string message) : base(message)
            {
            }
        }

        public class AccessDeniedException : Exception
        {
            public AccessDeniedException()
            {
            }

            public AccessDeniedException(string message) : base(message)
            {
            }
        }

        public class UserNotFoundException : Exception
        {
            public UserNotFoundException()
            {
            }

            public UserNotFoundException(string message) : base(message)
            {
            }
        }

        public class InputException : Exception
        {
            public InputException()
            {
            }

            public InputException(string message) : base(message)
            {
            }
        }

        /// <summary>
        /// Sends the email activation instructions to the specified email
        /// </summary>
        /// <param name="userID">The ID of the user who should activate the email</param>
        /// <param name="email">Email</param>
        [AjaxMethod]
        public string SendEmailActivationInstructions(Guid userID, string email)
        {
            if (userID == Guid.Empty) throw new ArgumentNullException("userID");
            if (String.IsNullOrEmpty(email)) throw new ArgumentNullException(Resources.Resource.ErrorEmailEmpty);
            if (!email.TestEmailRegex()) throw new InvalidEmailException(Resources.Resource.ErrorNotCorrectEmail);

            try
            {
                var viewer = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
                var user = CoreContext.UserManager.GetUsers(userID);

                if (user == null) throw new UserNotFoundException(Resources.Resource.ErrorUserNotFound);

                if (viewer == null) throw new AccessDeniedException(Resources.Resource.ErrorAccessDenied);

                if (viewer.IsAdmin() || viewer.ID == user.ID)
                {
                    var existentUser = CoreContext.UserManager.GetUserByEmail(email);
                    if (existentUser.ID != ASC.Core.Users.Constants.LostUser.ID && existentUser.ID != userID)
                        throw new InputException(CustomNamingPeople.Substitute<Resources.Resource>("ErrorEmailAlreadyExists"));

                    user.Email = email;
                    if (user.ActivationStatus == EmployeeActivationStatus.Activated)
                    {
                        user.ActivationStatus = EmployeeActivationStatus.NotActivated;
                    }
                    CoreContext.UserManager.SaveUserInfo(user);
                }
                else
                {
                    email = user.Email;
                }

                if (user.ActivationStatus == EmployeeActivationStatus.Pending)
                {
                    if (user.IsVisitor())
                    {
                        StudioNotifyService.Instance.GuestInfoActivation(user);
                    }
                    else
                    {
                        StudioNotifyService.Instance.UserInfoActivation(user);
                    }
                }
                else
                {
                    StudioNotifyService.Instance.SendEmailActivationInstructions(user, email);
                }

                MessageService.Send(HttpContext.Current.Request, MessageAction.UserSentActivationInstructions, user.DisplayUserName(false));

                return String.Format(Resources.Resource.MessageEmailActivationInstuctionsSentOnEmail, "<b>" + email + "</b>");
            }
            catch(UserNotFoundException)
            {
                throw;
            }
            catch(AccessDeniedException)
            {
                throw;
            }
            catch(InputException)
            {
                throw;
            }
            catch(Exception)
            {
                throw new Exception(Resources.Resource.UnknownError);
            }
        }

        /// <summary>
        /// Sends the email change instructions to the specified email
        /// </summary>
        /// <param name="userID">The ID of the user who is changing the email</param>
        /// <param name="email">Email</param>
        [AjaxMethod]
        public string SendEmailChangeInstructions(Guid userID, string email)
        {
            if (userID == Guid.Empty)
                throw new ArgumentNullException("userID");

            if (String.IsNullOrEmpty(email))
                throw new Exception(Resources.Resource.ErrorEmailEmpty);

            if (!email.TestEmailRegex())
                throw new Exception(Resources.Resource.ErrorNotCorrectEmail);

            try
            {
                var viewer = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
                var user = CoreContext.UserManager.GetUsers(userID);

                if (user == null)
                    throw new UserNotFoundException(Resources.Resource.ErrorUserNotFound);

                if (viewer == null)
                    throw new AccessDeniedException(Resources.Resource.ErrorAccessDenied);

                var existentUser = CoreContext.UserManager.GetUserByEmail(email);
                if (existentUser.ID != ASC.Core.Users.Constants.LostUser.ID)
                    throw new InputException(CustomNamingPeople.Substitute<Resources.Resource>("ErrorEmailAlreadyExists"));

                if (!viewer.IsAdmin())
                {
                    StudioNotifyService.Instance.SendEmailChangeInstructions(user, email);
                }
                else
                {
                    if (email == user.Email)
                        throw new InputException(Resources.Resource.ErrorEmailsAreTheSame);

                    user.Email = email;
                    user.ActivationStatus = EmployeeActivationStatus.NotActivated;
                    CoreContext.UserManager.SaveUserInfo(user);
                    StudioNotifyService.Instance.SendEmailActivationInstructions(user, email);
                }

                MessageService.Send(HttpContext.Current.Request, MessageAction.UserSentEmailChangeInstructions, user.DisplayUserName(false));

                return String.Format(Resources.Resource.MessageEmailChangeInstuctionsSentOnEmail, "<b>" + email + "</b>");
            }
            catch(AccessDeniedException)
            {
                throw;
            }
            catch(UserNotFoundException)
            {
                throw;
            }
            catch(InputException)
            {
                throw;
            }
            catch(Exception)
            {
                throw new Exception(Resources.Resource.UnknownError);
            }
        }
    }
}