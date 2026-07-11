using System;

namespace Finance_Tracker
{
    public class Transaction
    {
        public DateTime Date { get; set; }
        public string Type { get; set; }      // "Income" or "Expense"
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public string Notes { get; set; }
    }
}
