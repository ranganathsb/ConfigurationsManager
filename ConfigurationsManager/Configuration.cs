namespace ConfigurationsManager
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("settings.Configurations")]
    public partial class Configuration
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(255)]
        public string InstanceName { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(255)]
        public string ConfigurationKey { get; set; }

        public string ConfigurationValue { get; set; }
    }
}
