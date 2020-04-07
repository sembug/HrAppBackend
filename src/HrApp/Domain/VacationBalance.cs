﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HrApp
{
    public class VacationBalance
    {
        public List<Personal> Employees { get; set; }
    }
    public class Personal
    {
        public string Employee { get; set; }
        public double Total { get; set; }
        public double Used { get; set; }
        public double Left { get; set; }
    }
}
