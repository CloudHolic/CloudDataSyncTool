using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.TypeConversion;
using Dapper;
using MySqlConnector;

namespace DbTest
{
    public class DbModule
    {
        private readonly string _srcString;
        private readonly string _destString;

        public DbModule(Connection srcConString, Connection destConString)
        {
            _srcString = MakeConnectionString(srcConString);
            _destString = MakeConnectionString(destConString);
        }

        private static string MakeConnectionString(Connection conString)
        {
            var db = string.IsNullOrEmpty(conString.DbName) ? string.Empty : $"database={conString.DbName}";
            return $"host={conString.Host};port={conString.Port};user id={conString.Id};password={conString.Password};{db};AllowLoadLocalInfile=true";
        }

        private static MySqlConnection ConnectionFactory(string connString)
        {
            var connection = new MySqlConnection(connString);
            connection.Open();
            return connection;
        }

        public List<string> FindSchemas()
        {
            List<string> schemaList;

            using (var connection = ConnectionFactory(_srcString))
            {
                schemaList = connection.Query<string>("select SCHEMA_NAME from information_schema.SCHEMATA").ToList();
            }

            return schemaList;
        }

        public List<string> FindTables(string schemaName)
        {
            List<string> tableList;

            using (var connection = ConnectionFactory(_srcString))
            {
                tableList = connection.Query<string>("select TABLE_NAME from information_schema.TABLES where TABLE_SCHEMA = @Schema and TABLE_TYPE != 'VIEW'",
                    new {Schema = schemaName}).ToList();
            }

            return tableList;
        }

        public List<ColumnType> FindColumns(string schemaName, string tableName)
        {
            List<IDictionary<string, object>> result;

            using (var connection = ConnectionFactory(_srcString))
            {
                var rawResult = connection.Query("select COLUMN_NAME, COLUMN_TYPE from information_schema.COLUMNS where TABLE_SCHEMA = @Schema AND TABLE_NAME = @Table",
                    new {Schema = schemaName, Table = tableName}).ToList();
                result = rawResult.Select(x => (IDictionary<string, object>) x).ToList();
            }

            return result.Select(column => new ColumnType
                {
                    Name = column["COLUMN_NAME"].ToString(),
                    Type = column["COLUMN_TYPE"].ToString(),
                    MaxLength = 0
                }).ToList();
        }

        public string FindCreateSql(string schemaName, string tableName)
        {
            List<IDictionary<string, object>> result;

            using (var connection = ConnectionFactory(_srcString))
            {
                var rawResult = connection.Query($"show create table {schemaName}.{tableName}").ToList();
                result = rawResult.Select(x => (IDictionary<string, object>) x).ToList();
            }
            
            return result[0]["Create Table"].ToString();
        }

        public void SaveTables(string schemaName, List<TableName> tableNameList)
        {
            try
            {
                var stopwatch = new Stopwatch();

                using (var connection = ConnectionFactory(_srcString))
                {
                    foreach(var table in tableNameList)
                    {
                        if (string.IsNullOrEmpty(table.FileName) || string.IsNullOrEmpty(table.Table))
                            throw new ArgumentNullException();

                        stopwatch.Restart();

                        var records = connection.Query($"select * from {schemaName}.{table.Table}").ToList();

                        using (var streamWriter = new StreamWriter(table.FileName))
                        using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.CurrentCulture))
                        {
                            var options = new TypeConverterOptions {Formats = new[] {"yyyy/MM/dd"}};
                            csvWriter.Context.TypeConverterOptionsCache.AddOptions<DateTime>(options);
                            csvWriter.WriteRecords(records);
                        }

                        Console.WriteLine($"Table '{table.Table}' saved. Elapsed time: {stopwatch.Elapsed.TotalMilliseconds / 1000:0.####}s");
                    }
                }

                stopwatch.Stop();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public List<int> BulkLoad(string srcSchema, string destSchema, List<TableName> tableNameList, bool deleteFiles = true)
        {
            try
            {
                var countList = new List<int>();
                var stopwatch = new Stopwatch();

                using (var connection = ConnectionFactory(_destString))
                {
                    stopwatch.Start();

                    connection.Execute("set global local_infile = true");
                    connection.Execute("set foreign_key_checks = 0");
                    foreach (var table in tableNameList)
                    {
                        if (string.IsNullOrEmpty(table.FileName) || string.IsNullOrEmpty(table.Table))
                            throw new ArgumentNullException();

                        var createQuery = connection.Query($"show create table {srcSchema}.{table.Table}").ToList()
                            .Select(x => (IDictionary<string, object>) x).ToList()[0]["Create Table"].ToString();
                        connection.Execute(createQuery.Replace("CREATE TABLE ", $"CREATE TABLE IF NOT EXISTS {destSchema}."));
                    }

                    Console.WriteLine($"Tables created. Elapsed time: {stopwatch.Elapsed.TotalMilliseconds / 1000:0.####}s");
                    
                    for (var i = 0; i < tableNameList.Count; i++)
                    {
                        stopwatch.Restart();

                        var bulkLoader = new MySqlBulkLoader(connection)
                        {
                            Local = true,
                            TableName = $"{destSchema}.{tableNameList[i].Table}",
                            FieldTerminator = ",",
                            LineTerminator = "\n",
                            FileName = tableNameList[i].FileName,
                            NumberOfLinesToSkip = 1,
                            ConflictOption = MySqlBulkLoaderConflictOption.Replace
                        };

                        var count = bulkLoader.Load();
                        countList.Add(count);
                        if (deleteFiles)
                            File.Delete(tableNameList[i].FileName);

                        Console.WriteLine($"Table '{tableNameList[i].Table}' loaded. Count: {count}, Elapsed time: {stopwatch.Elapsed.TotalMilliseconds / 1000:0.####}s");
                    }


                    connection.Execute("set foreign_key_checks = 1");
                    connection.Execute("set global local_infile = false");
                    stopwatch.Stop();

                    return countList;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
    }
}
