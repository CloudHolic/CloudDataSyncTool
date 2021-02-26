namespace CloudSync.Models
{
    public class ColumnType
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public int MaxLength { get; set; }
    }

    public class RawColumnType
    {
        public string ColumnName { get; set; }

        public string ColumnType { get; set; }
    }
}
