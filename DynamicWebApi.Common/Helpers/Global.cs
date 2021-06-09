/* Copyright Chetan N Mandhania */
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace DynamicWebApi.Common.Helpers
{
    public static class Global
    {
        [DebuggerHidden]
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public static IConfiguration? Configuration { get; set; }
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    }
}
