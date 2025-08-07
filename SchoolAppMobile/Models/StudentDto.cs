using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolAppMobile.Models
{
    public class StudentDto
    {
        public int Id { get; set; }
        public string PupilNumber { get; set; }
        public int CourseId { get; set; }
        public string Course { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string ProfilePhoto { get; set; }
    }

}
