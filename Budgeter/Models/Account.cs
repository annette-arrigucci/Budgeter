using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Budgeter.Models
{
    public class Account
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Household")]
        public int HouseholdId { get; set; }
        [Required]
        [StringLength(160, MinimumLength = 3)]
        public string Name { get; set; }
        [Required]
        public string Type { get; set; }
        [Required]
        public Decimal Balance { get; set; }
        [Display(Name = "Reconciled Balance")]
        public Decimal ReconciledBalance { get; set; }

        public Account()
        {
            this.Transactions = new HashSet<Transaction>();
        }

        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}