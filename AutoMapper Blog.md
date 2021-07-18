# Three Things I Wish I Knew About AutoMapper Before Starting

Last year, I was in a position to create a completely new, greenfield project. This is not the kind of opportunity that arises too often, so I accepted the job offer, and set about understanding the business and technical requirements of the project.

Before long, I was in a position to decide on the stack I wanted to use. I chose ASP.Net MVC 3.1, the long-term support version of ASP.Net, as the main technology, alongside SQL Server and Entity Framework. One decision that I made was to use AutoMapper - a tool which I have read about, and used on small personal projects, but not used on an enterprise project before.

Now that a good amount of development has been completed, I've had a chance to reflect on my choices, and one area of reflection has been the use of AutoMapper. The conclusion that I've come to is that, although AutoMapper is a great tool and it was right to use it, there are some pitfalls which I wish I had been aware of before I started. This blog is designed to highlight some of the issues that I've faced, and the solutions that I've found.

The blog is accompanied by a number of folders containing an example project in various stages of development. Throughout this blog, I have pasted only the relevant parts of the code - the full code can be found in these folders.

## The Example Project

The folder "AutoMapperBlog 0 - Setup" contains a starting point for the project. In it, we've created a simple database representing that classic problem, departments containing employees:

```c#
    public class DepartmentEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<EmployeeEntity> Employees { get; set; }
    }

    public class EmployeeEntity
    {
        public int Id { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public decimal Salary { get; set; }

        public int DepartmentId { get; set; }
        public DepartmentEntity Department { get; set; }
    }

    public class Context : DbContext
    {
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
        }
    }
```

We've also created some models, which closely map the entities that are saved to the database, but add some extra business logic:

```c#
    class Department
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public List<Employee> Employees { get; private set; }

        public decimal Budget
        {
            get
            {
                return Employees.Count() > 2 ? Employees.Count() * 80000 : Employees.Count() * 100000;
            }
        }
    }

    class Employee
    {
        public int Id { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public decimal Salary { get; private set; }

        public int DepartmentId { get; private set; }

        public void GivePayRise(decimal amount)
        {
            Salary += amount;
        }
    }
```

And finally, an AutoMapper mapping from the database entities to the models:

```c#
    class Mappings : Profile
    {
        public Mappings()
        {
            CreateMap<DepartmentEntity, Department>()
                .ReverseMap();

            CreateMap<EmployeeEntity, Employee>()
                .ReverseMap();
        }
    }
```

The rest of the code in the setup project is simply the glue that holds all of this together - creating the dependency injection service provider, creating the database (we use a Sqlite In Memory database), and so on.

## Problem 1 - Dependency Injection in Models

Models often represent more than just data. A model which represents just data is known as an [anemic model](https://en.wikipedia.org/wiki/Anemic_domain_model), and this is typically recognised as being an anti-pattern. Models normally carry out business logic such as validation or calculation, as well as simply storing data.

Sometimes, that business logic will need to make use of services which are registered with dependency injection. To show how this presents a problem with AutoMapper, and how to solve the problem, we will add to the `Employee.GivePayRise()` method, and make it save the change to the database. In order to do this, the Employee object needs access to a Mapper and to a Context:

```c#
        public void GivePayRise(decimal amount)
        {
            Salary += amount;
            var entity = _mapper.Map<EmployeeEntity>(this);
            _context.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
            _context.SaveChanges();
        }
```

So how do we get the Employee object to be aware of these services?

I went through several iterations of trial and error before I found the solution to this problem. In the folder "AutoMapperBlog 1a - DI in Models", I've shown my first attempt at solving this problem. I use constructor injection:

```c#
    class Employee
    {
        public readonly Context _context;
        public readonly IMapper _mapper;

        public Employee(Context context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
      
        ...
    }
```

This prevents AutoMapper from creating the object, but it does not prevent me from using AutoMapper. It just means that I have to create the object with its dependencies first, and then get AutoMapper to fill in the data:

```c#
    class Problem1
    {
        public void Demonstrate()
        {
            var entity = _context.Employees.AsNoTracking().Single(e => e.Id == 2);
            var employee = new Employee(_context, _mapper);
            _mapper.Map(entity, employee);

            Assert.Equal(42000, employee.Salary);

            employee.GivePayRise(5000);

            _context.ChangeTracker.Clear();

            Assert.Equal(47000, _context.Employees.Single(e => e.Id == 2).Salary);
        }
    }
```

The overload of the Map() method which takes two parameters is the key to making this work. We first create the model using its constructor, and passing in the dependencies it needs. Then, we pass that object to Map() (by providing it as the second argument), and AutoMapper does the rest for us.

You can see from the Assert statements that this has the desired effect. But it's far from an elegant solution. It means that the code which retrieves the employee from the database and maps it to the model also needs to know what dependencies it needs, which reminds me a lot of the way we used to write code before we had dependency injection available.

What's more, it becomes even less manageable if we try to load a department which contains employees. When creating nested lists of objects, AutoMapper doesn't have a nice overload that lets us supply pre-created instances of the nested class. To make everything work in this situation, I had to use a different, but equally inelegant, solution - ensuring that the Employee class had a parameterless constructor in addition to the one with parameters (private is fine, AutoMapper can use private constructors), and providing an alternative method of injecting dependencies such as public properties. Then, when loading a department, I would have to iterate over the employees in the department and inject the dependencies into each employee.

At this point, I realised that I would need to find a different solution, and I began looking into creating my own value resolver, amongst other options, before I eventually stumbled upon what I believe to be the correct solution.

### The Solution

As is often the case, once I found the solution it was extremely elegant. There are three steps:

1) Register the Employee class with the dependency injection system. We use `AddTransient` to ensure that every time we ask for an Employee, we get a brand new instance of the class:

