﻿using DbInterface;
using DbInterface.Interfaces;
using DbLogger.Models;
using System;
using System.Data;

namespace OracleLibrary.Operations
{
    public class OraCheck : ICheckOperations
    {
        OraExecute m_Execute { get; set; }
        public OraCheck()
        {
            m_Execute = new OraExecute();
        }

        public bool ColumnExists(string tableName, string columnName)
        {
            try
            {
                var result = false;

                var sql = string.Format("SELECT * FROM {0} WHERE ColumnName = '{1}'", tableName, columnName);
                var tblSchema = m_Execute.ExecuteReadTableSchema(sql);

                foreach (DataRow dr in tblSchema.Rows)
                {
                    if (dr["ColumnName"].ToString() == columnName)
                    {
                        result = true;
                        break;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                SLLog.WriteError(new LogData
                {
                    Source = ToString(),
                    FunctionName = "ColumnExists Error!",
                    Ex = ex,
                });
                if (Settings.ThrowExceptions) throw new Exception("ColumnExists Error!", ex);
                return false;
            }
        }

        public bool ColumnValueExists(string table, string column, string value)
        {
            try
            {
                var resultObj = m_Execute.ExecuteScalar(string.Format(@"SELECT {1} FROM {0} WHERE {1} = '{2}'", table, column, value));
                return resultObj != null;
            }
            catch (Exception ex)
            {
                SLLog.WriteError(new LogData
                {
                    Source = ToString(),
                    FunctionName = "ColumnValueExists Error!",
                    Ex = ex,
                });
                if (Settings.ThrowExceptions) throw new Exception("ColumnValueExists Error!", ex);
                return false;
            }
        }

        public bool TableExists(string table)
        {
            try
            {
                var result = m_Execute.ExecuteScalar(string.Format(@"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='{0}'", table));
                if (result == null) return false;

                return result.ToString() == table;
            }
            catch (Exception ex)
            {
                SLLog.WriteError(new LogData
                {
                    Source = ToString(),
                    FunctionName = "TableExists Error!",
                    Ex = ex,
                });
                if (Settings.ThrowExceptions) throw new Exception("TableExists Error!", ex);
                return false;
            }
        }

        public bool DatabaseExists(string databaseName)
        {
            try
            {
                var result = m_Execute.ExecuteScalar(string.Format(@"SELECT name FROM master.dbo.sysdatabases WHERE name = '{0}'", databaseName));
                if (result == null) return false;

                return result.ToString() == databaseName;
            }
            catch (Exception ex)
            {
                SLLog.WriteError(new LogData
                {
                    Source = ToString(),
                    FunctionName = "DatabaseExists Error!",
                    Ex = ex,
                });
                if (Settings.ThrowExceptions) throw new Exception("DatabaseExists Error!", ex);
                return false;
            }
        }
    }
}
