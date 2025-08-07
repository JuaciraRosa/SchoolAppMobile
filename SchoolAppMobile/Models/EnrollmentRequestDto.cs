using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolAppMobile.Models
{
    public class EnrollmentRequestDto
    {
        public int Id { get; set; }
        public string StudentName { get; set; }
        public string CourseName { get; set; }
        public string? Message { get; set; }
        public string Status { get; set; }
    }

}
