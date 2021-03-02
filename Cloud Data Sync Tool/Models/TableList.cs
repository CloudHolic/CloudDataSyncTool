using System.Collections.ObjectModel;

namespace CloudSync.Models
{
    public class TableList
    {
        public string SchemaName { get; set; }

        public ObservableCollection<string> TableNames { get; set; }
    }
}
