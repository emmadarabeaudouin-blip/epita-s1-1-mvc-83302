using System;
using System.Collections.Generic;
using System.Text;

namespace Library.Domain
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; }= "";
        public string Phone { get; set; } = "";

        public List<Invoice> Loans { get; set; } = new();
    }
}
