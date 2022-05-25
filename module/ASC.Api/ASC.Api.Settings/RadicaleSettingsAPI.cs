using System;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using ASC.Api.Attributes;
using ASC.Common.Radicale;
using ASC.Common.Radicale.Core;
using ASC.Core;
using ASC.Core.Users;
using ASC.Security.Cryptography;
using ASC.Web.Core;
using ASC.Web.Studio.PublicResources;

namespace ASC.Api.Settings
{
    public partial class SettingsApi
    {

        /// <summary>
        /// Creates a CardDav address book for a user with all portal users and returns a link to this address book.
        /// </summary>
        /// <short>
        /// Get a link to the CardDav address book
        /// </short>
        /// <category>CardDav address book</category>
        /// <visible>false</visible>
        [Read("carddavurl")]
        public async Task<DavResponse> GetCardDavUrl()
        {

            if (WebItemManager.Instance[WebItemManager.PeopleProductID].IsDisabled())
            {
                await DeleteCardDavAddressBook().ConfigureAwait(false);
                throw new MethodAccessException("Method not available");
            }

            var myUri = HttpContext.Current.Request.GetUrlRewriter();
            var currUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
            var userName = currUser.Email.ToLower();
            var currentAccountPaswd = InstanceCrypto.Encrypt(userName);
            var cardBuilder = CardDavAllSerialization(myUri);


            var cardDavAddBook = new CardDavAddressbook();
            var userAuthorization = userName + ":" + currentAccountPaswd;
            var rootAuthorization = cardDavAddBook.GetSystemAuthorization();
            var sharedCardUrl = cardDavAddBook.GetRadicaleUrl(myUri.ToString(), userName, true, true, true);
            var getResponse = cardDavAddBook.GetCollection(sharedCardUrl, userAuthorization, myUri.ToString()).Result;
            if (getResponse.Completed)
            {
                return new DavResponse()
                {
                    Completed = true,
                    Data = sharedCardUrl
                };
            }
            else if (getResponse.StatusCode == 404)
            {
                var cardAB = new CardDavAddressbook();
                var createResponse = cardAB.Create("", "", "", sharedCardUrl, rootAuthorization).Result;
                if (createResponse.Completed)
                {
                    try
                    {
                        var dbConn = new DbRadicale();
                        dbConn.SaveCardDavUser(CurrentTenant, currUser.ID.ToString());
                    }
                    catch (Exception ex)
                    {
                        Log.Error("ERROR: " + ex.Message);
                    }

                    await cardAB.UpdateItem(sharedCardUrl, rootAuthorization, cardBuilder, myUri.ToString()).ConfigureAwait(false);
                    return new DavResponse()
                    {
                        Completed = true,
                        Data = sharedCardUrl
                    };
                }
                Log.Error("ERROR: " + createResponse.Error);
                throw new RadicaleException(createResponse.Error);
            }
            else
            {
                Log.Error("ERROR: " + getResponse.Error);
                throw new RadicaleException(getResponse.Error);
            }

        }


        /// <summary>
        /// Deletes a CardDav address book with all portal users.
        /// </summary>
        /// <short>
        /// Delete a CardDav address book
        /// </short>
        /// <category>CardDav address book</category>
        /// <visible>false</visible>
        [Delete("deletebook")]
        public async Task<DavResponse> DeleteCardDavAddressBook()
        {
            var currUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
            var currentUserEmail = currUser.Email;
            var cardDavAB = new CardDavAddressbook();
            var authorization = cardDavAB.GetSystemAuthorization();
            var myUri = HttpContext.Current.Request.GetUrlRewriter();
            var requestUrlBook = cardDavAB.GetRadicaleUrl(myUri.ToString(), currentUserEmail, true, true);
            var tenant = CurrentTenant;
            var davRequest = new DavRequest()
            {
                Url = requestUrlBook,
                Authorization = authorization,
                Header = myUri.ToString()
            };
            await RadicaleClient.RemoveAsync(davRequest).ConfigureAwait(false);

            try
            {
                var dbConn = new DbRadicale();
                dbConn.RemoveCardDavUser(tenant, currUser.ID.ToString());

                return new DavResponse()
                {
                    Completed = true,
                    Data = Resource.RadicaleCardDavDeleteMessage
                };
            }
            catch (Exception ex)
            {
                Log.Error("ERROR: " + ex.Message);
                return new DavResponse()
                {
                    Completed = false,
                    Error = ex.Message
                };
            }


        }
        
        public string CardDavAllSerialization(Uri uri)
        {
            var builder = new StringBuilder();
            var users = CoreContext.UserManager.GetUsers();
            var addbook = new CardDavAddressbook();

            foreach (var user in users)
            {
                builder.AppendLine(addbook.GetUserSerialization(ItemFromUserInfo(user)));
            }

            return builder.ToString();
        }

        public static CardDavItem ItemFromUserInfo(UserInfo u)
        {
            return new CardDavItem(u.ID, u.FirstName, u.LastName, u.UserName, u.BirthDate, u.Sex, u.Title, u.Email, u.Contacts, u.MobilePhone);

        }
    }
}