/* Copyright Chetan N Mandhania */
using DynamicWebApi.Common.Enums;
using DynamicWebApi.Common.Exceptions;
using DynamicWebApi.Common.Helpers;
using DynamicWebApi.Common.Interface;
using DynamicWebApi.DAL;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using db = DynamicWebApi.DAL.DataBaseManager;

namespace DynamicWebApi.Repository
{
    [DebuggerStepThrough]
    public class BaseRepository
    {
        private readonly ILogger<BaseRepository> _logger;
        private readonly IDBContext _context;
        [DebuggerHidden]
        [DebuggerStepThrough]
        public BaseRepository(ILogger<BaseRepository> logger, IDBContext context)
        {
            _logger = logger;
            _context = context;
        }
        [DebuggerHidden]
        [DebuggerStepThrough]
        public string GetQuery(string key, int userId, ExecutionType executionType, List<DbParameters> parameters)
        {
            var query = "";
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName, "queries.json")));
            var isResolveUserId = (Global.Configuration?.GetSection("IsResolveUserId").Value + "").ToLower() == "true";
            foreach (var item in from item in dict where (item.Key + "").ToLower() == (key + "").ToLower() select query = isResolveUserId ? item.Value.Replace("$UserId", userId.ToString()) : item.Value) ;
            switch (executionType)
            {
                case ExecutionType.DataSetText:
                case ExecutionType.DataTableText:
                case ExecutionType.NonQueryText:
                case ExecutionType.ScalarText:
                    {
                        parameters.ForEach((item) =>
                        {
                            var val = item.Value + "";
                            if (!string.IsNullOrEmpty(val) && val.Length > 0 && !val.Contains(","))
                            {
                                val = val.Replace("'", "''");
                                val = "'" + val + "'";
                            }
                        });
                        break;
                    }
            }
            if (string.IsNullOrEmpty(query))
            {
                _logger.LogError("Query not mapped for this request.");
                throw new CustomException((int)HttpStatusCode.InternalServerError, "Internal Server Error!!!", "Query not mapped for this request.");
            }
            return query;
        }
        [DebuggerHidden]
        [DebuggerStepThrough]
        public static List<DbParameters> GetParameters(List<RequestSpecification> requests)
        {
            List<DbParameters> list = new();
            foreach (var (item, l) in from item in requests let l = new DbParameters() select (item, l))
            {
                l.Name = item.PropertyName;
                l.Type = item.GetDBType();
                l.Value = item.GetValue();
                if (l.Type != SqlDBType.UnKnown) list.Add(l);
            }
            return list;
        }
        [DebuggerHidden]
        [DebuggerStepThrough]
        public dynamic ExecuteQuery(string query, ExecutionType executionType, List<DbParameters> list, OutPutType outPutType) => executionType switch
        {
            ExecutionType.DataSetText => outPutType == OutPutType.Excel ? ExcelExportHelper.GenerateExcel(new db(_context).ExecDataSet(query, null)) : new db(_context).ExecDataSet(query, null).DataSetToJSONWithJSONNet(),
            ExecutionType.DataTableText => outPutType == OutPutType.Excel ? ExcelExportHelper.GenerateExcel(new db(_context).ExecDataTable(query, null)) : outPutType == OutPutType.CSV ?  new db(_context).ExecDataTable(query, null).DataTableToCSV() : new db(_context).ExecDataTable(query, null).DataTableToJSONWithJSONNet(),
            ExecutionType.DataSetProcedure => outPutType == OutPutType.Excel ? ExcelExportHelper.GenerateExcel(new db(_context).ExecDataSetProc(query, list)) : new db(_context).ExecDataSetProc(query, list).DataSetToJSONWithJSONNet(),
            ExecutionType.DataTableProcedure => outPutType == OutPutType.Excel ? ExcelExportHelper.GenerateExcel(new db(_context).ExecDataTableProc(query, list)) : outPutType == OutPutType.CSV ? new db(_context).ExecDataTableProc(query, list).DataTableToCSV() : new db(_context).ExecDataTableProc(query, list).DataTableToJSONWithJSONNet(),
            ExecutionType.NonQueryText => new db(_context).ExecNonQuery(query, null),
            ExecutionType.ScalarText => new db(_context).ExecScalar(query, null),
            ExecutionType.NonQueryProcedure => new db(_context).ExecNonQueryProc(query, list),
            ExecutionType.ScalarProcedure => new db(_context).ExecScalarProc(query, list),
        };
    }
}
