using AutoMapper;
using AutoMapperBlog.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoMapperBlog.Models
{
    class Mappings : Profile
    {
        public Mappings()
        {
            CreateMap<DepartmentEntity, Department>()
                .ConstructUsingServiceLocator()
                .ReverseMap();

            CreateMap<EmployeeEntity, Employee>()
                .ConstructUsingServiceLocator()
                .ReverseMap();
        }
    }
}
