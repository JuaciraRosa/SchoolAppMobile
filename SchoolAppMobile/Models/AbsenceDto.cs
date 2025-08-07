using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SchoolAppMobile.Models
{
    public class AbsenceDto
    {
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("justified")]
        public bool Justified { get; set; }

        [JsonPropertyName("subject")]
        public SubjectDto Subject { get; set; }

        public string SubjectName => Subject?.Name; // usado no binding
    }
}
