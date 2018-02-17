namespace ConfigurationsManager
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("settings.FeatureFlags")]
    public partial class FeatureFlag
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(255)]
        public string InstanceName { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(255)]
        public string FlagName { get; set; }

        public bool FlagValue { get; set; }
    }
}
