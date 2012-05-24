using System;
using Sitecore.Data;

namespace SitecoreData.DataProviders
{
    [Serializable]
    public class FieldDto
    {
        public Guid Id { get; set; }
        public string Value { get; set; }
        public string Language { get; set; }
        public int? Version { get; set; }

        public bool Matches(VersionUri versionUri)
        {
            var versionUriLanguage = versionUri.Language != null ? versionUri.Language.Name : null;
            var versionUriVersion = versionUri.Version != null ? versionUri.Version.Number : null as int?;

            var matchesLanguage = string.IsNullOrWhiteSpace(Language) || Language.Equals(versionUriLanguage);
            var matchesVersion = !Version.HasValue || Version.Equals(versionUriVersion);

            return matchesLanguage && matchesVersion;
        }
    }
}