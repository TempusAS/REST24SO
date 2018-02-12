using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace in24seven.Models
{
    public class Hour
    {
        public DateTime StartTime { get; set; }
        public decimal TotalHours { get; set; }
        public int ProjectId { get; set; }
        public string Description { get; set; }
        public int ProjectTaskId { get; set; }
        public int  SalaryTypeId { get; set; }
    }
}