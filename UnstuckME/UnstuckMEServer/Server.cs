//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace UnstuckMEInterfaces
{
    using System;
    using System.Collections.Generic;
    
    public partial class Server
    {
        public int ServerID { get; set; }
        public string ServerName { get; set; }
        public string ServerIP { get; set; }
        public string ServerDomain { get; set; }
        public string SchoolName { get; set; }
        public string AdminUsername { get; set; }
        public string AdminPassword { get; set; }
        public string EmailCredentials { get; set; }
        public string Salt { get; set; }
    }
}