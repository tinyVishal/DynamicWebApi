/* Copyright Chetan N Mandhania */
using DynamicWebApi.Common.Enums;
using DynamicWebApi.Common.Exceptions;
using DynamicWebApi.Common.Interface;
using DynamicWebApi.DAL;
using DynamicWebApi.DataContracts.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace DynamicWebApi.Common.Helpers
{
    [DebuggerStepThrough]
    public class Base : ControllerBase, IBase
    {
        private readonly ILogger<Base> _logger;
        private readonly IDBContext _context;
        [DebuggerHidden]
        public OutPutType OutPutType { get; set; } = OutPutType.JSON;
        [DebuggerHidden]
        [DebuggerStepThrough]
        public Base(ILogger<Base> logger, IDBContext context)
        {
            _logger = logger; _context = context;
        }
        [DebuggerHidden]
        [DebuggerStepThrough]
        public async Task<T> ExecuteAsync<T>(Func<T> f, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumer = 0)
        {
            try
            {
                _logger.LogInformation($"Starting invocation of method name : {callerName}, file path : {callerFilePath}, line number : {callerLineNumer}, DateTime : {DateTime.UtcNow.ToLongDateString()} {DateTime.UtcNow.ToLongTimeString()}");
                return await Task.Run(f);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occured while invoking  method name : {callerName}, file path : {callerFilePath}, line number : {callerLineNumer}, DateTime : {DateTime.UtcNow.ToLongDateString()} {DateTime.UtcNow.ToLongTimeString()}");
                throw new CustomException((int)HttpStatusCode.InternalServerError, $"Internal Server Error!!!", ex.Message + "");
            }
            finally
            {
                _logger.LogInformation($"Finishing invocation of method name : {callerName}, file path : {callerFilePath}, line number : {callerLineNumer}, DateTime : {DateTime.UtcNow.ToLongDateString()} {DateTime.UtcNow.ToLongTimeString()}");
            }
        }
        [DebuggerHidden]
        [DebuggerStepThrough]
        public int GetLoggedInUserId()
        {
            Int32 userId = -1;
            var userName = this.GetLoggedInUserName();
            if (string.IsNullOrEmpty(userName) || (Global.Configuration?.GetSection("IsResolveUserId").Value + "").ToLower() != "true" || Global.Configuration?.GetSection("UserIdResolutionQuery").Value + "" == "") return 0;
            try
            {
                using (var db = new DataBaseManager(_context))
                {
                    List<DbParameters> list = new() { new DbParameters() { Name = "@UserName", Type = SqlDBType.String, Value = userName.ToUpper() } };
                    using (IDataReader dr = db.ExecDataReader(Global.Configuration?.GetSection("UserIdResolutionQuery").Value + "", list)) { userId = dr != null && dr.Read() ? dr.GetInt32(0) : -1; }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Internal Server Error!!!", "Unable to fetch user details...");
                throw new CustomException((int)HttpStatusCode.InternalServerError, $"Internal Server Error!!!", $"Unable to fetch user details...{Environment.NewLine}Error:{ex.Message + ""}");
            }
            return userId > 0 ? userId : 0;
        }
        [DebuggerHidden]
        [DebuggerStepThrough]
        public string GetLoggedInUserName()
        {
            return "" + HttpContext?.User?.Identity?.Name;
        }
        [DebuggerHidden]
        [DebuggerStepThrough]
        public List<RequestSpecification> GetRequestSpecification(dynamic request, ExecutionType executionType, OutPutType outPutType = OutPutType.JSON, bool hasFileContent = false, FileContentType fileContentType = FileContentType.Excel, string fileContentFieldName = "", string sheetName = "")
        {
            try
            {
                if (hasFileContent && executionType != ExecutionType.DataSetProcedure && executionType != ExecutionType.DataTableProcedure) throw new CustomException((int)HttpStatusCode.InternalServerError, "Internal Server Error!!!", "Request having file content should have execution type either DataTableProcedure or DataSetProcedure.");
                if (executionType == ExecutionType.DataSetProcedure && outPutType == OutPutType.CSV) throw new CustomException((int)HttpStatusCode.InternalServerError, "Internal Server Error!!!", "Request having execution type DataSetProcedure supports output type as Excel or JSON.");
                var result = new List<RequestSpecification>();
                foreach (var (p, r) in from KeyValuePair<string, JToken> p in request as JObject
                                       let r = new RequestSpecification
                                       {
                                           IsArray = p.Value.HasValues,
                                           PropertyName = hasFileContent && (p.Key + "").ToLower() == (fileContentFieldName + "").ToLower() ? fileContentFieldName + "" : p.Key,
                                           CallType = "P",
                                           PropertyType = null,
                                           IsDataTable = hasFileContent ? (p.Key + "").ToLower() != (fileContentFieldName + "").ToLower() && p.Value.HasValues && null != p.Value && null != p.Value.First && p.Value.First.HasValues : p.Value.HasValues && null != p.Value && null != p.Value.First && p.Value.First.HasValues,
                                           IsFileContent = hasFileContent && (p.Key + "").ToLower() == (fileContentFieldName + "").ToLower(),
                                           FileContentType = fileContentType
                                       }
                                       select (p, r))
                {
                    r.PropertyType = hasFileContent && (r.PropertyName + "").ToLower() == (fileContentFieldName + "").ToLower() ? null : GetPropertyType(p, r);
                    try { r.PropertyValue = hasFileContent && (r.PropertyName + "").ToLower() == (fileContentFieldName + "").ToLower() ? fileContentType == FileContentType.CSV ? ExcelExportHelper.GetXMLFromCSVByteArray(p.Value.ToObject<byte[]>()) : fileContentType == FileContentType.Excel ? ExcelExportHelper.GetXMLFromExcelByteArray(p.Value.ToObject<byte[]>(), sheetName) : p.Value.ToObject<byte[]>() : r.IsArray && !r.IsDataTable ? p.Value.ToObject<List<dynamic>>() : r.IsArray && r.IsDataTable ? p.Value : p.Value.ToObject(r.PropertyType); }
                    catch { r.PropertyType = null; r.PropertyValue = null; }
                    result.Add(r);
                }
                var q = this.GetRequestSpecificationFromQueryParameters();
                if (result.Count > 0) result.AddRange(q);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Invalid request!!!");
                throw new CustomException((int)HttpStatusCode.InternalServerError, $"Internal Server Error!!!", $"Invalid request...{Environment.NewLine}Error:{ex.Message + ""}");
            }
        }
        [DebuggerHidden]
        [DebuggerStepThrough]
        public List<RequestSpecification> GetRequestSpecificationFromQueryParameters() => (from c in HttpContext.Request.Query select new RequestSpecification() { PropertyName = c.Key, PropertyType = null, PropertyValue = c.Value, CallType = "G" }).ToList();
        [DebuggerHidden]
        [DebuggerStepThrough]
        public WhoAmI GetWhoAmI()
        {
            return new WhoAmI() { UserId = this.GetLoggedInUserId(), UserName = this.GetLoggedInUserName() };
        }
        [DebuggerHidden]
        [DebuggerStepThrough]
        public IBase MapOutPutType(OutPutType outPutType, ExecutionType executionType)
        {
            OutPutType = (executionType == ExecutionType.DataSetProcedure || executionType == ExecutionType.DataSetText || executionType == ExecutionType.DataTableProcedure || executionType == ExecutionType.DataTableText) && OutPutType == OutPutType.Excel ? OutPutType.Excel : (executionType == ExecutionType.DataTableProcedure || executionType == ExecutionType.DataTableText) && outPutType == OutPutType.CSV ? OutPutType.CSV : OutPutType.JSON;
            return this;
        }
        [DebuggerHidden]
        [DebuggerStepThrough]
        public static Type GetPropertyType(KeyValuePair<string, JToken> property, RequestSpecification r) => r.PropertyType = !r.IsDataTable ? (r.IsArray && !r.IsDataTable ? property.Value.First?.Type : property.Value.Type) switch { JTokenType.Raw or JTokenType.String or JTokenType.TimeSpan or JTokenType.Guid or JTokenType.Uri => typeof(String), JTokenType.Boolean => typeof(Boolean), JTokenType.Date => typeof(DateTime), JTokenType.Integer => typeof(Int64), JTokenType.Float => typeof(Decimal), _ => r.CallType == "P" ? null : typeof(Object) } : typeof(DataTable);
    }
}
