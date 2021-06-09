/* Copyright Chetan N Mandhania */
using DynamicWebApi.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace DynamicWebApi.WebApi
{
    [DebuggerStepThrough]
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        [DebuggerHidden]
        public ExceptionMiddleware(RequestDelegate next) => _next = next;
        [DebuggerHidden]
        [DebuggerStepThrough]
        public async Task Invoke(HttpContext context)
        {
            try { await _next.Invoke(context); }
            catch (Exception ex) { await HandleExceptionAsync(context, ex); }
        }
        [DebuggerHidden]
        [DebuggerStepThrough]
        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            int statusCode;
            string description, message;
            if (ex is CustomException exception)
            {
                message = exception.Message;
                description = exception.Description;
                statusCode = exception.Code;
            }
            else
            {
                message = ex.Message;
                description = ex.StackTrace ?? "Unexpected error";
                statusCode = (int)HttpStatusCode.InternalServerError;
            }
            var response = context.Response;
            response.ContentType = "application/json";
            response.StatusCode = statusCode;
            await response.WriteAsync(JsonConvert.SerializeObject(new CustomException(statusCode, message,description)));
        }
    }
}
