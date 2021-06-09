/* Copyright Chetan N Mandhania */
using DynamicWebApi.Common.Enums;
using DynamicWebApi.DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;

namespace DynamicWebApi.Common.Interface
{
    [DebuggerStepThrough]
    public class RequestSpecification
    {
        [DebuggerHidden]
        public bool IsFileContent { get; set; }
        [DebuggerHidden]
        public string PropertyName { get; set; }
        [DebuggerHidden]
        public string CallType { get; set; }
        [DebuggerHidden]
        public bool IsDataTable { get; set; }
        [DebuggerHidden]
        public FileContentType FileContentType { get; set; }
        [DebuggerHidden]
        public bool IsArray { get; set; }
        [DebuggerHidden]
        public Type PropertyType { get; set; }
        [DebuggerHidden]
        public dynamic PropertyValue { get; set; }
        [DebuggerStepThrough]
        [DebuggerHidden]
        public SqlDBType GetDBType() => IsFileContent ? FileContentType == FileContentType.BLOB ? SqlDBType.Binary : SqlDBType.Xml : this.PropertyType == null ? this.CallType == "P" ? SqlDBType.UnKnown : SqlDBType.String : this.PropertyType.Name switch { nameof(Object) => SqlDBType.Object, nameof(String) => SqlDBType.String, nameof(Boolean) => SqlDBType.Boolean, nameof(DateTime) or nameof(TimeSpan) => SqlDBType.DateTime, nameof(Int64) => SqlDBType.Int64, nameof(Decimal) => SqlDBType.Decimal, nameof(DataTable) => SqlDBType.Structured, _ => SqlDBType.String };
        public dynamic GetValue() => IsFileContent ? PropertyValue : IsArray && !IsDataTable ? PropertyType.Name switch { nameof(Object) => PropertyValue, nameof(String) or nameof(DateTime) or nameof(TimeSpan) => PropertyValue is List<String> ? string.Join(',', (this.PropertyValue as List<string>).Select(o => "'" + o.Replace("'", "''") + "'")) : PropertyValue, nameof(Int64) => PropertyValue is List<String> ? string.Join(',', (this.PropertyValue as List<Int32>).Select(o => o)) : PropertyValue, nameof(Decimal) => PropertyValue is List<String> ? string.Join(',', (this.PropertyValue as List<Decimal>).Select(o => o)) : PropertyValue, _ => PropertyValue, } : IsArray && IsDataTable ? (DataTable)JsonConvert.DeserializeObject(Convert.ToString(PropertyValue), typeof(DataTable)) : CallType == "P" && (null == PropertyValue || string.IsNullOrEmpty(Convert.ToString(PropertyValue))) ? null : CallType == "G" ? Convert.ToString(PropertyValue).Contains(",") ? string.Join(',', (this.PropertyValue.Split(',') as string[]).Select(o => "'" + o.Replace("'", "''") + "'")) : string.IsNullOrEmpty(Convert.ToString(this.PropertyValue)) ? "" : Convert.ToString(PropertyValue) : string.IsNullOrEmpty(Convert.ToString(this.PropertyValue)) ? "" : Convert.ToString(PropertyValue);
    }
}