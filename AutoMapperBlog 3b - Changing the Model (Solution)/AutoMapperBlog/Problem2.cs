using AutoMapper;
using AutoMapperBlog.Data.Contexts;
using AutoMapperBlog.Data.Entities;
using AutoMapperBlog.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AutoMapperBlog
{
    class Problem2
    {
        private readonly Context _context;
        private readonly IMapper _mapper;

        public Problem2(Context context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public void Demonstrate()
        {
            var entity = _context.Departments.AsNoTracking().Include(d => d.Employees).Single(d => d.Id == 1);
            var department = _mapper.Map<Department>(entity);

            // One employee has changed their name
            department.Employees[1].ChangeName("Mary", "Baker");

            // One employee has joined the department
            department.Employees.Add(new Employee(_context, _mapper) { FirstName = "Mia", Surname = "Lawson", Salary = 35500 });

            // One employee has left the department
            department.Employees.RemoveAt(2);

            // Now save all the changes
            _context.ChangeTracker.Clear();
            department.Save();

            // Check that all our changes have been saved correctly
            _context.ChangeTracker.Clear();

            Assert.Equal("Baker", _context.Employees.Single(e => e.Id == 2).Surname);
            Assert.True(_context.Employees.Any(e => e.Surname == "Lawson"));
            Assert.Equal(3, _context.Employees.Where(e => e.DepartmentId == 1).Count());

            // Now do an update without loading the employees
            _context.ChangeTracker.Clear();
            entity = _context.Departments.AsNoTracking().Single(d => d.Id == 1);
            department = _mapper.Map<Department>(entity);
            department.Name = "Domestic Sales";
            department.Save();
            _context.ChangeTracker.Clear();

            Assert.Equal("Domestic Sales", _context.Departments.Single(d => d.Id == 1).Name);
            Assert.Equal(3, _context.Employees.Where(e => e.DepartmentId == 1).Count());
        }
    }
}
