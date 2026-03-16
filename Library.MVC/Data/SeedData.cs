using Bogus;
using Library.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Library.MVC.Data
{
    public class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Admin role + user
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            const string adminEmail = "admin@library.com";
            const string adminPassword = "Admin@1234";
            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new IdentityUser { UserName = adminEmail, Email = adminEmail };
                await userManager.CreateAsync(admin, adminPassword);
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            // Only seed if no data exists
            if (await context.Products.AnyAsync()) return;

            // Seed 20 Books
            var categories = new[] { "Fiction", "Non-Fiction", "Science", "History", "Biography", "Fantasy", "Mystery","Shojo" };

            var bookFaker = new Faker<Product>()
                .RuleFor(p => p.Title, f => f.Lorem.Sentence(3).TrimEnd('.'))
                .RuleFor(p => p.Author, f => f.Name.FullName())
                .RuleFor(p => p.Isbn, f => f.Commerce.Ean13())
                .RuleFor(p => p.Category, f => f.PickRandom(categories))
                .RuleFor(p => p.IsAvailable, _ => true);

            var books = bookFaker.Generate(20);
            await context.Products.AddRangeAsync(books);
            await context.SaveChangesAsync();

            // Seed 10 Members
            var memberFaker = new Faker<Invoice>()
                .RuleFor(m => m.FullName, f => f.Name.FullName())
                .RuleFor(m => m.Email, (f, m) => f.Internet.Email(m.FullName))
                .RuleFor(m => m.Phone, f => f.Phone.PhoneNumber("###-###-####"));

            var members = memberFaker.Generate(10);
            await context.Invoices.AddRangeAsync(members);
            await context.SaveChangesAsync();

            // Seed 15 Loans
            var random = new Random();
            var today = DateTime.Today;
            var loansCreated = 0;
            var bookIndex = 0;

            // 5 returned loans
            for (int i = 0; i < 5; i++)
            {
                var loanDate = today.AddDays(-random.Next(30, 60));
                var loan = new InvoiceLine
                {
                    ProductId = books[bookIndex].Id,
                    InvoiceId = members[random.Next(members.Count)].Id,
                    LoanDate = loanDate,
                    DueDate = loanDate.AddDays(14),
                    ReturnedDate = loanDate.AddDays(random.Next(1, 13)) // returned before due
                };
                // book stays available since returned
                context.InvoiceLines.Add(loan);
                bookIndex++;
                loansCreated++;
            }

            // 5 overdue loans
            for (int i = 0; i < 5; i++)
            {
                var loanDate = today.AddDays(-random.Next(20, 40));
                var loan = new InvoiceLine
                {
                    ProductId = books[bookIndex].Id,
                    InvoiceId = members[random.Next(members.Count)].Id,
                    LoanDate = loanDate,
                    DueDate = loanDate.AddDays(7),
                    ReturnedDate = null
                };
                books[bookIndex].IsAvailable = false;
                context.InvoiceLines.Add(loan);
                bookIndex++;
                loansCreated++;
            }

            // 5 active loans
            for (int i = 0; i < 5; i++)
            {
                var loanDate = today.AddDays(-random.Next(1, 5));
                var loan = new InvoiceLine
                {
                    ProductId = books[bookIndex].Id,
                    InvoiceId = members[random.Next(members.Count)].Id,
                    LoanDate = loanDate,
                    DueDate = today.AddDays(random.Next(7, 14)),
                    ReturnedDate = null
                };
                books[bookIndex].IsAvailable = false;
                context.InvoiceLines.Add(loan);
                bookIndex++;
                loansCreated++;
            }

            await context.SaveChangesAsync();
        }
    }
}
