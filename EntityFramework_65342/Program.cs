using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EntityFramework_65342
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (var context = new MyDbContext())
            {
                context.Database.EnsureCreated();

                DoWhatIMean(context).Wait();
            }
        }

        public static async Task DoWhatIMean(MyDbContext context)
        {
            var match = new Customer
            {
                Orders = new List<Order>
                {
                    new Order(),
                    new Order(),
                },
                Address = new Address(),
            };

            // See aspnet/EntityFrameworkCore#6534
            await context.AddAsync(match);
            await context.SaveChangesAsync();
            var results = from c in context.Customers
                          where c.Id < 400
                          select new
                          {
                              Name = c.Address.CustomerId,
                              Count = c.Orders.Count(),
                          };

            var realz = await results.ToListAsync();
            Console.WriteLine($"DTO count: {realz.Count}.");

            // See aspnet/EntityFrameworkCore#8208 and aspnet/EntityFrameworkCore#9128
            var lastResults = await context.Customers.Select(c => new
            {
                Name = c.Address.CustomerId,
                MaxOrderId = c.Orders.OrderByDescending(o => o.Id).Select(o => o.Id).First(),
            }).LastAsync();
            Console.WriteLine($"Inner DTO max Id: {lastResults.MaxOrderId}");

            // See aspnet/EntityFrameworkCore#8208 and aspnet/EntityFrameworkCore#9128
            var deepResults = await context.Customers.Select(c => new
            {
                Name = c.Address.CustomerId,
                Orders = c.Orders.Select(i => new
                {
                    i.Id,
                    i.Yes
                }).ToList(),
            }).ToListAsync();

            foreach (var item in deepResults)
            {
                Console.WriteLine($"Inner DTO count: {item.Orders.Count}.");
            }
        }

        public class Customer
        {
            public int Id { get; set; }

            public List<Order> Orders { get; set; }

            public Address Address { get; set; }
        }

        public class Order
        {
            public int Id { get; set; }

            public bool Yes { get; set; } = true;
        }

        public class Address
        {
            public int Id { get; set; }

            public int? CustomerId { get; set; }
        }

        public class MyDbContext : DbContext
        {
            public DbSet<Customer> Customers { get; set; }

            public DbSet<Order> Orders { get; set; }

            public DbSet<Address> Addresses { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlite("Data Source=EntityFramework_6534.db");
            }
        }
    }
}
