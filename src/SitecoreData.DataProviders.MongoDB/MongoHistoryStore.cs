using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Workflows;
using MongoDB.Driver;
using Sitecore;
using MongoDB.Driver.Builders;

namespace SitecoreData.DataProviders.MongoDB
{
   public  class MongoHistoryStore : HistoryStore
    {
        private const string CollectionName = "WorkflowHistory";

        private MongoCollection<WorkflowHistory> WorkflowHistories { get; set; } 

        private MongoServer Server { get; set; }

        private MongoDatabase Db { get; set; }

        public MongoHistoryStore(string connectionString)
        {
            var databaseName = MongoUrl.Create(connectionString).DatabaseName;

            Server = MongoServer.Create(connectionString);
            Db = Server.GetDatabase(databaseName);

            WorkflowHistories = Db.GetCollection<WorkflowHistory>(CollectionName, SafeMode.True);

        }

        public override void AddHistory(Sitecore.Data.Items.Item item, string oldState, string newState, string text)
        {
            WorkflowHistory history = new WorkflowHistory();
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

        public override void ClearHistory(Sitecore.Data.Items.Item item)
        {
            var query = Query.And(
                    Query.EQ("Id", item.ID.Guid),
                    Query.EQ("Language", item.Language.ToString()),
                    Query.EQ("Version", item.Version.ToInt32())
                );

            WorkflowHistories.Remove(query);
        }

        public override WorkflowEvent[] GetHistory(Sitecore.Data.Items.Item item)
        {
            var query = Query.And(
                   Query.EQ("Id", item.ID.Guid),
                   Query.EQ("Language", item.Language.ToString()),
                   Query.EQ("Version", item.Version.ToInt32())
               );

            var results = WorkflowHistories.Find(query);

            List<WorkflowEvent> list = new List<WorkflowEvent>();

            foreach (var result in results)
            {
                WorkflowEvent workflow = new WorkflowEvent(result.OldState, result.NewState, result.Text, result.User, result.Now);
                list.Add(workflow);
            }
            return list.ToArray();
        }
    }
}
