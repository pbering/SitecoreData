using System;
using System.Collections.Generic;

namespace SitecoreData.DataProviders
{
    public abstract class DataProviderBase : IReadableDataProvider
    {
        protected DataProviderBase(string connectionString)
        {
            ConnectionString = connectionString;
        }

        protected string ConnectionString { get; set; }

        public IWritableDataProvider WritableProvider
        {
            get
            {
                var provider = this as IWritableDataProvider;

                if (provider == null)
                {
                    throw new Exception(string.Format("Current provider \"{0}\" does not implement {1}", GetType(), typeof(IWritableDataProvider)));
                }

                return provider;
            }
        }

        public abstract ItemDto GetItem(Guid id);

        public abstract Guid GetParentId(Guid id);

        public abstract IEnumerable<Guid> GetChildIds(Guid parentId);

        public abstract IEnumerable<Guid> GetTemplateIds(Guid templateId);
    }
}