﻿using Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.ContractorPost
{
    public class AppliedPostDTO
    {
        public Guid? UserID { get; set; }
        public int BuilderID { get; set; } 
         public string? FirstName { get; set; }
        public string? Avatar { get; set; }
        public string? LastName { get; set; }
        public decimal? WishSalary { get; set; }
        public DateTime? AppliedDate{ get; set; }
        public List<AppliedGroup>? Groups { get; set; }
    }
}
