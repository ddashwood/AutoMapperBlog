using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoMapperBlog.Data.Entities
{
    public class EmployeeEntity
    {
        public int Id { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public decimal Salary { get; set; }

        public int DepartmentId { get; set; }
        public DepartmentEntity Department { get; set; }
    }
}
