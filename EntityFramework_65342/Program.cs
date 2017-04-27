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
            var results = from c in context.Customers
                          where c.Id < 400
                          select new
                          {
                              Name = c.Address.CustomerId,
                              Count = c.Orders.Count(),
                          };

            var realz = await results.ToListAsync();
            Console.WriteLine(realz.Count);
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
                optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=aspnet-EntityFramework_65342;Trusted_Connection=True;MultipleActiveResultSets=true");
            }
        }
    }
}
