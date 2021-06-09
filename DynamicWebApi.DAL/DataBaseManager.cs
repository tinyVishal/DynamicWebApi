/* Copyright Chetan N Mandhania */
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Xml;

namespace DynamicWebApi.DAL
{
    [DebuggerStepThrough]
    public class DataBaseManager : IDisposable
    {
        private readonly IDBContext _context;
        private SqlConnection _conn;
        private bool _disposed;
        [DebuggerHidden]
        [DebuggerStepThrough]
        public DataBaseManager(IDBContext context) => _conn = (_context = context).GetConnection();
        [DebuggerHidden]
        [DebuggerStepThrough]
        public SqlCommand CreateCommand(string query, CommandType type, List<DbParameters> parameters)
        {
            SqlCommand cmd = new(query, _conn);
            cmd.CommandType = type;
            cmd.CommandTimeout = int.TryParse(DBContext.Configuration?.GetSection("ConnecttionTimeOut").Value, out int result) ? result : 1200;
            if(null!= parameters && parameters.Count > 0)
            {
                foreach(var item in parameters)
                {
                    if(item.Type == SqlDBType.Structured && !item.IsOutParameter)
                    {
                        cmd.Parameters.Add(new() { SqlValue = item.Value, ParameterName = !string.IsNullOrEmpty(item.Name) && item.Name.Contains("@") ? item.Name : "@" + item.Name, TypeName = "dbo." + item.Name, SqlDbType = SqlDbType.Structured, Direction = ParameterDirection.Input });
                        continue;
                    }
                    var p = new SqlParameter();
                    if (item.IsOutParameter)
                    {
                        p.Direction = ParameterDirection.Output;
                        p.Size = item.Size;
                    }
                    else
                    {
                        p.Direction = ParameterDirection.Input;
                        p.Value = item.Value;
                    }
                    p.ParameterName = !string.IsNullOrEmpty(item.Name) && item.Name.Contains("@") ? item.Name : "@" + item.Name;
                    if (null != item.Type && item.Type.HasValue && item.Type.Value == SqlDBType.UnKnown) p.DbType = (DbType)item.Type.Value;
                    cmd.Parameters.Add(p);
                }
            }
            return cmd;
        }
        [DebuggerHidden]
        [DebuggerStepThrough]
        public int ExecNonQuery(string query, List<DbParameters> parameters) => _context.ExecuteNonQuery(CreateCommand(query, CommandType.Text, parameters));
        [DebuggerHidden]
        [DebuggerStepThrough]
        public int ExecNonQueryProc(string query, List<DbParameters> parameters) => _context.ExecuteNonQuery(CreateCommand(query, CommandType.StoredProcedure, parameters));
        [DebuggerHidden]
        [DebuggerStepThrough]
        public object ExecScalar(string query, List<DbParameters> parameters) => _context.ExecuteScalar(CreateCommand(query, CommandType.Text, parameters));
        [DebuggerHidden]
        [DebuggerStepThrough]
        public object ExecScalarProc(string query, List<DbParameters> parameters) => _context.ExecuteScalar(CreateCommand(query, CommandType.StoredProcedure, parameters));
        [DebuggerHidden]
        [DebuggerStepThrough]
        public SqlDataReader ExecDataReader(string query, List<DbParameters> parameters) => _context.ExecuteDataReader(CreateCommand(query, CommandType.Text, parameters));
        [DebuggerHidden]
        [DebuggerStepThrough]
        public SqlDataReader ExecDataReaderProc(string query, List<DbParameters> parameters) => _context.ExecuteDataReader(CreateCommand(query, CommandType.StoredProcedure, parameters));
        [DebuggerHidden]
        [DebuggerStepThrough]
        public XmlReader ExecXmlReader(string query, List<DbParameters> parameters) => _context.ExecuteXmlReader(CreateCommand(query, CommandType.Text, parameters));
        [DebuggerHidden]
        [DebuggerStepThrough]
        public XmlReader ExecXmlReaderProc(string query, List<DbParameters> parameters) => _context.ExecuteXmlReader(CreateCommand(query, CommandType.StoredProcedure, parameters));
        [DebuggerHidden]
        [DebuggerStepThrough]
        public DataSet ExecDataSet(string query, List<DbParameters> parameters) => _context.FillDataSet(new SqlDataAdapter(CreateCommand(query, CommandType.Text, parameters)));
        [DebuggerHidden]
        [DebuggerStepThrough]
        public DataSet ExecDataSetProc(string query, List<DbParameters> parameters) => _context.FillDataSet(new SqlDataAdapter(CreateCommand(query, CommandType.StoredProcedure, parameters)));
        [DebuggerHidden]
        [DebuggerStepThrough]
        public DataTable ExecDataTable(string query, List<DbParameters> parameters) => _context.FillDataTable(new SqlDataAdapter(CreateCommand(query, CommandType.Text, parameters)));
        [DebuggerHidden]
        [DebuggerStepThrough]
        public DataTable ExecDataTableProc(string query, List<DbParameters> parameters) => _context.FillDataTable(new SqlDataAdapter(CreateCommand(query, CommandType.StoredProcedure, parameters)));
        [DebuggerHidden]
        [DebuggerStepThrough]
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        [DebuggerHidden]
        [DebuggerStepThrough]
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if(disposing && null != _conn)
                {
                    _conn.Dispose();
                    _conn = null;
                }
                _disposed = true;
            }
        }
    }
    [DebuggerStepThrough]
    public class DbParameters
    {
        [DebuggerHidden]
        public string Name { get; set; }
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        [DebuggerHidden]
        public object? Value { get; set; }
        [DebuggerHidden]
        public SqlDBType? Type { get; set; }
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        [DebuggerHidden]
        public bool IsOutParameter { get; set; }
        [DebuggerHidden]
        public int Size { get; set; }
    }
    public enum SqlDBType
    {
        AnsiString, Binary, Byte, Boolean, Currency, Date, DateTime, Decimal, Double, Guid, Int16, Int32, Int64, Object, SByte, Single, String, Time, UInt16, UInt32, UInt64, VarNumeric, AnsiStringFixedLength, StringFixedLength, Xml, DateTime2, DateTimeOffset, Structured, UnKnown
    }

}
