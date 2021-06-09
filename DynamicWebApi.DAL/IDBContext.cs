/* Copyright Chetan N Mandhania */
using System.Data;
using System.Data.SqlClient;
using System.Xml;

namespace DynamicWebApi.DAL
{
    public interface IDBContext
    {
        SqlConnection GetConnection();
        int ExecuteNonQuery(SqlCommand sqlCommand);
        object ExecuteScalar(SqlCommand sqlCommand);
        SqlDataReader ExecuteDataReader(SqlCommand sqlCommand);
        DataSet FillDataSet(SqlDataAdapter sqlDataAdapter);
        DataTable FillDataTable(SqlDataAdapter sqlDataAdapter);
        XmlReader ExecuteXmlReader(SqlCommand sqlCommand);
    }
}