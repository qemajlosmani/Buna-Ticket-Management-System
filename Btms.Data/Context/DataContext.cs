using Btms.Data.Entities;
using Microsoft.EntityFrameworkCore;


namespace Btms.Data.Context
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public virtual DbSet<Account> Accounts { get; set; }

    }
}
