using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASC.Common.Radicale
{
    interface IRadicaleEntity
    {
        string Create(DavRequest davrequest);
        void Update(DavRequest davRequest);
        void Remove(DavRequest davRequest);
        void UpdateItem(DavRequest davRequest);
        DavResponse GetCollection(DavRequest davRequest);

    }
}
