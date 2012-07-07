using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Sitecore;
using Sitecore.Data;

namespace SitecoreData.DataProviders.MongoDB
{
    public class MongoDataProvider : DataProviderBase, IWritableDataProvider, IDisposable
    {
        public MongoDataProvider(string connectionString) : base(connectionString)
        {
            SafeMode = SafeMode.True;

            var databaseName = MongoUrl.Create(connectionString).DatabaseName;

            Server = MongoServer.Create(connectionString);
            Db = Server.GetDatabase(databaseName);

            Items = Db.GetCollection<ItemDto>("items", SafeMode);
            Items.EnsureIndex(IndexKeys.Ascending(new[] {"ParentId"}));
            Items.EnsureIndex(IndexKeys.Ascending(new[] {"TemplateId"}));
        }

        private MongoServer Server { get; set; }

        private MongoDatabase Db { get; set; }

        private MongoCollection<ItemDto> Items { get; set; }

        private SafeMode SafeMode { get; set; }

        public void Dispose()
        {
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
            var result = Items.Remove(Query.EQ("_id", id), RemoveFlags.Single, SafeMode);

            return result != null && result.Ok;
        }

        public void Store(ItemDto item)
        {
            Items.Save(item, SafeMode);
        }

        public override IEnumerable<ItemDto> GetItemsInWorkflowState(Guid workflowStateId)
        {
            var query = Query.EQ("WorkflowStateId", workflowStateId);

            return Items.Find(query);
        }

        public override ItemDto GetItem(Guid id)
        {
            return Items.FindOneByIdAs<ItemDto>(id);
        }

        public override IEnumerable<Guid> GetChildIds(Guid parentId)
        {
            var query = Query.EQ("ParentId",
                                 parentId == ID.Null.ToGuid()
                                     ? Guid.Empty
                                     : parentId);

            return Items.FindAs<ItemDto>(query).Select(it => it.Id).ToArray();
        }

        public override Guid GetParentId(Guid id)
        {
            var result = Items.FindOneByIdAs<ItemDto>(id);

            return result != null ? (result.ParentId != Guid.Empty ? result.ParentId : ID.Null.ToGuid()) : Guid.Empty;
        }

        public override IEnumerable<Guid> GetTemplateIds(Guid templateId)
        {
            var query = Query.EQ("TemplateId", TemplateIDs.Template.ToGuid());
            var ids = new List<Guid>();

            foreach (var id in Items.FindAs<ItemDto>(query).Select(it => it.Id))
            {
                ids.Add(id);
            }

            return ids;
        }
    }
}