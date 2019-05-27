using System.Collections.Generic;
using ASC.Mail.Core.Entities;

namespace ASC.Mail.Core.Dao.Interfaces
{
    public interface IFilterDao
    {
        /// <summary>
        ///     Get a list of filters
        /// </summary>
        List<Filter> GetList();

        /// <summary>
        ///     Get filter by id.
        /// </summary>
        /// <param name="id">id</param>
        Filter Get(int id);

        /// <summary>
        ///     Save or update filter
        /// </summary>
        /// <param name="filter"></param>
        int Save(Filter filter);

        /// <summary>
        ///     Delete filter
        /// </summary>
        /// <param name="id">id</param>
        int Delete(int id);
    }
}
