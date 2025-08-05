using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolAppMobile.Services
{
    public class AuthSuccessResponse
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
    }
}