```c#
        public static void ConfigureServices(this IServiceCollection services)
        {
            services.AddDbContext<Context>();
            services.AddAutoMapper(typeof(Program).Assembly);

            services.AddTransient<Employee>();
        }
```

2) Tell AutoMapper to use the dependency injection system to create Employee objects. AutoMapper provides the `ConstructUsingServiceLocator()` method for this purpose:

```c#
        public Mappings()
        {
            CreateMap<DepartmentEntity, Department>()
                .ReverseMap();

            CreateMap<EmployeeEntity, Employee>()
                .ConstructUsingServiceLocator()
                .ReverseMap();
        }
```

3) Remove the line which creates an Employee object prior to calling AutoMapper, and instead use AutoMapper in the traditional way to do the mapping:

```c#
        public void Demonstrate()
        {
            var entity = _context.Employees.AsNoTracking().Single(e => e.Id == 2);
            var employee = _mapper.Map<Employee>(entity);
        }
```

And that's it! Simple, elegant, it works in all scenarios including when the Employee is nested inside other classes such as Departments... if only I'd known about it before I started my project.

You can find the code including the solution to this problem in the folder "AutoMapperBlog 1b - DI in Models (Solution)".

## Problem 2 - Saving Changed, Nested Data

Entity Framework has an excellent system of change-tracking.

By using AutoMapper, however, we're bypassing the built-in change-tracking. We load an entity, then map it onto a model. We change the model - and Entity Framework has no way of knowing that the data which it gave to us has been changed.

For the most part, this isn't a great problem. We can map our model back to an entity, and then attach that entity to Entity Framework. We change it's state to "modified", and now Entity Framework will save it for us.

This technique does not save any nested navigation properties - for example, it would not save an Employee's Department data, nor would it save a Department's Employees. But a different technique _does_ save nested data, and that is to use the `Update()` method.

However, `Update()` does not remove data, so we need to handle removal separately. We can demonstrate this with the following code. First of all, we add a `Save()` method to the Department class:

```c#
        public void Save()
        {
            var entity = _mapper.Map<DepartmentEntity>(this);
            _context.Update(entity);
            _context.SaveChanges();
        }
```

Then we update a department's employees and save the data:

```c#
        public void Demonstrate()
        {
            var entity = _context.Departments.AsNoTracking().Include(d => d.Employees).Single(d => d.Id == 1);
            var department = _mapper.Map<Department>(entity);

            // One employee has changed their name
            department.Employees[1].ChangeName("Mary", "Baker");

            // One employee has joined the department
            department.Employees.Add(new Employee(_context, _mapper) { FirstName = "Mia", LastName = "Lawson", Salary = 35500 });

            // One employee has left the department
            department.Employees.RemoveAt(2);

            // Now save all the changes
            _context.ChangeTracker.Clear();
            department.Save();

            // Check that all our changes have been saved correctly
            _context.ChangeTracker.Clear();

            Assert.Equal("Baker", _context.Employees.Single(e => e.Id == 2).LastName);
            Assert.True(_context.Employees.Any(e => e.LastName == "Lawson"));
            Assert.Equal(3, _context.Employees.Where(e => e.DepartmentId == 1).Count()); // FAILS!
        }
```

