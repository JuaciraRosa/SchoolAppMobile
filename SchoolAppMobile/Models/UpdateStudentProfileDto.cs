using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolAppMobile.Models
{
    public class UpdateStudentProfileDto
    {
        public string UserName { get; set; }
        public IFormFile? ProfilePhoto { get; set; }
    }

}
