using System;
using LiteDB;

namespace SAGPoliceUtilities.Models
{
    public class Fine
    {
        public ObjectId FineID { get; set; } 
        
        public string PlayerId { get; set; }
        
        public DateTime FinedDate { get; set; }
        
        public Decimal FinedAmount { get; set; }
        
        public string Reason { get; set; }

        public bool Active { get; set; }
        
        public int CaseID { get; set; }
    }
}