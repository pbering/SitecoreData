using System;
using System.Collections.Generic;
using Sitecore.Data;

namespace SitecoreData.DataProviders.Serialization
{
    public class SerializationDataProvider : DataProviderBase
    {
        private readonly SerializedDatabase _database;

        public SerializationDataProvider(string connectionString) : base(connectionString)
        {
            _database = new SerializedDatabase(connectionString);
        }

        public override ItemDto GetItem(Guid id)
        {
            var syncItem = _database.GetItem(id.ToString());

            if (syncItem == null)
            {
                return null;
            }

            return new ItemDto
                       {
                           Id = Guid.Parse(syncItem.ID),
                           BranchId = Guid.Parse(syncItem.BranchId),
                           // TODO: Add fields?
                           FieldValues = new List<FieldDto>(),
                           Name = syncItem.Name,
                           ParentId = Guid.Parse(syncItem.ParentID),
                           TemplateId = Guid.Parse(syncItem.TemplateID)
                       };
        }

        public override Guid GetParentId(Guid id)
        {
            var result = GetItem(id);

            return result != null ? (result.ParentId != Guid.Empty ? result.ParentId : ID.Null.ToGuid()) : Guid.Empty;
        }

        public override IEnumerable<Guid> GetChildIds(Guid parentId)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Guid> GetTemplateIds(Guid templateId)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<ItemDto> GetItemsInWorkflowState(Guid workflowStateId)
        {
            return new ItemDto[] {};
        }
    }
}