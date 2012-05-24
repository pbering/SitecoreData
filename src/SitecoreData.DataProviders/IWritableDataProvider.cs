using System;

namespace SitecoreData.DataProviders
{
    public interface IWritableDataProvider
    {
        bool CreateItem(Guid id, string name, Guid templateId, Guid parentId);

        bool DeleteItem(Guid id);

        void Store(ItemDto itemDto);
    }
}