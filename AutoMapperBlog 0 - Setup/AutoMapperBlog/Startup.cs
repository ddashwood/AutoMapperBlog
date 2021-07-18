using AutoMapperBlog.Data.Contexts;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoMapperBlog
{
    static class Startup
    {
        public static void ConfigureServices(this IServiceCollection services)
        {
            services.AddDbContext<Context>();
            services.AddAutoMapper(typeof(Program).Assembly);
        }
    }
}
