using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Budgeter.Models
{
    public class BudgetItem
    {
        public int Id { get; set; }
        [Required]
        public int CategoryId { get; set; }
        [Required]
        public int BudgetId { get; set; }
        [Required]
        [Range(typeof(decimal), "0.00","1000000.00")]
        public Decimal Amount { get; set; }
        [Range(0,365)]
        public int AnnualFrequency { get; set; }
    }
}