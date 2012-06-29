using System;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Workflows;

namespace SitecoreData.DataProviders.MongoDB
{
    public class MongoHistoryStore : HistoryStore
    {
        private const string _collectionName = "WorkflowHistory";

        public MongoHistoryStore(string connectionString)
        {
            var databaseName = MongoUrl.Create(connectionString).DatabaseName;

            Server = MongoServer.Create(connectionString);
            Db = Server.GetDatabase(databaseName);

            WorkflowHistories = Db.GetCollection<WorkflowHistory>(_collectionName, SafeMode.True);
        }

        private MongoCollection<WorkflowHistory> WorkflowHistories { get; set; }

        private MongoServer Server { get; set; }

        private MongoDatabase Db { get; set; }

        public override void AddHistory(Item item, string oldState, string newState, string text)
        {
            var history = new WorkflowHistory();
            history.Id = item.ID.Guid;
            history.Language = item.Language.ToString();
            history.NewState = newState;
            history.Now = DateTime.Now;
            history.OldState = oldState;
            history.Text = text;
            history.User = Context.GetUserName();
            history.Version = item.Version.ToInt32();

            WorkflowHistories.Save(history);
        }

        public override void ClearHistory(Item item)
        {
            var query = Query.And(
                Query.EQ("Id", item.ID.Guid),
                Query.EQ("Language", item.Language.ToString()),
                Query.EQ("Version", item.Version.ToInt32())
                );

            WorkflowHistories.Remove(query);
        }

        public override WorkflowEvent[] GetHistory(Item item)
        {
            var query = Query.And(
                Query.EQ("Id", item.ID.Guid),
                Query.EQ("Language", item.Language.ToString()),
                Query.EQ("Version", item.Version.ToInt32())
                );

            var results = WorkflowHistories.Find(query);
            var list = new List<WorkflowEvent>();

            foreach (var result in results)
            {
                var workflow = new WorkflowEvent(result.OldState, result.NewState, result.Text, result.User, result.Now);
                list.Add(workflow);
            }

            return list.ToArray();
        }
    }
}