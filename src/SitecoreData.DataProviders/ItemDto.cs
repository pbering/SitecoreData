using System;
using System.Collections.Generic;

namespace SitecoreData.DataProviders
{
    [Serializable]
    public class ItemDto
    {
        private List<FieldDto> _fieldValues;
        public Guid Id { get; set; }
        public Guid BranchId { get; set; }
        public Guid TemplateId { get; set; }
        public string Name { get; set; }
        public Guid ParentId { get; set; }
        public Guid WorkflowStateId { get; set; }

        public List<FieldDto> FieldValues
        {
            get { return _fieldValues ?? (_fieldValues = new List<FieldDto>()); }
            set { _fieldValues = value; }
        }
    }
}