The last line of this demonstrations fails. The department started with 3 employees (which we set up in the seed data). We changed one employee's data, added one employee, and removed one employee, which ought to leave 3 employees still in the department, but the `Assert.Equal` fails with an error message saying the actual value is 4. That's because the `Update()` method that we used to attach the entity to Entity Framework doesn't remove data, it only adds and updates data.

It's not clear what the correct behaviour should be here. Should we remove the employee from the database? Leave the employee in the database but with their department set to null? Or should we do something else - perhaps the current behaviour is correct? For the sake of argument, let's assume that for our system, we want to actually remove the employee from the database.

This can be easily fixed with a little extra logic. (This may not be the most optimised way of achieving the desired result, but it works well enough for a blog post. You might be tempted, in real life, to keep track of which employees have been deleted, in which case this whole problem goes away, but depending on the amount of data you've got, you may decide that keeping track of deleted employees is overkill for your project.) If you wanted something else to happen, such as setting the employee's department to null, this can be easily adapted:

```c#
        public void Save()
        {
            var entity = _mapper.Map<DepartmentEntity>(this);
            _context.Update(entity);

            var departmentEmployeeIds = Employees.Select(e => e.Id);
            var deletedEmployees = _context.Employees
                    .Where(e => e.DepartmentId == Id && !departmentEmployeeIds.Contains(e.Id));
            _context.RemoveRange(deletedEmployees);

            _context.SaveChanges();
        }
```

Bul although this works, we have just set ourselves a fairly major trap. The above code only works because of a single method call which is buried away and easy to miss. And, what's more, if you do miss it, you're going to end up deleting data you don't want to delete. The method call in question? It's on the first line of the `Demonstrate()` method. Specifically, it's the call to `.Include(d => d.Employees)`.

Let's see what happens if we miss out this bit, by adding the following to our demonstration. You can find all this code in the folder "AutoMapperBlog 2a - Deleting Nested Data"

```c#
        public void Demonstrate()
        {
            ...
            
            
            // Here is where it goes wrong
            _context.ChangeTracker.Clear();
            entity = _context.Departments.AsNoTracking().Single(d => d.Id == 1);
            department = _mapper.Map<Department>(entity);
            department.Name = "Domestic Sales";
            department.Save();
            _context.ChangeTracker.Clear();

            Assert.Equal("Domestic Sales", _context.Departments.Single(d => d.Id == 1).Name);
            Assert.Equal(3, _context.Employees.Where(e => e.DepartmentId == 1).Count()); // FAILS!
        }
```

It's not obvious what's gone wrong here - or at least, it wasn't obvious to me. So let me take you through it.

First of all, we load the department from the database. Since we didn't use the `.Include()` method this time, we have not loaded the employees (and we were careful to clear the change tracker before the test, to simulate the fact that perhaps this was the very first database call after the program started) - so `entity.Employees` has the value "null".

Then, we map the entity onto its model. AutoMapper handles the null collection in its source data by creating an empty collection in the destination. Yes, that's right - `department.Employees` is _not_ equal to "null", instead it is set to `new List<Employee>()`.

This presents a massive problem for us when we come to save the data. There is no way that we can distinguish between a department with no employees (a department that's being closed, for example), and a department with lots of employees but where we just chose not to load them from the database because they weren't required for the action we were carrying out. In both cases, we will see an empty list of employees.

### The Solution

The solution, in this case, is a change to a single line of code.

We simply need to configure AutoMapper to allow null collections. This is as opposed to the default behaviour, which is to create empty collections in the destination where the source is null.

This can be done in an individual profile, but to me the default behaviour seems sufficiently dangerous that I decided to change it globally. This can be done when we add AutoMapper to the service collection, in Startup.cs:

```c#
        public static void ConfigureServices(this IServiceCollection services)
        {
            services.AddAutoMapper(cfg => cfg.AllowNullCollections = true, typeof(Program).Assembly);
        }
```

Now, if we load the department without including employees, the `Employees` property will be set to "null", and we can easily check for that in the `Save()` method and take the appropriate action:

```c#
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
```

Making these simple changes fixes the final Assert which was broken before, without having any negative side effects on the rest of our code, and the full code is in the folder "AutoMapperBlog 2b - Deleting Nested Data (Solution)". Be aware, though, that this is a global change, so if you're applying this to an existing project you should check that it's not going to have any unforeseen side-effects on your project.

## Problem 3 - Changing the Model

For our final problem, I want to investigate what happens when an aspect of the model changes.

The Employees in our example scenario have a "FirstName" property and a "LastName" property. But I want to imagine that, in the early days of the project, we become aware of a company standard that "Surname" is preferred over "LastName".

Changing a database column in the early stages of a project is not normally a problem. In the folder "AutoMapperBlog 3a - Changing the Model", I clicked on the Surname property of the EmployeeEntity class. I selected Edit/Refactor/Rename from the menu (or pressed F2), and renamed the property to Surname. If I had a real database, I would then create and run a database migration, but for the in-memory database being used for this demonstration that's not necessary.

The folder "AutoMapperBlog 3a - Changing the Model" contains the results of this change - and I haven't even had to create a new test to show the problem, because this simple change has broken the existing code. When I run the demonstration now, it results in an exception in the demonstration of the first part of this blog, which was working previously. The error is `SqliteException: SQLite Error 19: 'NOT NULL constraint failed: Employee.Surname'`.

It should be obvious what the problem is here. We updated the name of the property in our database entity, but not in the model itself. This means that when we map from the model to the entity, AutoMapper is not able to find a matching property, and so the Surname never gets set.

It's possible to configure AutoMapper so that the LastName property of the model maps to the Surname property of the entity, and vice versa. But I found this to be a band-aid rather than a solution. In an enterprise project, where you've perhaps got multiple view-models and DTOs which are mapped to the model, as well as mapping the model to the entities, the overhead of ensuring that every mapping configuration always gets updated can become large and the process can become error-prone. The debugging process can also be complex, with multiple mapping which might be at fault (as well as other possible faults which are not related to AutoMapper), all of which need to be checked individually.

### The Solution

I have not found a solution to this problem which I'm happy with. The best way I've found of addressing this problem in code is shown in the folder "AutoMapperBlog 3b - Changing the Model (Solution)". Here, I've create a base class which contains the properties that are need for each model. For example:

```c#
    public class EmployeeBase
    {
        public int Id { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public decimal Salary { get; set; }

        public int DepartmentId { get; set; }
    }
```

Then I've made both the entity class and the model inherit from this base class:

```c#
    public class EmployeeEntity : EmployeeBase
    {
        public DepartmentEntity Department { get; set; }
    }
```

```c#
    class Employee : EmployeeBase
    {
        public readonly Context _context;
        public readonly IMapper _mapper;

        public Employee(Context context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }


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
```

I've done similar for the departments, too.

This approach does have several limitations, though:

- It's not possible to attach attributes to the properties for a specific layer. This can be overcome with only a little effort. Most (maybe even all?) attributes which are used by Entity Framework can be substituted for a fluent syntax in the DbContext's `OnModelCreating()` method. MVC supports the `MetadataType` attribute, which enables attributes relating to the view-model to be applied from a different class. But the `MetadataType` solution immediately re-introduces the problem we've just solved, by having properties in different classes whose names must match or else something breaks, so this is far from ideal.
- Even more problematic, is that sub-classes must inherit all of the base classes' properties. This means we can't create a DTO with only a sub-set of properties, and renders this solution all but unusable for a Web API project.
- Aside from the practical considerations, this just does not seem like the right way of solving the problem. It's not what inheritance is designed for, and therefore seems like the wrong tool for the job.

So, as a result, the solution which I've used is to change my processes, rather than my code. Changing properties in this way should be frowned upon, and avoided where possible. Where a layout does need to be changed, it's imperitive that the change is accompanied immediately by a full audit of any models, view-models and DTOs which are mapped from the entity, and the same change must be applied to all layers of code immediately. Keeping code from each layer in a single place (a single folder) helps with this process, but it is still going to be somewhat error-prone.

## Conclusion

Using any new tool for the first time is always going to be a voyage of discover. For me, AutoMapper has been a valuable tool, and one which I would not hesitate to use again - but only after appreciating some of the pitfalls, and introducing techniques which avoid those pitfalls from the very start.
