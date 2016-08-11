namespace DAL
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("CurrencyReport")]
    public partial class CurrencyReport
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Key]
        [StringLength(100)]
        public string CurrencyName { get; set; }

        public decimal? ForexBuying { get; set; }

        public decimal? ForexSelling { get; set; }

        public decimal? BanknoteBuying { get; set; }

        public decimal? BanknoteSelling { get; set; }

        public decimal? CrossRateUSD { get; set; }

        public DateTime? CreatedDate { get; set; }
    }
}
