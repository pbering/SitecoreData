using System.Collections.Generic;
using System.Configuration;
using MongoDB.Driver;
using NUnit.Framework;
using Sitecore;

namespace SitecoreData.DataProviders.Tests
{
    [TestFixture]
    public class ReadableDataProviderWrapperShould
    {
        private MongoServer _mongoServer;
        private const string _testDatabaseName = "webtest";

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            _mongoServer = MongoServer.Create(ConfigurationManager.ConnectionStrings["mongodb_" + _testDatabaseName].ConnectionString);

            var mongoAdminDatbase = _mongoServer.GetDatabase("admin");

            mongoAdminDatbase.RunCommand(new CommandDocument(new Dictionary<string, object>
                                                                 {
                                                                     {"copydb", "1"},
                                                                     {"fromdb", "web"},
                                                                     {"todb", _testDatabaseName},
                                                                 }));

            Assert.That(_mongoServer.DatabaseExists(_testDatabaseName), Is.True);

            // TODO: Setup RavenDB
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            if (_mongoServer.DatabaseExists(_testDatabaseName))
            {
                _mongoServer.DropDatabase(_testDatabaseName);
            }

            // TODO: Cleanup RavenDB
        }

        [Test]
        [TestCase("mongodb_webtest", "SitecoreData.DataProviders.MongoDB.MongoDataProvider, SitecoreData.DataProviders.MongoDB")]
        public void return_item_from_id(string connectionStringName, string implementationType)
        {
            //// Arrange
            var provider = new DataProviderWrapper(connectionStringName, implementationType);

            //// Act
            var item = provider.GetItemDefinition(ItemIDs.ContentRoot, null);

            //// Assert
            Assert.That(item, Is.Not.Null);
            Assert.That(item.Key, Is.EqualTo("content"));
        }
    }
}