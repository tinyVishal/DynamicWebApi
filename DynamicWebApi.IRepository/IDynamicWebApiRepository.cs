/* Copyright Chetan N Mandhania */
using DynamicWebApi.Common.Enums;
using DynamicWebApi.Common.Interface;
using System;
using System.Collections.Generic;

namespace DynamicWebApi.IRepository
{
    public interface IDynamicWebApiRepository
    {
        dynamic Post(string key, List<RequestSpecification> request, Int32 userId, ExecutionType executionType, OutPutType outPutType);
        dynamic Get(string key, List<RequestSpecification> request, Int32 userId, ExecutionType executionType, OutPutType outPutType);
    }
}