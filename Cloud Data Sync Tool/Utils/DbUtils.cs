using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CloudSync.Models;
using Dapper;
using MySqlConnector;
using Z.BulkOperations;

namespace CloudSync.Utils
{
    public class DbUtils
    {
        private readonly string _srcString;
        private readonly string _destString;
        private readonly int _maxRows;

        public DbUtils(Connection srcConString, Connection destConString)
        {
            _srcString = MakeConnectionString(srcConString);
            _destString = MakeConnectionString(destConString);
            _maxRows = ConfigManager.Instance.Config.MaxRows;
        }

        public static bool CheckConnection(Connection conn)
        {
            bool result;
            var connString = MakeConnectionString(conn);
            LastErrorManager.Instance.SetLastError();

            try
            {
                var connection = new MySqlConnection(connString);
                connection.Open();
                result = true;
                connection.Close();
            }
            catch(Exception e)
            {
                result = false;
                LastErrorManager.Instance.SetLastError(e);
            }

            return result;
        }

        public List<string> FindSchemas(bool isSrc = true)
        {
            List<string> schemaList;
            LastErrorManager.Instance.SetLastError();

            try
            {
                using (var connection = ConnectionFactory(isSrc ? _srcString : _destString))
                {
                    schemaList = connection.Query<string>("select SCHEMA_NAME from information_schema.SCHEMATA").ToList();
                    schemaList.Sort();
                }
            }
            catch (Exception e)
            {
                LastErrorManager.Instance.SetLastError(e);
                schemaList = null;
            }
            
            return schemaList;
        }

        public List<string> FindTables(string schemaName, bool isSrc = true)
        {
            List<string> tableList;
            LastErrorManager.Instance.SetLastError();

            try
            {
                using (var connection = ConnectionFactory(isSrc ? _srcString : _destString))
                {
                    tableList = connection.Query<string>("select TABLE_NAME from information_schema.TABLES "
                                                         + "where TABLE_SCHEMA = @Schema and TABLE_TYPE != 'VIEW'",
                        new { Schema = schemaName }).ToList();
                }
            }
            catch (Exception e)
            {
                LastErrorManager.Instance.SetLastError(e);
                tableList = null;
            }

            tableList?.Sort();
            return tableList;
        }

        public int BulkCopy(string srcSchemaName, string destSchemaName, string tableName)
        {
            var copiedRows = 0;
            LastErrorManager.Instance.SetLastError();

            try
            {
                if (string.IsNullOrEmpty(srcSchemaName) || string.IsNullOrEmpty(destSchemaName) || string.IsNullOrEmpty(tableName))
                    throw new ArgumentNullException();

                using (var srcConn = ConnectionFactory(_srcString))
                {
                    var createQuery = srcConn.Query($"show create table {srcSchemaName}.{tableName}").ToList()
                        .Select(x => (IDictionary<string, object>)x).ToList()[0]["Create Table"].ToString();
                    var pk = srcConn.Query<string>("select column_name from information_schema.COLUMNS "
                                                   + "where TABLE_SCHEMA = @Schema and TABLE_NAME = @Table and COLUMN_KEY = 'PRI'",
                        new { Schema = srcSchemaName, Table = tableName }).ToList();
                    var columns = FindColumns(srcSchemaName, tableName);
                    
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

                        //srcConn.Execute("set session sort_buffer_size=2147483648");
                        
                        foreach(var sample in LazyLoad(srcConn, srcSchemaName, tableName, columns))
                        {
                            if (sample.Rows.Count == 0)
                                break;
                            
                            using (var dstTrans = dstConn.BeginTransaction())
                            {
                                var bulk = new BulkOperation(dstConn)
                                {
                                    DestinationTableName = $"{destSchemaName}.{tableName}",
                                    BatchTimeout = 0,
                                    Transaction = dstTrans
                                };
                                bulk.BulkInsert(sample);

                                copiedRows += sample.Rows.Count;
                                dstTrans.Commit();
                            }
                        }

                        dstConn.Execute($"set session sql_mode = \"{sqlMode}\"");
                        dstConn.Execute("set global local_infile = false");
                        dstConn.Execute("set foreign_key_checks = 1");
                        dstConn.Execute("set autocommit = 1");
                        dstConn.Execute("set unique_checks = 1");
                    }

                    return copiedRows;
                }
            }
            catch (Exception e)
            {
                LastErrorManager.Instance.SetLastError(e);
                return copiedRows;
            }
        }
        
        private static string MakeConnectionString(Connection conString)
        {
            // SslMode=VerifyCA;
            return $"host={conString.Host};port={conString.Port};user id={conString.Id};password={SecureStringUtils.ConvertToString(conString.Password)};"
                   + "AllowLoadLocalInfile=true;Allow User Variables=true;";
        }

        private static MySqlConnection ConnectionFactory(string connString)
        {
            var connection = new MySqlConnection(connString);
            connection.Open();
            return connection;
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

        private IEnumerable<DataTable> LazyLoad(IDbConnection conn, string srcSchema, string srcTable, IReadOnlyCollection<ColumnType> types)
        {
            var dataTable = new DataTable();
            var record = conn.ExecuteReader($"select * from {srcSchema}.{srcTable}", commandTimeout: 0);

            foreach (var type in types)
                dataTable.Columns.Add(type.Name);

            while (record.Read())
            {
                var row = dataTable.NewRow();
                for (var i = 0; i < types.Count; i++)
                {
                    if (record.GetValue(i) is DateTime dateTime)
                        row[record.GetName(i)] = dateTime.ToString("yyyy-MM-dd hh:mm:ss");
                    else
                        row[record.GetName(i)] = record.GetValue(i);
                }

                dataTable.Rows.Add(row);

                if (dataTable.Rows.Count < _maxRows)
                    continue;

                yield return dataTable;
                dataTable.Rows.Clear();
            }

            yield return dataTable;
        }
    }
}