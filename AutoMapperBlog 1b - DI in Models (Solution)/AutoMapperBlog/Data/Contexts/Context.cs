using AutoMapperBlog.Data.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoMapperBlog.Data.Contexts
{
    public class Context : DbContext
    {
        public Context()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(CreateInMemoryDatabase());
            base.OnConfiguring(optionsBuilder);
        }

        public DbSet<DepartmentEntity> Departments { get; set; }
        public DbSet<EmployeeEntity> Employees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DepartmentEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("Department");
                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<EmployeeEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("Employee");
                entity.HasOne(e => e.Department).WithMany(d => d.Employees).HasForeignKey(e => e.DepartmentId);
                entity.Property(e => e.FirstName).IsRequired();
                entity.Property(e => e.LastName).IsRequired();
            });

            // Add some test data

            modelBuilder.Entity<DepartmentEntity>().HasData(new DepartmentEntity { Id = 1, Name = "Sales" });
            modelBuilder.Entity<DepartmentEntity>().HasData(new DepartmentEntity { Id = 2, Name = "Marketing" });
            modelBuilder.Entity<DepartmentEntity>().HasData(new DepartmentEntity { Id = 3, Name = "IT" });

            modelBuilder.Entity<EmployeeEntity>().HasData(new EmployeeEntity { Id = 1, DepartmentId = 1, FirstName = "John", LastName = "Smith", Salary = 35000 });
            modelBuilder.Entity<EmployeeEntity>().HasData(new EmployeeEntity { Id = 2, DepartmentId = 1, FirstName = "Mary", LastName = "Jones", Salary = 42000 });
            modelBuilder.Entity<EmployeeEntity>().HasData(new EmployeeEntity { Id = 3, DepartmentId = 1, FirstName = "Mel", LastName = "Black", Salary = 25000 });
            modelBuilder.Entity<EmployeeEntity>().HasData(new EmployeeEntity { Id = 4, DepartmentId = 2, FirstName = "Jack", LastName = "White", Salary = 55000 });
            modelBuilder.Entity<EmployeeEntity>().HasData(new EmployeeEntity { Id = 5, DepartmentId = 2, FirstName = "Jess", LastName = "Tan", Salary = 47000 });
            modelBuilder.Entity<EmployeeEntity>().HasData(new EmployeeEntity { Id = 6, DepartmentId = 2, FirstName = "Steve", LastName = "Davies", Salary = 22000 });
            modelBuilder.Entity<EmployeeEntity>().HasData(new EmployeeEntity { Id = 7, DepartmentId = 3, FirstName = "Emma", LastName = "Williams", Salary = 47500 });
            modelBuilder.Entity<EmployeeEntity>().HasData(new EmployeeEntity { Id = 8, DepartmentId = 3, FirstName = "Darren", LastName = "Miller", Salary = 28000 });
        }

        private static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();

            return connection;
        }
    }
}
