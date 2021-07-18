using AutoMapper;
using AutoMapperBlog.Data.Contexts;
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
    class Problem1
    {
        private readonly Context _context;
        private readonly IMapper _mapper;

        public Problem1(Context context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public void Demonstrate()
        {
            var entity = _context.Employees.AsNoTracking().Single(e => e.Id == 2);
            var employee = _mapper.Map<Employee>(entity);

            Assert.Equal(42000, employee.Salary);

            employee.GivePayRise(5000);

            _context.ChangeTracker.Clear();

            Assert.Equal(47000, _context.Employees.Single(e => e.Id == 2).Salary);
        }
    }
}
