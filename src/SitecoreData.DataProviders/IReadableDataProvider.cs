using System;
using System.Collections.Generic;

namespace SitecoreData.DataProviders
{
    public interface IReadableDataProvider
    {
        ItemDto GetItem(Guid id);

        Guid GetParentId(Guid id);

        IEnumerable<Guid> GetChildIds(Guid parentId);

        IEnumerable<Guid> GetTemplateIds(Guid templateId);

        IEnumerable<ItemDto> GetItemsInWorkflowState(Guid workflowStateId);
    }
}