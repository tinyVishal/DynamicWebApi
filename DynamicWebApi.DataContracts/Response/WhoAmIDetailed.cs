/* Copyright Chetan N Mandhania */
using System.Collections.Generic;

namespace DynamicWebApi.DataContracts.Response
{
    public class WhoAmIDetailed : WhoAmI
    {
        public List<string> UserGroups { get; set; }
    }
}
