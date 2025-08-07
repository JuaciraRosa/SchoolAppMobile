using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolAppMobile.Models
{
    public class AlertDto
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsResolved { get; set; }
        public string? AdminResponse { get; set; }

       

    }

}
