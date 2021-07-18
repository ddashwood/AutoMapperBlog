using AutoMapperBlog.BaseProperties;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoMapperBlog.Data.Entities
{
    public class EmployeeEntity : EmployeeBase
    {
        public DepartmentEntity Department { get; set; }
    }
}
