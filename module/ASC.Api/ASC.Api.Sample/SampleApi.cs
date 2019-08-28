using System.Collections.Generic;
using ASC.Api.Attributes;
using ASC.Api.Interfaces;
using ASC.Web.Sample.Classes;

namespace ASC.Api.Sample
{
    /// <summary>
    /// Sample CRUD Api
    /// </summary>
    public class SampleApi : IApiEntryPoint
    {
        /// <summary>
        /// ASC.Api.Interfaces.IApiEntryPoint.Name
        /// </summary>
        public string Name
        {
            get { return "sample"; }
        }

        /// <summary>
        /// Create item
        /// </summary>
        /// <param name="value">item value</param>
        /// <returns>SampleClass item</returns>
        [Create("create", false)]
        public SampleClass Create(string value)
        {
            return SampleDao.Create(value);
        }

        /// <summary>
        /// Read item by id
        /// </summary>
        /// <param name="id">item id</param>
        /// <returns>SampleClass item</returns>
        [Read(@"read/{id:[0-9]+}", false)]
        public SampleClass Read(int id)
        {
            return SampleDao.Read(id);
        }

        /// <summary>
        /// Read all items
        /// </summary>
        /// <returns>SampleClass items list</returns>
        [Read("read", false)]
        public List<SampleClass> Read()
        {
            return SampleDao.Read();
        }

        /// <summary>
        /// Update item
        /// </summary>
        /// <param name="id">item id</param>
        /// <param name="value">new item value</param>
        [Update("update", false)]
        public void Update(int id, string value)
        {
            SampleDao.Update(id, value);
        }

        /// <summary>
        /// Update item by id
        /// </summary>
        /// <param name="id">item id</param>
        [Delete("delete/{id:[0-9]+}", false)]
        public void Delete(int id)
        {
            SampleDao.Delete(id);
        }
    }
}
