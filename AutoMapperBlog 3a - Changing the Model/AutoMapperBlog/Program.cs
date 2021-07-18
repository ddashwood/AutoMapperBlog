using AutoMapperBlog.Data.Contexts;
using AutoMapperBlog.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace AutoMapperBlog
{
    class Program
    {
        static void Main(string[] args)
        {
            // Set up dependency injection
            IServiceCollection services = new ServiceCollection();
            services.ConfigureServices();

            services.AddScoped<Problem1>();
            services.AddScoped<Problem2>();
            services.AddScoped<Problem3>();

            var serviceProvider = services.BuildServiceProvider();

            // Set up test database
            serviceProvider.GetRequiredService<Context>().Database.EnsureCreated();

            // Injecting services into models
            serviceProvider.GetRequiredService<Problem1>().Demonstrate();

            // Updating navigation properties
            serviceProvider.GetRequiredService<Problem2>().Demonstrate();

            // Changing property names
            serviceProvider.GetRequiredService<Problem2>().Demonstrate();
        }
    }
}
