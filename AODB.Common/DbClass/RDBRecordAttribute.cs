using System;

namespace AODB.Common.DbClasses
{
    public class RDBRecordAttribute : Attribute
    {
        public RDBRecordAttribute() { }

        public int RecordTypeID { get; set; }
        public string Comments { get; set; }
    }
}
