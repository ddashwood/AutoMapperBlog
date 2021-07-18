using AutoMapper;
using AutoMapperBlog.Data.Contexts;
using AutoMapperBlog.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoMapperBlog.Models
{
    class Employee
    {
        public readonly Context _context;
        public readonly IMapper _mapper;

        public Employee(Context context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public decimal Salary { get; set; }

        public int DepartmentId { get; set; }

        public void GivePayRise(decimal amount)
        {
            Salary += amount;
            var entity = _mapper.Map<EmployeeEntity>(this);
            _context.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public void ChangeName(string newFirstName, string newLastName)
        {
            FirstName = newFirstName;
            LastName = newLastName;
        }
    }
}
