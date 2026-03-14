using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Library.Domain;
using Library.MVC.Data;
using Library.MVC.ViewModels;

namespace Library.MVC.Controllers
{
    public class InvoicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InvoicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Invoices
        public async Task<IActionResult> Index()
        {
            var loans = await _context.InvoiceLines //new Loans
                .Include(l => l.Invoice) //new Member
                .Include(l => l.Product) //new Book
                .OrderByDescending(l => l.LoanDate)
                .ToListAsync();

            return View(loans);
        }

        // GET: Invoices/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var loan = await _context.InvoiceLines
                .Include(l => l.Invoice) 
                .Include(l => l.Product)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loan == null)
            {
                return NotFound();
            }

            return View(loan);
        }

        // GET: Invoices/Create
        public async Task<IActionResult> Create()
        {
            await NewMethod();

            return View();
        }

        private async Task NewMethod()
        {
            ViewData["ProductId"] = new SelectList(
                            await _context.Products
                                .Where(p => p.IsAvailable)
                                .OrderBy(p => p.Title)
                                .ToListAsync(),
                            "Id", "Title");

            ViewData["InvoiceId"] = new SelectList(
                await _context.Invoices
                    .OrderBy(i => i.FullName)
                    .ToListAsync(),
                "Id", "FullName");
        }





        // POST: Invoices/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("InvoiceId,ProductId,DueDate")] InvoiceLine loan)
        {
            // Remove navigation properties from validation
            ModelState.Remove("Invoice");
            ModelState.Remove("Product");

            // Check book is still available
            var book = await _context.Products.FindAsync(loan.ProductId);
            if (book == null || !book.IsAvailable)
            {
                ModelState.AddModelError("ProductId", "This book is not available for loan.");
            }

            // Check no active loan already exists for this book
            var activeLoan = await _context.InvoiceLines
                .AnyAsync(l => l.ProductId == loan.ProductId && l.ReturnedDate == null);
            if (activeLoan)
            {
                ModelState.AddModelError("ProductId", "This book already has an active loan.");
            }

            if (ModelState.IsValid)
            {
                loan.LoanDate = DateTime.Today;
                _context.InvoiceLines.Add(loan);

                book!.IsAvailable = false;
                _context.Products.Update(book);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["ProductId"] = new SelectList(
                await _context.Products.Where(p => p.IsAvailable).ToListAsync(),
                "Id", "Title", loan.ProductId);

            ViewData["InvoiceId"] = new SelectList(
                await _context.Invoices.ToListAsync(),
                "Id", "FullName", loan.InvoiceId);

            return View(loan);
        }


        // POST: Invoices/MarkReturned/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkReturned(int id)
        {
            var loan = await _context.InvoiceLines
                .Include(l => l.Product)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loan == null) return NotFound();

            // set returned date
            loan.ReturnedDate = DateTime.Today;

            // mark book as available 
            loan.Product!.IsAvailable = true;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        // GET: Invoices/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var loan = await _context.InvoiceLines
                .Include(l => l.Invoice)
                .Include(l => l.Product)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loan == null) return NotFound();

            return View(loan);
        }

        // POST: Invoices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var loan = await _context.InvoiceLines
                .Include(l => l.Product)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loan != null)
            {
                // Free up the book if loan was still active
                if (loan.ReturnedDate == null && loan.Product != null)
                    loan.Product.IsAvailable = true;

                _context.InvoiceLines.Remove(loan);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool InvoiceExists(int id)
        {
            return _context.InvoiceLines.Any(e => e.Id == id);
        }

    }
}
