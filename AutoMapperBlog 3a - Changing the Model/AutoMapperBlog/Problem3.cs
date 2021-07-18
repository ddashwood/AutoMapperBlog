using AutoMapper;
using AutoMapperBlog.Data.Contexts;
using AutoMapperBlog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AutoMapperBlog
{
    class Problem3
    {
        private readonly Context _context;
        private readonly IMapper _mapper;

        public Problem3(Context context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public void Demonstrate()
        {
        }
    }
}
