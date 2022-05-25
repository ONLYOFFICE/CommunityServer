using System;
using System.Threading.Tasks;
using System.Web;

namespace ASC.Common.Radicale
{
    public class CalDavCalendar : RadicaleEntity
    {
        public bool IsShared { set; get; } = false;

        public readonly string strUpdateTemplate = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" +
                        "<propertyupdate xmlns=\"DAV:\" xmlns:C=\"urn:ietf:params:xml:ns:caldav\" xmlns:CR=\"urn:ietf:params:xml:ns:carddav\" xmlns:I=\"http://apple.com/ns/ical/\" xmlns:INF=\"http://inf-it.com/ns/ab/\">" +
                        "<set><prop>" +
                        "<C:supported-calendar-component-set><C:comp name=\"VEVENT\" /><C:comp name=\"VJOURNAL\" /><C:comp name=\"VTODO\" />" +
                        "</C:supported-calendar-component-set><displayname>{0}</displayname><I:calendar-color>{1}</I:calendar-color>" +
                        "<C:calendar-description>{2}</C:calendar-description></prop></set><remove><prop>" +
                        "<INF:calendar-color /><CR:calendar-description /></prop></remove></propertyupdate>";

        public readonly string strCreateTemplate = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" +
                      "<mkcol xmlns=\"DAV:\" xmlns:C=\"urn:ietf:params:xml:ns:caldav\" xmlns:CR=\"urn:ietf:params:xml:ns:carddav\" xmlns:I=\"http://apple.com/ns/ical/\" xmlns:INF=\"http://inf-it.com/ns/ab/\">" +
                      "<set><prop>" +
                      "<resourcetype><collection /><C:calendar /></resourcetype>" +
                      "<C:supported-calendar-component-set><C:comp name=\"VEVENT\" /><C:comp name=\"VJOURNAL\" /><C:comp name=\"VTODO\" />" +
                      "</C:supported-calendar-component-set><displayname>{0}</displayname>" +
                      "<I:calendar-color>{1}</I:calendar-color>" +
                      "<C:calendar-description>{2}</C:calendar-description></prop></set></mkcol>";

        public CalDavCalendar(string uid, bool isShared)
        {
            Uid = uid;
            IsShared = isShared;
        }

        public async Task<DavResponse> CreateAsync(string name, string description, string backgroundColor, Uri uri, string userName, string authorization)
        {
            var requestUrl = uri.Scheme + "://" + uri.Host + "/caldav/" + HttpUtility.UrlEncode(userName) + "/" + Uid + (IsShared ? "-readonly" : "");

            var davRequest = new DavRequest()
            {
                Url = requestUrl,
                Authorization = authorization,
                Data = GetData(strCreateTemplate, name, description, backgroundColor)
            };

            try
            {
                return await RadicaleClient.CreateAsync(davRequest).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new RadicaleException(ex.Message);
            }

        }

        public async Task<DavResponse> Update(string name, string description, string backgroundColor, Uri uri, string userName, string authorization)
        {
            var requestUrl = GetRadicaleUrl(uri.ToString(), userName, IsShared, isRedirectUrl: true, entityId: Uid);

            var davRequest = new DavRequest()
            {
                Url = requestUrl,
                Authorization = authorization,
                Data = GetData(strUpdateTemplate, name, description, backgroundColor)
            };

            return await RadicaleClient.UpdateAsync(davRequest).ConfigureAwait(false);
        }

        public async Task<DavResponse> GetCollection(string url, string authorization)
        {
            var davRequest = new DavRequest()
            {
                Url = url,
                Authorization = authorization
            };

            return await RadicaleClient.GetAsync(davRequest).ConfigureAwait(false);
        }

        public async Task<DavResponse> UpdateItem(string url, string authorization, string data, string headerUrl = "")
        {
            var davRequest = new DavRequest()
            {
                Url = url,
                Authorization = authorization,
                Header = headerUrl,
                Data = data
            };

            return await RadicaleClient.UpdateItemAsync(davRequest).ConfigureAwait(false);
        }


    }
}
