using NUnit.Framework;
using Sitecore;

namespace SitecoreData.DataProviders.RavenDB.Tests
{
    [TestFixture]
    public class DataProviderShould
    {
        [TestFixtureSetUp]
        public void SetUp()
        {
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
        }

        [Test]
        public void create_root_item_with_version_in_constructor()
        {
            //// Arrange
            //// Act
            var provider = new DataProviderWrapper("Url = http://localhost:8080; DefaultDatabase = Test",
                                                   "SitecoreData.DataProviders.RavenDB.RavenDataProvider, SitecoreData.DataProviders.RavenDB");
            //// Assert
            var rootItem = provider.GetItemDefinition(ItemIDs.RootID, null);
            var rootItemVerison = provider.GetItemVersions(rootItem, null);

            Assert.That(rootItem, Is.Not.Null);
            Assert.That(rootItem.Name, Is.EqualTo("sitecore"));
            Assert.That(rootItemVerison, Is.Not.Null);
            Assert.That(rootItemVerison[0].Language.CultureInfo.Name, Is.EqualTo("en"));
            Assert.That(rootItemVerison[0].Version.Number, Is.EqualTo(1));
        }
    }
}