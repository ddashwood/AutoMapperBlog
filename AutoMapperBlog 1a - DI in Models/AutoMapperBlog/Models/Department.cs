using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoMapperBlog.Models
{
    class Department
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public List<Employee> Employees { get; private set; }

        public decimal Budget
        {
            get
            {
                return Employees.Count() > 2 ? Employees.Count() * 80000 : Employees.Count() * 100000;
            }
        }
    }
}
