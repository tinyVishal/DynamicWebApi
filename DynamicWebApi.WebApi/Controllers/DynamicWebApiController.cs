/* Copyright Chetan N Mandhania */
using DynamicWebApi.Common.Enums;
using DynamicWebApi.Common.Helpers;
using DynamicWebApi.Common.Interface;
using DynamicWebApi.DAL;
using DynamicWebApi.DataContracts.Response;
using DynamicWebApi.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;

namespace DynamicWebApi.WebApi.Controllers
{
    public class DynamicWebApiController : Base
    {
        private readonly IDynamicWebApiRepository r;
        private readonly IBase _;
        public DynamicWebApiController(IDynamicWebApiRepository repository, ILogger<DynamicWebApiController> logger, IDBContext context) : base(logger, context)
        {
            r = repository;
            _ = this;
        }
        [HttpPost]
        [Authorize]
        [Route("api/DynamicWebApi/Post/{key}/{executionType}/{outPutType?}/{hasFileContent?}/{fileContentType?}/{fileContentFieldName?}/{sheetName?}")]
        public async Task<IActionResult> Post(string key, [FromBody] dynamic request, ExecutionType executionType, OutPutType outPutType = OutPutType.JSON, bool hasFileContent = false, FileContentType fileContentType = FileContentType.Excel, string fileContentFieldName = "", string sheetName = "") => await _.ExecuteAsync(() => _.MapOutPutType(outPutType, executionType).OutPutType == OutPutType.JSON ? Ok(r.Post(key, _.GetRequestSpecification(request, executionType, outPutType, hasFileContent, fileContentType, fileContentFieldName, sheetName), _.GetLoggedInUserId(), executionType, outPutType)) : File(r.Post(key, _.GetRequestSpecification(request, executionType, outPutType, hasFileContent, fileContentType, fileContentFieldName, sheetName), _.GetLoggedInUserId(), executionType, outPutType), _.OutPutType == OutPutType.Excel ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" : "text/csv", Guid.NewGuid().ToString() + (_.OutPutType == OutPutType.Excel ? ".xlsx" : ".csv")));
        [HttpGet]
        [Authorize]
        [Route("api/DynamicWebApi/Get/{key}/{executionType}/{outPutType?}")]
        public async Task<IActionResult> Get(string key, ExecutionType executionType, OutPutType outPutType = OutPutType.JSON) => await _.ExecuteAsync(() => _.MapOutPutType(outPutType, executionType).OutPutType == OutPutType.JSON ? Ok(r.Get(key, _.GetRequestSpecificationFromQueryParameters(), _.GetLoggedInUserId(), executionType, outPutType)) : File(r.Get(key, _.GetRequestSpecificationFromQueryParameters(), _.GetLoggedInUserId(), executionType, outPutType), _.OutPutType == OutPutType.Excel ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" : "text/csv", Guid.NewGuid().ToString() + (_.OutPutType == OutPutType.Excel ? ".xlsx" : ".csv")));
        [HttpGet]
        [Authorize]
        [Route("api/DynamicWebApi/WhoAmI")]
        public async Task<IActionResult> WhoAmI() => await _.ExecuteAsync(() => Ok(_.GetWhoAmI()));
        [HttpGet]
        [Authorize]
        [Route("api/DynamicWebApi/WhoAmIDetailed")]
#pragma warning disable CA1416 // Validate platform compatibility
#pragma warning disable IDE0063 // Use simple 'using' statement
        public async Task<IActionResult> WhoAmIDetailed() => await _.ExecuteAsync(() => Ok(_.ExecuteAsync(() => { var groups = new List<string>(); using (var wi = HttpContext?.User?.Identity as WindowsIdentity) { if (null != wi && null != wi.Groups) { foreach (var group in wi.Groups) { try { groups.Add(group.Translate(typeof(NTAccount)).ToString()); } catch { } } } return new WhoAmIDetailed() { UserId = GetLoggedInUserId(), UserName = GetLoggedInUserName(), UserGroups = groups }; } })));
#pragma warning restore IDE0063 // Use simple 'using' statement
#pragma warning restore CA1416 // Validate platform compatibility
    }
}
