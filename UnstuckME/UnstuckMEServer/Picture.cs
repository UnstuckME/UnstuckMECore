//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace UnstuckMEServer
{
    using System;
    using System.Collections.Generic;
    
    public partial class Picture
    {
        public int UserID { get; set; }
        public byte[] Photo { get; set; }
    
        public virtual UserProfile UserProfile { get; set; }
    }
}
