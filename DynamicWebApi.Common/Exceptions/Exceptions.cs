/* Copyright Chetan N Mandhania */
using System;
using System.Diagnostics;

namespace DynamicWebApi.Common.Exceptions
{
    [DebuggerStepThrough]
    [Serializable]
    public class CustomException : Exception
    {
        [DebuggerHidden]
        public int Code { get; set; }
        [DebuggerHidden]
        public string Description { get; set; }
        [DebuggerHidden]
        public new string Message { get; set; }
        [DebuggerHidden]
        [DebuggerStepThrough]
        public CustomException(int code, string message, string description)
        {
            this.Code = code;
            this.Description = description;
            this.Message = message;
        }
    }
}
