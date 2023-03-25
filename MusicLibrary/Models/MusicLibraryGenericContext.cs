using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicLibrary.Models
{
    public class MusicLibraryGenericContext<QueryType> : MusicLibraryContext where QueryType : class
    {
        public DbSet<QueryType> QueryModel { get; set; }

        public MusicLibraryGenericContext()
        {
        }

        public MusicLibraryGenericContext(DbContextOptions<MusicLibraryContext> options) : base(options)
        {
        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    if (!optionsBuilder.IsConfigured)
        //    {
        //        optionsBuilder.UseSqlite(@"Filename=..\..\..\..\MusicLibrary\Database\MusicLibrary.db");
        //        optionsBuilder.LogTo(message => System.Diagnostics.Debug.WriteLine(message));
        //    }
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<QueryType>().HasNoKey();
            base.OnModelCreating(modelBuilder);
        }
    }
}
