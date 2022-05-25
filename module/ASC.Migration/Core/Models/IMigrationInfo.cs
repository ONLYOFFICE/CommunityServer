using ASC.Migration.Core.Models.Api;

namespace ASC.Migration.Core.Models
{
    public interface IMigrationInfo
    {
        MigrationApiInfo ToApiInfo();

        void Merge(MigrationApiInfo apiInfo);
    }
}
