/* Copyright Chetan N Mandhania */
using DynamicWebApi.Common.Enums;
using DynamicWebApi.Common.Exceptions;
using DynamicWebApi.Common.Interface;
using DynamicWebApi.DAL;
using DynamicWebApi.IRepository;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

namespace DynamicWebApi.Repository
{
    [DebuggerStepThrough]
    public class DynamicWebApiRepository : BaseRepository, IDynamicWebApiRepository
    {
        private readonly ILogger<DynamicWebApiRepository> _logger;
        [DebuggerHidden]
        [DebuggerStepThrough]
        public DynamicWebApiRepository(ILogger<DynamicWebApiRepository> logger, IDBContext context) : base(logger, context) => _logger = logger;
        [DebuggerHidden]
        [DebuggerStepThrough]
        public dynamic Get(string key, List<RequestSpecification> request, int userId, ExecutionType executionType, OutPutType outPutType) => Execute(key, request, userId, executionType, outPutType);
        [DebuggerHidden]
        [DebuggerStepThrough]
        public dynamic Post(string key, List<RequestSpecification> request, int userId, ExecutionType executionType, OutPutType outPutType) => Execute(key, request, userId, executionType, outPutType);
        [DebuggerHidden]
        [DebuggerStepThrough]
        private dynamic Execute(string key, List<RequestSpecification> request, int userId, ExecutionType executionType, OutPutType outPutType)
        {
            try
            {
                if (string.IsNullOrEmpty(key)) throw new ArgumentNullException($"key cannot be null or empty.");
                var list = GetParameters(request);
                var query = GetQuery(key, userId, executionType, list);
                return ExecuteQuery(query, executionType, list, outPutType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong while trying to serve request...");
                throw new CustomException((int)HttpStatusCode.InternalServerError, "Internal Server Error!!!", $"Something went wrong while trying to serve request...{Environment.NewLine}Error:{ex.Message + ""}");
            }
        }
    }
}
