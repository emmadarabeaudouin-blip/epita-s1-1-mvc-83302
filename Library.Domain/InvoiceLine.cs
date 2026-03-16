using System;
using System.Collections.Generic;
using System.Text;

namespace Library.Domain
{
    public class InvoiceLine
    {
        public int Id { get; set; }

        // FK to Invoice
        public int InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }

        // FK to Product
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public DateTime LoanDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnedDate { get; set; }
        public bool IsActive=> ReturnedDate == null;
        public bool IsOverdue => ReturnedDate == null && DueDate < DateTime.UtcNow;
    }
}
