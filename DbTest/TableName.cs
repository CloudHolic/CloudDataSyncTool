using System.Collections.Generic;

namespace DbTest
{
    public class TableName
    {
        public string Table { get; set; }

        public string FileName { get; set; }

        public static List<TableName> GetTableNameList(List<string> tableNameList, List<string> fileNameList)
        {
            if (tableNameList.Count != fileNameList.Count)
                return null;

            var result = new List<TableName>();
            for(var i = 0; i < tableNameList.Count; i++)
                result.Add(new TableName
                {
                    Table = tableNameList[i],
                    FileName = fileNameList[i]
                });

            return result;
        }
    }
}
