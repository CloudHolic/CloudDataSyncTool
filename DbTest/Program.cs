using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DbTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var srcCon = new Connection("cloudholic", "Lhs951356!", "localhost");
            var dstCon = new Connection("cloudholic", "Lhs951356!", "localhost");
            var srcSchema = "employees";
            var dstSchema = "devtest";


            var dbModule = new DbModule(srcCon, dstCon);

            /*
            var schemas = dbModule.FindSchemas();
            foreach(var schema in schemas)
                Console.WriteLine(schema);
            Console.WriteLine();

            var tables = dbModule.FindTables(schemaName);
            foreach (var table in tables)
                Console.WriteLine(table);
            Console.WriteLine();

            var columns = dbModule.FindColumns(schemaName, tableName);
            foreach(var column in columns)
                Console.WriteLine($"{column.Name} - {column.Type}");
            Console.WriteLine();

            var sql = dbModule.FindCreateSql(schemaName, tableName);
            Console.WriteLine(sql);
            */

            /*
            var stopwatch = new Stopwatch();
            var lapTime = 0.0;
            stopwatch.Start();

            var tables = dbModule.FindTables(srcSchema);
            foreach (var table in tables)
            {
                var filePath = $@"D:\DbDumps\{srcSchema}-{table}.csv";
                dbModule.SaveTable(srcSchema, table, filePath);
                var count = dbModule.BulkLoad(srcSchema, dstSchema, table, filePath);
                File.Delete(filePath);

                Console.WriteLine($"{count} rows inserted into '{table}'. Elapsed time: {(stopwatch.Elapsed.TotalMilliseconds - lapTime) / 1000:0.####}s");
                lapTime = stopwatch.Elapsed.TotalMilliseconds;
            }

            Console.WriteLine($"Total time elapsed : {stopwatch.Elapsed.TotalMilliseconds / 1000:0.####}s");
            */

            var tables = dbModule.FindTables(srcSchema);
            var files = tables.Select(table => $@"D:\DbDumps\{srcSchema}-{table}.csv").ToList();
            var tableList = TableName.GetTableNameList(tables, files);

            if (tableList != null && tableList.Count > 0)
            {
                dbModule.SaveTables(srcSchema, tableList);
                dbModule.BulkLoad(srcSchema, dstSchema, tableList, false);
            }

            Console.WriteLine("End Tasks");
            Console.ReadKey();
        }
    }
}
