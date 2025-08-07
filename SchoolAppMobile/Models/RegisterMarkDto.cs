using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolAppMobile.Models
{
    public class RegisterMarkDto
    {
        public int StudentId { get; set; }
        public int SubjectId { get; set; }
        public float Value { get; set; }
        public DateTime Date { get; set; }
    }

}
