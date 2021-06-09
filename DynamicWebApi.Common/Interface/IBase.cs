/* Copyright Chetan N Mandhania */
using DynamicWebApi.Common.Enums;
using DynamicWebApi.DataContracts.Response;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace DynamicWebApi.Common.Interface
{
    public interface IBase
    {
        Task<T> ExecuteAsync<T>(Func<T> f, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumer = 0);
        Int32 GetLoggedInUserId();
        string GetLoggedInUserName();
        List<RequestSpecification> GetRequestSpecification(dynamic request, ExecutionType executionType, OutPutType outPutType = OutPutType.JSON, bool hasFileContent = false, FileContentType  fileContentType = FileContentType.Excel, string fileContentFieldName = "", string sheetName = "");
        List<RequestSpecification> GetRequestSpecificationFromQueryParameters();
        IBase MapOutPutType(OutPutType outPutType, ExecutionType executionType);
        OutPutType OutPutType { get; set; }
        WhoAmI GetWhoAmI();
    }
}
