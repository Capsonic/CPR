namespace ReportsWEBAPI
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class FourthShiftContext : DbContext
    {
        public FourthShiftContext()
            : base("name=FourthShiftConnection")
        {
        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
