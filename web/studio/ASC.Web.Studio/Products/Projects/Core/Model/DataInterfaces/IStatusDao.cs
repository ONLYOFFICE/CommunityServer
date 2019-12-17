using System.Collections.Generic;
using ASC.Web.Projects;

namespace ASC.Projects.Core.DataInterfaces
{
    public interface IStatusDao
    {
        CustomTaskStatus Create(CustomTaskStatus status);

        void Update(CustomTaskStatus status);

        List<CustomTaskStatus> Get();

        void Delete(int id);
    }
}