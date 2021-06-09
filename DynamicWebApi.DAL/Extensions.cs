/* Copyright Chetan N Mandhania */
using Newtonsoft.Json;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DynamicWebApi.DAL
{
    [DebuggerStepThrough]
    public static class Extensions
    {
        [DebuggerHidden]
        [DebuggerStepThrough]
        public static dynamic DataTableToJSONWithJSONNet(this DataTable dt) => JsonConvert.SerializeObject(dt, Formatting.Indented);
        [DebuggerHidden]
        [DebuggerStepThrough]
        public static dynamic DataSetToJSONWithJSONNet(this DataSet ds) => JsonConvert.SerializeObject(ds, Formatting.Indented);
        [DebuggerHidden]
        [DebuggerStepThrough]
        public static dynamic DataTableToCSV(this DataTable dt)
        {
            StringBuilder sb = new();
            var columnNames = dt.Columns.Cast<DataColumn>().Select(o => o.ColumnName.Replace("\"", "\"\""));
            _ = sb.AppendLine(string.Join(",", columnNames));
            foreach (var fields in from DataRow row in dt.Rows let fields = row.ItemArray.Select(fields => string.Concat("\"", fields.ToString().Replace("\"", "\"\""), "\"")).ToList() select fields) _ = sb.AppendLine(string.Join(",", fields));
            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}
