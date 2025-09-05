using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppSchoolMaui.Models
{
    public sealed record PublicSubjectItem(int Id, string Name, string CourseName, int TotalLessons);
}
