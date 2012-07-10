using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Sitecore;
using Sitecore.Data;

namespace SitecoreData.DataProviders.MongoDB
{
    public class MongoDataProvider : DataProviderBase, IWritableDataProvider
    {
        private readonly MongoCollection<ItemDto> _items;

        public MongoDataProvider(string connectionString) : base(connectionString)
        {
            var databaseName = MongoUrl.Create(connectionString).DatabaseName;
            var server = MongoServer.Create(connectionString);
            var database = server.GetDatabase(databaseName);

            _items = database.GetCollection<ItemDto>("items");
            _items.EnsureIndex(IndexKeys.Ascending(new[] {"ParentId"}));
            _items.EnsureIndex(IndexKeys.Ascending(new[] {"TemplateId"}));
        }

        public bool CreateItem(Guid id, string name, Guid templateId, Guid parentId)
        {
            var exists = GetItem(id);

            if (exists != null)
            {
                return true;
            }

            var item = new ItemDto
                           {
                               Id = id,
                               Name = name,
                               TemplateId = templateId,
                               ParentId = parentId
                           };

            Store(item);

            return true;
        }

        public bool DeleteItem(Guid id)
        {
            var result = _items.Remove(Query.EQ("_id", id), RemoveFlags.Single);

            return result != null && result.Ok;
        }

        public void Store(ItemDto item)
        {
            _items.Save(item);
        }

        public override IEnumerable<ItemDto> GetItemsInWorkflowState(Guid workflowStateId)
        {
            var query = Query.EQ("WorkflowStateId", workflowStateId);

            return _items.Find(query);
        }

        public override ItemDto GetItem(Guid id)
        {
            return _items.FindOneByIdAs<ItemDto>(id);
        }

        public override IEnumerable<Guid> GetChildIds(Guid parentId)
        {
            var query = Query.EQ("ParentId",
                                 parentId == ID.Null.ToGuid()
                                     ? Guid.Empty
                                     : parentId);

            return _items.FindAs<ItemDto>(query).Select(it => it.Id).ToArray();
        }

        public override Guid GetParentId(Guid id)
        {
            var result = _items.FindOneByIdAs<ItemDto>(id);

            return result != null ? (result.ParentId != Guid.Empty ? result.ParentId : ID.Null.ToGuid()) : Guid.Empty;
        }

        public override IEnumerable<Guid> GetTemplateIds(Guid templateId)
        {
            var query = Query.EQ("TemplateId", TemplateIDs.Template.ToGuid());
            var ids = new List<Guid>();

            foreach (var id in _items.FindAs<ItemDto>(query).Select(it => it.Id))
            {
                ids.Add(id);
            }

            return ids;
        }
    }
}