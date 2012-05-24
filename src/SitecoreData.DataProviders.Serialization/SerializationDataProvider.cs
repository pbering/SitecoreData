using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sitecore;
using Sitecore.Collections;
using Sitecore.Data;
using Sitecore.Data.DataProviders;
using Sitecore.Data.Templates;
using Sitecore.Globalization;
using Version = Sitecore.Data.Version;

namespace SitecoreData.DataProviders.Serialization
{
    public class SerializationDataProvider : DataProvider
    {
        public SerializationDataProvider(string connectionString)
        {
        }

        public string Name { get; set; }

        public override CacheOptions CacheOptions
        {
            get
            {
                var options = new CacheOptions();

                options.DisableAll = true;

                return options;
            }
        }

        public override ID SelectSingleID(string query, CallContext context)
        {
            return base.SelectSingleID(query, context);
        }

        public override bool HasChildren(ItemDefinition itemDefinition, CallContext context)
        {
            var database = GetDatabase(context);
            var children = database.GetChildren(itemDefinition.ID.ToString());

            if (children == null || !children.Any())
            {
                return false;
            }

            return true;
        }

        public override IDList GetChildIDs(ItemDefinition itemDefinition, CallContext context)
        {
            var database = GetDatabase(context);
            var children = database.GetChildren(itemDefinition.ID.ToString());
            var ids = new IDList();

            if (children == null)
            {
                return ids;
            }

            foreach (var syncItem in children)
            {
                ids.Add(ID.Parse(syncItem.ID));
            }

            return ids;
        }

        public override bool BlobStreamExists(Guid blobId, CallContext context)
        {
            return base.BlobStreamExists(blobId, context);
        }

        public override Stream GetBlobStream(Guid blobId, CallContext context)
        {
            return base.GetBlobStream(blobId, context);
        }

        public override long GetDataSize(int minEntitySize, int maxEntitySize)
        {
            return base.GetDataSize(minEntitySize, maxEntitySize);
        }

        public override string GetProperty(string name, CallContext context)
        {
            return base.GetProperty(name, context);
        }

        public override long GetDictionaryEntryCount()
        {
            return base.GetDictionaryEntryCount();
        }

        public override List<string> GetPropertyKeys(string prefix, CallContext context)
        {
            return base.GetPropertyKeys(prefix, context);
        }

        public override ID ResolvePath(string itemPath, CallContext context)
        {
            var id = ID.Null;

            if (itemPath.EndsWith("/"))
            {
                itemPath = itemPath.Remove(itemPath.LastIndexOf("/"));
            }

            var database = GetDatabase(context);
            var syncItem = database.GetItem(itemPath);

            if (syncItem == null)
            {
                return id;
            }

            return id;
        }

        public override IdCollection GetTemplateItemIds(CallContext context)
        {
            var database = GetDatabase(context);
            var templatesRoot = database.GetItems(ItemIDs.TemplateRoot.ToString());
            var ids = new IdCollection();

            foreach (var syncItem in templatesRoot)
            {
                ids.Add(ID.Parse(syncItem.ID));
            }

            return ids;
        }

