using Library.Domain;


namespace Library.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void ActiveLoan_BookIsNotAvailable()
        {
            var book = new Product { Id = 1, Title = "Test Book", IsAvailable = false };
            Assert.False(book.IsAvailable);
        }
        [Fact]
        public void ReturnedLoan_BookBecomesAvailable()
        {
            var book = new Product { Id = 1, Title = "Test Book", IsAvailable = false };
            var loan = new InvoiceLine
            {
                Id = 1,
                ProductId = book.Id,
                LoanDate = DateTime.Today.AddDays(-10),
                DueDate = DateTime.Today.AddDays(4),
                ReturnedDate = null
            };

            loan.ReturnedDate = DateTime.Today;
            book.IsAvailable = true;

            Assert.NotNull(loan.ReturnedDate);
            Assert.True(book.IsAvailable);
        }
        [Fact]
        public void BookSearch_ByTitle_ReturnsMatch()
        {
            var books = new List<Product>
            {
                new Product { Id = 1, Title = "The Great Gatsby", Author = "Fitzgerald" },
                new Product { Id = 2, Title = "1984", Author = "Orwell" },
                new Product { Id = 3, Title = "To Kill a Mockingbird", Author = "Lee" }
            };

            var search = "gatsby";
            var results = books.Where(b =>
                b.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                b.Author.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();

            Assert.Single(results);
            Assert.Equal("The Great Gatsby", results[0].Title);
        }
        [Fact]
        public void Loan_IsOverdue_WhenDueDatePassedAndNotReturned()
        {
            var loan = new InvoiceLine
            {
                LoanDate = DateTime.Today.AddDays(-20),
                DueDate = DateTime.Today.AddDays(-5), // past due
                ReturnedDate = null
            };

            Assert.True(loan.IsOverdue);
        }
        [Fact]
        public void Loan_IsNotOverdue_WhenReturned()
        {
            var loan = new InvoiceLine
            {
                LoanDate = DateTime.Today.AddDays(-20),
                DueDate = DateTime.Today.AddDays(-5),
                ReturnedDate = DateTime.Today.AddDays(-3) // returned
            };

            Assert.False(loan.IsOverdue);
        }

    }
}
