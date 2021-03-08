using System.Collections.Generic;

namespace CloudSync.Models
{
    public class TableList
    {
        public string SchemaName { get; set; }

        public List<string> Tables { get; set; }

        public TableList()
        {
            Tables = new List<string>();
        }

        public TableList(TableList prevList)
        {
            SchemaName = prevList.SchemaName;
            Tables = new List<string>(prevList.Tables);
        }

        public TableList(string schemaName, List<string> tables)
        {
            SchemaName = schemaName;
            Tables = new List<string>(tables);
        }
    }
}
