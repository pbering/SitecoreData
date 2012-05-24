using System.Collections.Generic;
using Sitecore.Collections;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Reflection;

namespace Codehouse.SerializationDataProvider
{
    public class PrefetchData
    {
        // Fields
        private readonly StringDictionary _addedVersions = new StringDictionary();
        private static readonly int _baseCacheSize = (TypeUtil.SizeOfDictionary() + (3 * TypeUtil.SizeOfInt32()));
        private readonly IDList _childIds = new IDList();
        private long _dataLength;
        private readonly SafeDictionary<string, FieldList> _fieldLists = new SafeDictionary<string, FieldList>();
        private readonly ItemDefinition _itemDefinition;
        private readonly ID _parentId;
        private readonly VersionUriList _versionUris = new VersionUriList();

        // Methods
        public PrefetchData(ItemDefinition itemDefinition, ID parentId)
        {
            Assert.ArgumentNotNull(itemDefinition, "itemDefinition");
            Assert.ArgumentNotNull(parentId, "parentId");
            this._itemDefinition = itemDefinition;
            this._parentId = parentId;
        }

        public void AddChildId(ID childId)
        {
            this._childIds.Add(childId);
        }

        public void AddField(string language, int version, ID fieldId, string value)
        {
            if (version > 0)
            {
                this.AddVersionedField(language, version, fieldId, value);
            }
            else if (language.Length > 0)
            {
                this.AddUnversionedField(language, fieldId, value);
            }
            else
            {
                this.AddSharedField(fieldId, value);
            }
        }

        private FieldList AddFieldList(string key)
        {
            FieldList list = new FieldList();
            lock (this._fieldLists.SyncRoot)
            {
                this._fieldLists[key] = list;
            }
            return list;
        }

        private FieldList AddFieldList(Language language, Version version)
        {
            string fieldListKey = this.GetFieldListKey(language.Name, version.Number);
            return this.AddFieldList(fieldListKey);
        }

        private FieldList AddFieldList(string language, int version)
        {
            string fieldListKey = this.GetFieldListKey(language, version);
            return this.AddFieldList(fieldListKey);
        }

        public void AddSharedField(ID fieldId, string value)
        {
            foreach (FieldList list in this._fieldLists.Values)
            {
                list.Add(fieldId, value);
            }
        }

        public void AddUnversionedField(string language, ID fieldId, string value)
        {
            string fieldListKey = this.GetFieldListKey(language);
            lock (this._fieldLists.SyncRoot)
            {
                foreach (KeyValuePair<string, FieldList> pair in this._fieldLists)
                {
                    if (pair.Key.StartsWith(fieldListKey))
                    {
                        pair.Value.Add(fieldId, value);
                    }
                }
            }
        }

        public void AddVersionedField(string language, int version, ID fieldId, string value)
        {
            this.GetFieldList(language, version, true).Add(fieldId, value);
            this.AddVersionUri(language, version);
        }

        private void AddVersionUri(string languageName, int versionNumber)
        {
            Language language;
            string key = languageName + '\x00a4' + versionNumber;
            if (!this._addedVersions.ContainsKey(key) && Language.TryParse(languageName, out language))
            {
                this._versionUris.Add(language, new Version(versionNumber));
                lock (this._addedVersions.SyncRoot)
                {
                    this._addedVersions[key] = string.Empty;
                }
            }
        }

        public IDList GetChildIds()
        {
            return this._childIds;
        }

        public long GetDataLength()
        {
            if (this._dataLength > 0L)
            {
                return this._dataLength;
            }
            long num = _baseCacheSize;
            num += this._itemDefinition.GetDataLength();
            num += this._parentId.GetDataLength();
            num += this._childIds.GetDataLength();
            num += this._versionUris.GetDataLength();
            foreach (FieldList list in this._fieldLists.Values)
            {
                num += list.GetDataLength();
            }
            this._dataLength = num;
            return num;
        }

        public FieldList GetFieldList(string language, int version)
        {
            return this.GetFieldList(language, version, false);
        }

        public FieldList GetFieldList(string language, int version, bool force)
        {
            string fieldListKey = this.GetFieldListKey(language, version);
            FieldList list = this._fieldLists[fieldListKey];
            if (list != null)
            {
                return list;
            }
            if (!force)
            {
                return null;
            }
            return this.AddFieldList(language, version);
        }

        private string GetFieldListKey(string language)
        {
            return (language.ToUpper() + '\x00a4');
        }

        private string GetFieldListKey(string language, int version)
        {
            return (language.ToUpper() + '\x00a4' + version);
        }

        public FieldList GetSharedFields()
        {
            return this.GetFieldList("", 1, false);
        }

        public VersionUriList GetVersionUris()
        {
            return this._versionUris;
        }

        public void InitializeFieldLists(LanguageCollection languages)
        {
            foreach (Language language in languages)
            {
                this.AddFieldList(language, Version.First);
            }
            this.AddFieldList(Language.Invariant, Version.First);
        }

        // Properties
        public ItemDefinition ItemDefinition
        {
            get
            {
                return this._itemDefinition;
            }
        }

        public ID ParentId
        {
            get
            {
                return this._parentId;
            }
        }
    }


 
 

}