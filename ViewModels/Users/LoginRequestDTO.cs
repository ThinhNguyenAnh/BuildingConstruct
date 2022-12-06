﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.Users
{
    public class LoginRequestDTO
    {
        public string? UserName { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }

    }
}
