using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CloudSync.Models;
using Dapper;
using MySqlConnector;

namespace CloudSync.Utils
{
    public class DbUtils
    {
        private readonly string _srcString;
        private readonly string _destString;

        public DbUtils(Connection srcConString, Connection destConString)
        {
            _srcString = MakeConnectionString(srcConString);
            _destString = MakeConnectionString(destConString);
        }

        private static string MakeConnectionString(Connection conString)
        {
            // SslMode=VerifyCA;
            return $"host={conString.Host};port={conString.Port};user id={conString.Id};password={conString.GetPassword()};"
                   + "AllowLoadLocalInfile=true;Allow User Variables=true;";
        }

        private static MySqlConnection ConnectionFactory(string connString)
        {
            var connection = new MySqlConnection(connString);
            connection.Open();
            return connection;
        }

        public List<string> FindSchemas(bool isSrc = true)
        {
            List<string> schemaList;

            using (var connection = ConnectionFactory(isSrc ? _srcString : _destString))
            {
                schemaList = connection.Query<string>("select SCHEMA_NAME from information_schema.SCHEMATA").ToList();
                schemaList.Sort();
            }

            return schemaList;
        }

        public List<string> FindTables(string schemaName, bool isSrc = true)
        {
            List<string> tableList;

            using (var connection = ConnectionFactory(isSrc ? _srcString : _destString))
            {
                tableList = connection.Query<string>("select TABLE_NAME from information_schema.TABLES "
                                                     + "where TABLE_SCHEMA = @Schema and TABLE_TYPE != 'VIEW'",
                    new { Schema = schemaName }).ToList();
            }

            return tableList;
        }

        public int BulkCopy(string srcSchemaName, string destSchemaName, string tableName)
        {
            try
            {
                //var stopwatch = new Stopwatch();

                if (string.IsNullOrEmpty(srcSchemaName) || string.IsNullOrEmpty(destSchemaName) || string.IsNullOrEmpty(tableName))
                    throw new ArgumentNullException();

                using (var srcConn = ConnectionFactory(_srcString))
                {
                    //stopwatch.Restart();
                    var count = srcConn.Query<int>($"select count(*) from {srcSchemaName}.{tableName}").First();
                    var createQuery = srcConn.Query($"show create table {srcSchemaName}.{tableName}").ToList()
                        .Select(x => (IDictionary<string, object>) x).ToList()[0]["Create Table"].ToString();
                    var pk = srcConn.Query<string>($"select column_name from information_schema.COLUMNS "
                                                   + "where TABLE_SCHEMA = @Schema and TABLE_NAME = @Table and COLUMN_KEY = 'PRI'",
                        new { Schema = srcSchemaName, Table = tableName }).ToList();
                    var columns = FindColumns(srcSchemaName, tableName);
                    
                    var loopCount = count / 1000000 + 1;

                    using (var dstConn = ConnectionFactory(_destString))
                    {
                        var sqlMode = dstConn.Query<string>("select @@SESSION.sql_mode").First();
                        var noStrictMode = sqlMode.Split(',').Where(mode => mode != "STRICT_TRANS_TABLES")
                            .Aggregate("", (current, mode) => current + mode + ",");
                        noStrictMode = noStrictMode.TrimEnd(',');

                        dstConn.Execute($"set session sql_mode = \"{noStrictMode}\"");
                        dstConn.Execute("set global local_infile = true");
                        dstConn.Execute("set foreign_key_checks = 0");
                        dstConn.Execute("set autocommit = 0");
                        dstConn.Execute("set unique_checks = 0");
                        
                        dstConn.Execute(createQuery.Replace("CREATE TABLE ", $"CREATE TABLE IF NOT EXISTS {destSchemaName}."));

                        var transaction = dstConn.BeginTransaction();
                        var bulkCopy = new MySqlBulkCopy(dstConn, transaction)
                        {
                            DestinationTableName = $"{destSchemaName}.{tableName}"
                        };

                        for (var i = 0; i < loopCount; i++)
                        {
                            var rawRecord = srcConn.Query($"select * from {srcSchemaName}.{tableName} "
                                                        + $"order by {string.Join(", ", pk)} asc limit 1000000 offset {i * 1000000}");
                            var result = rawRecord.Select(x => (IDictionary<string, object>) x).ToList();
                            var table = ConvertToDataTable(result, columns);
                            bulkCopy.WriteToServer(table);
                        }
                        transaction.Commit();

                        dstConn.Execute($"set session sql_mode = \"{sqlMode}\"");
                        dstConn.Execute("set global local_infile = false");
                        dstConn.Execute("set foreign_key_checks = 1");
                        dstConn.Execute("set autocommit = 1");
                        dstConn.Execute("set unique_checks = 1");
                    }
                }

                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 0;
            }
        }

        private List<ColumnType> FindColumns(string schemaName, string tableName, bool isSrc = true)
        {
            List<IDictionary<string, object>> result;

            using (var connection = ConnectionFactory(isSrc ? _srcString : _destString))
            {
                var rawResult = connection.Query("select COLUMN_NAME, COLUMN_TYPE, IS_NULLABLE from information_schema.COLUMNS "
                                                 + "where TABLE_SCHEMA = @Schema and TABLE_NAME = @Table",
                    new { Schema = schemaName, Table = tableName }).ToList();
                result = rawResult.Select(x => (IDictionary<string, object>)x).ToList();
            }

            return result.Select(column => new ColumnType
            {
                Name = column["COLUMN_NAME"].ToString(),
                Type = column["COLUMN_TYPE"].ToString(),
                Nullable = column["IS_NULLABLE"].ToString() == "YES"
            }).ToList();
        }

        private static DataTable ConvertToDataTable(List<IDictionary<string, object>> data, List<ColumnType> types)
        {
            var table = new DataTable();

            foreach (var type in types)
                table.Columns.Add(type.Name);
            
            foreach (var item in data)
            {
                var row = table.NewRow();
                foreach (var type in types)
                    row[type.Name] = item[type.Name];
                table.Rows.Add(row);
            }
            return table;
        }
    }
}
