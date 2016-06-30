﻿using DbInterface;
using DbInterface.Helpers;
using DbInterface.Interfaces;
using DbInterface.Models;
using DbLogger.Models;
using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace OracleLibrary.Operations
{
    public class OraExecute : IExecuteOperations
    {
        public OraExecute()
        {

        }

        public int ExecuteNonQuery(string sql)
        {
            int rowsUpdated = 0;
            try
            {
                var con = CONNECTION.OpenCon();

                OracleCommand cmd = new OracleCommand(sql, con);
                rowsUpdated = cmd.ExecuteNonQuery();

                cmd.Dispose();
                CONNECTION.CloseCon(con);
            }
            catch (Exception ex)
            {
                SLLog.WriteError(new LogData
                {
                    Source = ToString(),
                    FunctionName = "ExecuteNonQuery Error!",
                    Ex = ex,
                });
                return -1;
            }

            return rowsUpdated;
        }

        public int ExecuteNonQuery(List<string> sqlList)
        {
            int rowsUpdated = 0;
            try
            {
                var con = CONNECTION.OpenCon();

                foreach (var sql in sqlList)
                {
                    var cmd = new OracleCommand(sql, con);
                    rowsUpdated += cmd.ExecuteNonQuery();

                    cmd.Dispose();
                }

                CONNECTION.CloseCon(con);
            }
            catch (Exception ex)
            {
                SLLog.WriteError(new LogData
                {
                    Source = ToString(),
                    FunctionName = "ExecuteNonQuery Error!",
                    Ex = ex,
                });
                return -1;
            }

            return rowsUpdated;
        }

        public object ExecuteScalar(string sql)
        {
            object value = null;
            try
            {
                var con = CONNECTION.OpenCon();

                var cmd = new OracleCommand(sql, con);
                value = cmd.ExecuteScalar();

                cmd.Dispose();
                CONNECTION.CloseCon(con);
            }
            catch (Exception ex)
            {
                SLLog.WriteError(new LogData
                {
                    Source = ToString(),
                    FunctionName = "ExecuteScalar Error!",
                    Ex = ex,
                });
                return null;
            }

            return value;
        }

        public DataTable ExecuteReadTable(string sql)
        {
            var dt = new DataTable();
            try
            {
                var con = CONNECTION.OpenCon();

                var cmd = new OracleCommand(sql, con);
                var reader = cmd.ExecuteReader();

                var schemaTbl = reader.GetSchemaTable();
                dt.Load(reader);


                //GetTableName
                if (schemaTbl.Rows.Count <= 0) return dt;
                var schemaRow = schemaTbl.Rows[0];
                var tableName = schemaRow[DbCIC.BaseTableName].ToString();

                if (string.IsNullOrEmpty(tableName))
                    tableName = ExecuteReadTableName(dt.Columns[0].ColumnName);

                dt.TableName = tableName;

                reader.Close();

                cmd.Dispose();
                CONNECTION.CloseCon(con);
            }
            catch (Exception ex)
            {
                SLLog.WriteError(new LogData
                {
                    Source = ToString(),
                    FunctionName = "ExecuteReadTable Error!",
                    Ex = ex,
                });
                return null;
            }

            return dt;
        }

        public DataTable ExecuteReadTableSchema(string sql)
        {
            var dt = new DataTable();
            try
            {
                var con = CONNECTION.OpenCon();

                var cmd = new OracleCommand(sql, con);

                var reader = cmd.ExecuteReader();
                dt = reader.GetSchemaTable();

                reader.Close();

                cmd.Dispose();
                CONNECTION.CloseCon(con);
            }
            catch (Exception ex)
            {
                SLLog.WriteError(new LogData
                {
                    Source = ToString(),
                    FunctionName = "ExecuteReadTableSchema Error!",
                    Ex = ex,
                });
                return null;
            }

            return dt;
        }

        public string ExecuteReadTableName(string columnName)
        {
            try
            {
                var dt = new DataTable();
                var sql = string.Format(@"SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE column_name = '{0}'", columnName);

                var con = CONNECTION.OpenCon();

                var cmd = new OracleCommand(sql, con);
                var reader = cmd.ExecuteReader();

                dt.Load(reader);

                cmd.Dispose();
                CONNECTION.CloseCon(con);

                if (dt == null || dt.Rows.Count <= 0) return string.Empty;
                var dr = dt.Rows[0];
                return dr[DbCIC.TableName].ToString();
            }
            catch (Exception ex)
            {
                SLLog.WriteError(new LogData
                {
                    Source = ToString(),
                    FunctionName = "ExecuteReadTableName Error!",
                    Ex = ex,
                });
                return null;
            }
        }

        public bool RenewTbl(string tableName, List<ColumnData> columns)
        {
            try
            {
                var result = true;

                var scriptList = new List<string>();
                var oldColumns = new List<ColumnData>();
                var timeStamp = DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second + "_" +
                                DateTime.Now.Day.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Year.ToString();

                //Standart Felder erstellen
                ColumnHelper.SetDefaultColumns(columns);

                var oldTblSchema = ExecuteReadTableSchema(string.Format("SELECT * FROM {0}", tableName));
                foreach (DataRow row in oldTblSchema.Rows)
                {
                    var oldCol = columns.Find(i => i.Name == row["ColumnName"].ToString());
                    if (oldCol == null) continue;
                    oldCol.existsInDB = true;

                    oldColumns.Add(oldCol);
                }

                scriptList.Add(string.Format("ALTER TABLE {0} RENAME TO {0}_OLD{1}", tableName, timeStamp));

                scriptList.Add(ScriptHelper.GetCreateTableSql(tableName, columns));

                var insertSQL = string.Format("INSERT INTO {0} ({1}) ", tableName, ColumnHelper.GetColumnString(columns, true));
                insertSQL += string.Format("SELECT {2} FROM {0}_OLD{1}", tableName, timeStamp, ColumnHelper.GetColumnString(columns));
                scriptList.Add(insertSQL);


                var exResult = ExecuteNonQuery(scriptList);
                if (exResult == -1)
                    result = false;

                return result;
            }
            catch (Exception ex)
            {
                SLLog.WriteError(new LogData
                {
                    Source = ToString(),
                    FunctionName = "RenewTbl Error!",
                    Ex = ex,
                });
                return false;
            }
        }

        public bool RenewTbl(string tableName, Dictionary<string, string> columns)
        {
            try
            {
                var colList = new List<ColumnData>();

                foreach (var col in columns)
                {
                    var colData = new ColumnData
                    {
                        Name = col.Key,
                        Type = col.Value,
                    };
                    if (col.Value == DbDEF.TxtNotNull)
                        colData.DefaultValue = "default";

                    colList.Add(colData);
                }

                return RenewTbl(tableName, colList);
            }
            catch (Exception ex)
            {
                SLLog.WriteError(new LogData
                {
                    Source = ToString(),
                    FunctionName = "RenewTbl Error!",
                    Ex = ex,
                });
                return false;
            }
        }
    }
}