using System.Collections.Generic;

namespace CloudSync.Models
{
    public class WorkerArgs
    {
        public Connection SrcConnection { get; set; }
        
        public Connection DstConnection { get; set; }

        public string SrcSchemaName { get; set; }

        public string DstSchemaName { get; set; }

        public string DumpDirectory { get; set; }

        public List<string> SrcTables { get; set; }

        public bool DeleteFile { get; set; }
    }
}
