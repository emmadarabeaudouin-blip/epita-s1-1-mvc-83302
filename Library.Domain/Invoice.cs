using System;
using System.Collections.Generic;
using System.Text;

namespace Library.Domain
{
    public class Invoice
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";

        public List<InvoiceLine> InvoiceLines { get; set; } = new();
    }
}
