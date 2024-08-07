using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedDB
{
    public class SharedDbContext : DbContext
    {
        public DbSet<TokenDb> Tokens { get; set; }
        public DbSet<ServerDb> Servers { get; set; }
        //GameServer
        public SharedDbContext()
        {

        }
        //ASP .NET
        public SharedDbContext(DbContextOptions<SharedDbContext> options) : base(options)
        {

        }
        public static string ConnectionString { get; set;} = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SharedDB;Integrated Security=True";
        //GameServer
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if(options.IsConfigured == false)
            {
                options
                    .UseSqlServer(ConnectionString);
            }
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<TokenDb>()
                .HasIndex(t => t.AccountDbId)
                .IsUnique();

            builder.Entity<ServerDb>()
                .HasIndex(s => s.Name)
                .IsUnique();
        }
    } 

    
}
