using AutoMapper;
using AutoMapperBlog.BaseProperties;
using AutoMapperBlog.Data.Contexts;
using AutoMapperBlog.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoMapperBlog.Models
{
    class Department : DepartmentBase
    {
        public readonly Context _context;
        public readonly IMapper _mapper;

        public Department(Context context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public List<Employee> Employees { get; set; }

        public decimal Budget
        {
            get
            {
                return Employees.Count() > 2 ? Employees.Count() * 80000 : Employees.Count() * 100000;
            }
        }

        public void Save()
        {
            var entity = _mapper.Map<DepartmentEntity>(this);
            _context.Update(entity);

            // Do we have an employee list? If so, check for any deleted employees:
            if (Employees != null)
            {
                var departmentEmployeeIds = Employees.Select(e => e.Id);
                var deletedEmployees = _context.Employees
                        .Where(e => e.DepartmentId == Id && !departmentEmployeeIds.Contains(e.Id));
                _context.RemoveRange(deletedEmployees);
            }

            _context.SaveChanges();
        }
    }
}
