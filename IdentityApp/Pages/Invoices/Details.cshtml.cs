using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using IdentityApp.Data;
using IdentityApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using IdentityApp.Authorization;

namespace IdentityApp.Pages.Invoices
{
    public class DetailsModel : DI_BasePageModel
    {
       

        public DetailsModel(ApplicationDbContext context,
            IAuthorizationService authorizationService,
            UserManager<IdentityUser> userManager)
            : base(context, authorizationService, userManager)
        {
            
        }

        public Invoice Invoice { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Invoice = await Context.Invoice.FirstOrDefaultAsync(m => m.InvoiceId == id);
            if (Invoice == null)
            {
                return NotFound();
            }

            //check for am I the creator for the invoice?
            var isCreator = await AuthorizationService.AuthorizeAsync(
                User, Invoice, InvoiceOperations.Read);

            //check if manager then I will also be able to see the invoice
            var isManager = User.IsInRole(Constants.InvoiceManagerRole);

            //if I am none of them then access will be denied
            if (isCreator.Succeeded == false && isManager == false)
                return Forbid();
            
            return Page();
        }

        public async Task<IActionResult> OnPostASync(int id, InvoiceStatus status)
        {
            Invoice = await Context.Invoice.FindAsync(id);

            if(Invoice == null)
                return NotFound();

            //checking if we are authorized to perform an operation.
            //and for that we need to check which invoice operation user is currently trying to perfrom
            var invoiceOperation = status == InvoiceStatus.Approved ?
                InvoiceOperations.Approve : InvoiceOperations.Reject;

            //as we know which invoice operation we are performing we will get the correct handler,
            //and the handler will check if we are authorized for this operation
            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                User, Invoice, invoiceOperation);

            //if authorization failed
            if (isAuthorized.Succeeded == false)
                return Forbid();


            Invoice.Status = status;
            Context.Invoice.Update(Invoice);

            await Context.SaveChangesAsync();

            return RedirectToPage("./Index");

        }
    }
}
