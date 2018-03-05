namespace ConfigurationsManager
{
    using System.Data.Entity;

    public partial class DataModel : DbContext
    {
        public DataModel(
            string server,
            string database)
            : base($"data source={server};initial catalog={database};persist security info=True;integrated security=true;")
        {

        }

        public DataModel(
            string server,
            string database,
            string username,
            string password)
            : base($"data source={server};initial catalog={database};persist security info=True;user id={username};password={password};")
        {
            
        }

        public virtual DbSet<Configuration> Configurations { get; set; }
        public virtual DbSet<FeatureFlag> FeatureFlags { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}