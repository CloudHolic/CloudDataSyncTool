using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using CloudSync.Models;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
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

        #region Unused Methods
        public string FindCreateSql(string schemaName, string tableName)
        {
            List<IDictionary<string, object>> result;

            using (var connection = ConnectionFactory(_srcString))
            {
                var rawResult = connection.Query($"show create table {schemaName}.{tableName}").ToList();
                result = rawResult.Select(x => (IDictionary<string, object>)x).ToList();
            }

            return result[0]["Create Table"].ToString();
        }

        public string SaveTable(string schemaName, string tableName, string dumpDirectory)
        {
            try
            {
                //var stopwatch = new Stopwatch();

                if (string.IsNullOrWhiteSpace(schemaName) || string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(dumpDirectory))
                    throw new ArgumentNullException();

                if (!Directory.Exists(dumpDirectory))
                    throw new DirectoryNotFoundException();

                var baseFileName = $@"{schemaName}-{tableName}";

                using (var connection = ConnectionFactory(_srcString))
                {
                    //stopwatch.Restart();
                    var count = connection.Query<int>($"select count(*) from {schemaName}.{tableName}").First();
                    var pk = connection.Query<string>("select column_name from information_schema.COLUMNS "
                                                      + "where TABLE_SCHEMA = @Schema and TABLE_NAME = @Table and COLUMN_KEY = 'PRI'",
                        new { Schema = schemaName, Table = tableName }).ToList();

                    var loopCount = count / 1000000 + 1;
                    var config = new CsvConfiguration(CultureInfo.CurrentCulture);

                    var fileName = Path.Combine(dumpDirectory, $@"{baseFileName}.csv");

                    for (var i = 0; i < loopCount; i++)
                    {
                        var records = connection.Query($"select * from {schemaName}.{tableName} "
                                                       + $"order by {string.Join(", ", pk)} asc limit 1000000 offset {i * 1000000}");

                        config.HasHeaderRecord = i == 0;

                        using (var streamWriter = new StreamWriter(fileName, true))
                        using (var csvWriter = new CsvWriter(streamWriter, config))
                        {
                            var options = new TypeConverterOptions { Formats = new[] { "yyyy/MM/dd HH:mm:ss.ffffff" } };
                            csvWriter.Context.TypeConverterOptionsCache.AddOptions<DateTime>(options);
                            csvWriter.WriteRecords(records);
                        }
                    }

                    //Console.WriteLine($@"Table '{table.Table}' saved. Elapsed time: {stopwatch.Elapsed.TotalMilliseconds / 1000:0.####}s");
                    return fileName;
                }
                //stopwatch.Stop();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return string.Empty;
            }
        }

        public int BulkLoad(string srcSchemaName, string destSchemaName, string tableName, string dumpFileName, bool deleteFile = true)
        {
            try
            {
                //var stopwatch = new Stopwatch();

                if (string.IsNullOrWhiteSpace(srcSchemaName) || string.IsNullOrWhiteSpace(destSchemaName) ||
                    string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(dumpFileName))
                    throw new ArgumentNullException();

                if (!File.Exists(dumpFileName))
                    throw new FileNotFoundException();

                if (!(Path.HasExtension(dumpFileName) && Path.GetExtension(dumpFileName) == ".csv"))
                    throw new FileFormatException();

                //var queryString = MakeLoadQuery(dumpFileName, srcSchemaName, destSchemaName, tableName);

                string createQuery;
                using (var connection = ConnectionFactory(_srcString))
                {
                    createQuery = connection.Query($"show create table {srcSchemaName}.{tableName}").ToList()
                        .Select(x => (IDictionary<string, object>)x).ToList()[0]["Create Table"].ToString();
                }

                using (var connection = ConnectionFactory(_destString))
                {
                    //stopwatch.Start();
                    var sqlMode = connection.Query<string>("select @@SESSION.sql_mode").First();
                    var noStrictMode = sqlMode.Split(',').Where(mode => mode != "STRICT_TRANS_TABLES")
                        .Aggregate("", (current, mode) => current + mode + ",");
                    noStrictMode = noStrictMode.TrimEnd(',');

                    connection.Execute($"set session sql_mode = \"{noStrictMode}\"");
                    connection.Execute("set global local_infile = true");
                    connection.Execute("set foreign_key_checks = 0");
                    connection.Execute("set autocommit = 0");
                    connection.Execute("set unique_checks = 0");

                    connection.Execute(createQuery.Replace("CREATE TABLE ", $"CREATE TABLE IF NOT EXISTS {destSchemaName}."));

                    //Console.WriteLine($@"Tables created. Elapsed time: {stopwatch.Elapsed.TotalMilliseconds / 1000:0.####}s");

                    //stopwatch.Restart();

                    // TODO: 1. '0' should be false in bit(1).
                    // TODO: 2. Empty value should be NULL.
                    // TODO: 3. If data itself has '\n', DB doesn't work correctly.

                    var bulkLoader = new MySqlBulkLoader(connection)
                    {
                        Local = true,
                        TableName = $"{destSchemaName}.{tableName}",
                        FieldTerminator = ",",
                        LineTerminator = "\n",
                        FileName = dumpFileName,
                        NumberOfLinesToSkip = 1,
                        ConflictOption = MySqlBulkLoaderConflictOption.Replace,
                        FieldQuotationCharacter = '\"',
                        FieldQuotationOptional = false
                    };

                    var count = bulkLoader.Load();

                    /*var count = 0;
                    using (var transaction = connection.BeginTransaction())
                    {
                        count = connection.Execute(queryString);
                        transaction.Commit();
                    }*/

                    if (deleteFile)
                        File.Delete(dumpFileName);

                    //Console.WriteLine($@"Table '{table.Table}' loaded. Count: {count}, Elapsed time: {stopwatch.Elapsed.TotalMilliseconds / 1000:0.####}s");

                    connection.Execute($"set session sql_mode = \"{sqlMode}\"");
                    connection.Execute("set global local_infile = false");
                    connection.Execute("set foreign_key_checks = 1");
                    connection.Execute("set autocommit = 1");
                    connection.Execute("set unique_checks = 1");
                    //stopwatch.Stop();

                    return count;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 0;
            }
        }
        
        private string MakeLoadQuery(string dumpFileName, string srcSchemaName, string dstSchemaName, string tableName)
        {
            var columns = FindColumns(srcSchemaName, tableName, false);
            var query = $"load data local infile '{dumpFileName}' "
                        + $"replace into table {dstSchemaName}.{tableName} "
                        + "fields terminated by ',' optionally enclosed by '\"'"
                        + "lines terminated by '\n'"
                        + "ignore 1 lines (";

            for (var i = 0; i < columns.Count; i++)
            {
                query += "@var" + $"{i + 1}";
                if (i != columns.Count - 1)
                    query += ", ";
            }

            query += ") set ";

            for (var i = 0; i < columns.Count; i++)
            {
                if (columns[i].Type == "bit(1)")
                    query += $"{columns[i].Name} = CAST(CONV(@var{i + 1}, 10, 2) as unsigned),";
                else if (columns[i].Nullable)
                    query += $"{columns[i].Name} = NULLIF(@var{i + 1},''),";
                else
                    query += $"{columns[i].Name} = @var{i + 1},";
            }

            query = query.TrimEnd(',');
            return query;
        }
        #endregion
    }
}
