using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolAppMobile.Models
{
    public class AbsenceDto
    {
        public string Subject { get; set; }
        public DateTime Date { get; set; }
        public bool Justified { get; set; }
    }
}
