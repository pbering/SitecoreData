using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Client.Document;

namespace SitecoreData.DataProviders.RavenDB
{
    public class RavenDataProvider : DataProviderBase, IWritableDataProvider, IDisposable
    {
        private readonly DocumentStore _store;

        public RavenDataProvider(string connectionString) : base(connectionString)
        {
            _store = new DocumentStore();
            _store.ParseConnectionString(connectionString);
            _store.Initialize();
        }

        public void Dispose()
        {
            if (_store != null && !_store.WasDisposed)
            {
                _store.Dispose();
            }
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
            using (var session = _store.OpenSession())
            {
                // TODO: Implement casede deletes of children or via index?
                session.Advanced.DatabaseCommands.Delete("itemdtos/" + id, null);

                // TODO: This is not recursive!?!?!
                foreach (var itemDto in GetChildIds(id))
                {
                    session.Delete(itemDto);
                }
            }

            return true;
        }

        public void Store(ItemDto item)
        {
            using (var session = _store.OpenSession())
            {
                session.Store(item);
                session.SaveChanges();
            }
        }

        public override ItemDto GetItem(Guid id)
        {
            using (var session = _store.OpenSession())
            {
                // TODO: Implement static index
                var item = session.Load<ItemDto>(id);

                return item;
            }
        }

        public override IEnumerable<Guid> GetChildIds(Guid parentId)
        {
            return GetChildrenOf(parentId).ToList().Select(item => item.Id).ToArray();
        }

        internal IEnumerable<ItemDto> GetChildrenOf(Guid itemId)
        {
            // TODO: Max results sets: http://ravendb.net/docs/client-api/basic-operations/understanding-session-object
            using (var session = _store.OpenSession())
            {
                // TODO: Implement static index
                var items = (from dto in session.Query<ItemDto>()
                             where dto.ParentId == itemId
                             select dto);

                return items.ToArray();
            }
        }

        internal IEnumerable<ItemDto> FindItemsOfTemplate(Guid templateId)
        {
            // TODO: Max results sets: http://ravendb.net/docs/client-api/basic-operations/understanding-session-object
            using (var session = _store.OpenSession())
            {
                // TODO: Implement static index
                var items = (from dto in session.Query<ItemDto>()
                             where dto.TemplateId == templateId
                             select dto);

                return items.ToArray();
            }
        }

        public override Guid GetParentId(Guid id)
        {
            // TODO: Only load Id, not the whole dto
            var item = GetItem(id);

            if (item != null)
            {
                return item.ParentId;
            }

            return Guid.Empty;
        }

        public override IEnumerable<Guid> GetTemplateIds(Guid templateId)
        {
            // TODO: Max results sets: http://ravendb.net/docs/client-api/basic-operations/understanding-session-object
            using (var session = _store.OpenSession())
            {
                // TODO: Implement static index
                var items = (from dto in session.Query<ItemDto>()
                             where dto.TemplateId == templateId
                             select dto);

                return items.ToArray().Select(item => item.Id).ToArray();
            }
        }

        public override IEnumerable<ItemDto> GetItemsInWorkflowState(Guid workflowStateId)
        {
            return new ItemDto[] {};
        }
    }
}