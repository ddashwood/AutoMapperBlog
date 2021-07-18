using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoMapperBlog.Models
{
    class Employee
    {
        public int Id { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public decimal Salary { get; private set; }

        public int DepartmentId { get; private set; }

        public void GivePayRise(decimal amount)
        {
            Salary += amount;
        }
    }
}
