using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolAppMobile.Models
{
    public class MarkDto
    {
        public string Subject { get; set; }
        public double Value { get; set; }
        public bool IsPassed { get; set; }
        public string Date { get; set; }
    }
}
