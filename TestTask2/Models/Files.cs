﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestTask2.Models
{
    public class MyFiles
    {

        public IEnumerable<HttpPostedFileBase> Files { get; set; }
    }
}