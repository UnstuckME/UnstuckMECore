//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;

namespace UnstuckMEServerGUI
{
    public partial class Message
    {
        public int MessageID { get; set; }
        public int ChatID { get; set; }
        public string MessageData { get; set; }
        public string FilePath { get; set; }
        public bool IsFile { get; set; }
        public int SentBy { get; set; }
        public DateTime SentTime { get; set; }
    
        public virtual Chat Chat { get; set; }
        public virtual UserProfile UserProfile { get; set; }
    }
}
