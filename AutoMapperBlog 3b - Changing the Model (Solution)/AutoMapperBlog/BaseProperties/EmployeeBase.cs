using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoMapperBlog.BaseProperties
{
    public class EmployeeBase
    {
        public int Id { get; set; }

        public string FirstName { get; set; }
        public string Surname { get; set; }

        public decimal Salary { get; set; }

        public int DepartmentId { get; set; }
    }
}
