/* Copyright Chetan N Mandhania */
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace DynamicWebApi.DAL
{
    [DebuggerStepThrough]
    public class DBContext : IDBContext
    {
        private static string _connectionString;
        [DebuggerHidden]
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public static IConfiguration? Configuration { get; set; }
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        [DebuggerHidden]
        public static string ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_connectionString))
                {
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName, "connectionstring.json")));
                    var environment = Configuration?.GetSection("Environment").Value + "";
                    foreach (var item in from item in dict where (item.Key + "").ToLower() == environment.ToLower() select _connectionString = item.Value) ;
                }
                return _connectionString;
            }
        }
        [DebuggerStepThrough]
        [DebuggerHidden]
        public SqlDataReader ExecuteDataReader(SqlCommand sqlCommand) => sqlCommand.ExecuteReader();
        [DebuggerStepThrough]
        [DebuggerHidden]
        public int ExecuteNonQuery(SqlCommand sqlCommand) => sqlCommand.ExecuteNonQuery();
        [DebuggerStepThrough]
        [DebuggerHidden]
        public object ExecuteScalar(SqlCommand sqlCommand) => sqlCommand.ExecuteScalar();
        [DebuggerStepThrough]
        [DebuggerHidden]
        public XmlReader ExecuteXmlReader(SqlCommand sqlCommand) => sqlCommand.ExecuteXmlReader();
        [DebuggerStepThrough]
        [DebuggerHidden]
        public DataSet FillDataSet(SqlDataAdapter sqlDataAdapter)
        {
            DataSet ds = new();
            sqlDataAdapter.Fill(ds);
            return ds;
        }
        [DebuggerStepThrough]
        [DebuggerHidden]
        public DataTable FillDataTable(SqlDataAdapter sqlDataAdapter)
        {
            DataTable dt = new();
            sqlDataAdapter.Fill(dt);
            return dt;
        }
        [DebuggerStepThrough]
        [DebuggerHidden]
        public SqlConnection GetConnection()
        {
            var con = new SqlConnection(ConnectionString);
            if (con.State != ConnectionState.Open) con.Open();
            return con;
        }
    }
}
