﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.SavePost
{
    public class SavePostRequest
    {
        public int? ContractorPostId { get; set; }
        public int? BuiderPostId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
