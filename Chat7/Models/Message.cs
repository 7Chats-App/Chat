//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Chat7.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Message
    {
        public int Id { get; set; }
        public int MainID { get; set; }
        public int SecondID { get; set; }
        public string Message1 { get; set; }
        public bool isMain { get; set; }
        public System.DateTime DateCreated { get; set; }
    }
}