        public override TemplateCollection GetTemplates(CallContext context)
        {
            var owner = new TemplateCollection();

            var database = GetDatabase(context);
            var templatesRoot = database.GetItems(ItemIDs.TemplateRoot.ToString());

            var templates = new List<Template>(templatesRoot.Count());

            foreach (var syncItem in templatesRoot)
            {
                if (ID.IsNullOrEmpty(ID.Parse(syncItem.ID)) || syncItem.ID.Equals(TemplateIDs.TemplateFolder.ToString()))
                {
                    continue;
                }

                var templateBuilder = new Template.Builder(syncItem.Name, ID.Parse(syncItem.ID), owner);

                templateBuilder.SetFullName(syncItem.ItemPath);

                var dataSection = templateBuilder.AddSection("Data", ID.NewID);

                foreach (var syncField in syncItem.SharedFields)
                {
                    if (string.IsNullOrEmpty(syncField.FieldID) || string.IsNullOrEmpty(syncField.FieldName))
                    {
                        continue;
                    }

                    if (syncField.FieldID.Equals(FieldIDs.BaseTemplate.ToString()))
                    {
                        templateBuilder.SetBaseIDs(syncField.FieldValue);
                    }
                    else
                    {
                        dataSection.AddField(syncField.FieldName, ID.Parse(syncField.FieldID));
                    }
                }

                var version = syncItem.GetLatestVersions().FirstOrDefault();

                foreach (var syncField in version.Fields)
                {
                    if (string.IsNullOrEmpty(syncField.FieldID) || string.IsNullOrEmpty(syncField.FieldName))
                    {
                        continue;
                    }

                    dataSection.AddField(syncField.FieldName, ID.Parse(syncField.FieldID));
                }

                templates.Add(templateBuilder.Template);
            }

            owner.Reset(templates.ToArray());

            return owner;
        }

        public override ID GetRootID(CallContext context)
        {
            return ItemIDs.RootID;
        }

        public override LanguageCollection GetLanguages(CallContext context)
        {
            return base.GetLanguages(context);
        }

        public override ID GetParentID(ItemDefinition itemDefinition, CallContext context)
        {
            if (itemDefinition == ItemDefinition.Empty)
            {
                return ID.Null;
            }

            var database = GetDatabase(context);
            var syncItem = database.GetItem(itemDefinition.ID.ToString());

            if (syncItem == null)
            {
                return ID.Null;
            }

            return ID.Parse(syncItem.ParentID);
        }

        public override IDList SelectIDs(string query, CallContext context)
        {
            return base.SelectIDs(query, context);
        }

        public override FieldList GetItemFields(ItemDefinition itemDefinition, VersionUri versionUri, CallContext context)
        {
            var fields = new FieldList();

            if (itemDefinition == ItemDefinition.Empty)
            {
                return fields;
            }

            var database = GetDatabase(context);
            var syncItem = database.GetItem(itemDefinition.ID.ToString(), versionUri.Language.Name);

            if (syncItem == null)
            {
                return fields;
            }

            foreach (var syncField in syncItem.SharedFields)
            {
                fields.Add(ID.Parse(syncField.FieldID), syncField.FieldValue);
            }

            var syncVersion = syncItem.GetLatestVersions().Where(v => v.Language.Equals(versionUri.Language.Name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            if (syncVersion == null)
            {
                return fields;
            }

            foreach (var syncField in syncVersion.Fields)
            {
                fields.Add(ID.Parse(syncField.FieldID), syncField.FieldValue);
            }

            return fields;
        }

        public override VersionUriList GetItemVersions(ItemDefinition itemDefinition, CallContext context)
        {
            var versions = new VersionUriList();

            if (itemDefinition == ItemDefinition.Empty)
            {
                return versions;
            }

            var database = GetDatabase(context);
            var syncItem = database.GetItem(itemDefinition.ID.ToString());

            foreach (var syncVersion in syncItem.Versions)
            {
                versions.Add(Language.Parse(syncVersion.Language), Version.Parse(syncVersion.Version));
            }

            return versions;
        }

        public override ItemDefinition GetItemDefinition(ID itemId, CallContext context)
        {
            var database = GetDatabase(context);
            var syncItem = database.GetItem(itemId.ToString());

            if (syncItem == null)
            {
                return ItemDefinition.Empty;
            }

            return new ItemDefinition(itemId, syncItem.Name, ID.Parse(syncItem.TemplateID), ID.Parse(syncItem.BranchId));
        }

        private SerializedDatabase GetDatabase(CallContext context)
        {
            var database =
                new SerializedDatabase(string.Format(@"...\Data\serialization\{0}", context.DataManager.Database.Name));

            return database;
        }
    }
}