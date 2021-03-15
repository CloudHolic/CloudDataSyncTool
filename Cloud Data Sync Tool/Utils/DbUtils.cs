using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using CloudSync.Models;
using CsvHelper;
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
            return $"host={conString.Host};port={conString.Port};user id={conString.Id};password={conString.GetPassword()};AllowLoadLocalInfile=true";
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
                tableList = connection.Query<string>("select TABLE_NAME from information_schema.TABLES where TABLE_SCHEMA = @Schema and TABLE_TYPE != 'VIEW'",
                    new { Schema = schemaName }).ToList();
            }

            return tableList;
        }

        public List<ColumnType> FindColumns(string schemaName, string tableName, bool isSrc = true)
        {
            List<IDictionary<string, object>> result;

            using (var connection = ConnectionFactory(isSrc ? _srcString : _destString))
            {
                var rawResult = connection.Query("select COLUMN_NAME, COLUMN_TYPE from information_schema.COLUMNS where TABLE_SCHEMA = @Schema AND TABLE_NAME = @Table",
                    new { Schema = schemaName, Table = tableName }).ToList();
                result = rawResult.Select(x => (IDictionary<string, object>)x).ToList();
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

                var fileName = Path.Combine(dumpDirectory, $@"{schemaName}-{tableName}.csv");

                using (var connection = ConnectionFactory(_srcString))
                {
                    //stopwatch.Restart();

                    var records = connection.Query($"select * from {schemaName}.{tableName}").ToList();

                    using (var streamWriter = new StreamWriter(fileName))
                    using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.CurrentCulture))
                    {
                        var options = new TypeConverterOptions { Formats = new[] { "yyyy/MM/dd" } };
                        csvWriter.Context.TypeConverterOptionsCache.AddOptions<DateTime>(options);
                        csvWriter.WriteRecords(records);
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
                
                using (var connection = ConnectionFactory(_destString))
                {
                    //stopwatch.Start();

                    connection.Execute("set global local_infile = true");
                    connection.Execute("set foreign_key_checks = 0");
                    
                    var createQuery = connection.Query($"show create table {srcSchemaName}.{tableName}").ToList()
                        .Select(x => (IDictionary<string, object>)x).ToList()[0]["Create Table"].ToString();
                    connection.Execute(createQuery.Replace("CREATE TABLE ", $"CREATE TABLE IF NOT EXISTS {destSchemaName}."));

                    //Console.WriteLine($@"Tables created. Elapsed time: {stopwatch.Elapsed.TotalMilliseconds / 1000:0.####}s");
                    
                    //stopwatch.Restart();
                    
                    var bulkLoader = new MySqlBulkLoader(connection)
                    {
                        Local = true,
                        TableName = $"{destSchemaName}.{tableName}",
                        FieldTerminator = ",",
                        LineTerminator = "\n",
                        FileName = dumpFileName,
                        NumberOfLinesToSkip = 1,
                        ConflictOption = MySqlBulkLoaderConflictOption.Replace
                    };

                    var count = bulkLoader.Load();
                    if (deleteFile)
                        File.Delete(dumpFileName);
                    
                    //Console.WriteLine($@"Table '{table.Table}' loaded. Count: {count}, Elapsed time: {stopwatch.Elapsed.TotalMilliseconds / 1000:0.####}s");
                    
                    connection.Execute("set foreign_key_checks = 1");
                    connection.Execute("set global local_infile = false");
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
    }
}
