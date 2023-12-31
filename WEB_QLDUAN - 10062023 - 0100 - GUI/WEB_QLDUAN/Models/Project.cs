//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WEB_QLDUAN.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Project
    {
        public Project()
        {
            this.Tasks = new HashSet<Task>();
            this.UserProjects = new HashSet<UserProject>();
        }
    
        public string ID { get; set; }
        public string ProjectName { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public string Description { get; set; }
        public Nullable<int> Status { get; set; }
        public Nullable<int> Quantity { get; set; }
        public string Owner { get; set; }
    
        public virtual ICollection<Task> Tasks { get; set; }
        public virtual ICollection<UserProject> UserProjects { get; set; }
        public virtual User User { get; set; }
    }
}
