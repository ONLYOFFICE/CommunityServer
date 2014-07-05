
using System;
using System.Reflection;

namespace MSBuild.Community.Tasks.Tfs.Proxies
{
    internal class Changeset
    {
        Type _type;
        object _instance;

        public Changeset(Assembly clientAssembly) : this(clientAssembly, null) { }
        public Changeset(Assembly clientAssembly, object instance)
        {
            _type = clientAssembly.GetType("Microsoft.TeamFoundation.VersionControl.Client.Changeset");
            _instance = instance;
        }

        public Type Type { get { return _type; } }
        public object Instance { get { return _instance; } }

        public int ChangesetId
        {
            get
            {
                PropertyInfo changesetIdProperty = _type.GetProperty("ChangesetId");
                int changesetId = (int)changesetIdProperty.GetValue(_instance, null);
                return changesetId;
            }
        }
    }
}
