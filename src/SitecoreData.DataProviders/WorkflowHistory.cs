using System;

namespace SitecoreData.DataProviders
{
    public class WorkflowHistory
    {
        public Guid Id { get; set; }
        public string Language { get; set; }
        public int Version { get; set; }
        public string OldState { get; set; }
        public string NewState { get; set; }
        public string Text { get; set; }
        public string User { get; set; }
        public DateTime Now { get; set; }
    }
}