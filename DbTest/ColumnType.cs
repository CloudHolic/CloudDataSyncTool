using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbTest
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
