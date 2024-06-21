using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IdentityApp.Data;
using IdentityApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using IdentityApp.Authorization;

namespace IdentityApp.Pages.Invoices
{
    public class EditModel : DI_BasePageModel
    {
       

        public EditModel(ApplicationDbContext context,
            IAuthorizationService authorizationService,
            UserManager<IdentityUser> userManager)
            : base(context, authorizationService, userManager)
        {
            
        }

        [BindProperty]
        public Invoice Invoice { get; set; } = default!;

        //gets called when we load/visit the page of edit
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Invoice =  await Context.Invoice.FirstOrDefaultAsync(m => m.InvoiceId == id);
            if (Invoice == null)
            {
                return NotFound();
            }

            //adding authorizzation stuff
            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                User, Invoice, InvoiceOperations.Update);

            if (isAuthorized.Succeeded == false)
                return Forbid();



            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.

        //gets called when we edit something in the edit page.
        public async Task<IActionResult> OnPostAsync(int id)
        {
            //as the creatorId is set in backend code and not in the frontend code i.e. why we will never recieve a valid model state

            //grabbing the invoice from the database
            var invoice= await Context.Invoice.AsNoTracking()
                .SingleOrDefaultAsync(m=> m.InvoiceId == id);

            if(invoice == null)
                return NotFound();

            //setting creatorId right after grabbing the invoice
            //and this very imp. coz then we will not get authorized as the creatorId will always be null.
            Invoice.CreatorId = invoice.CreatorId;

            //adding authorizzation stuff
            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                User, Invoice, InvoiceOperations.Update);

            if (isAuthorized.Succeeded == false)
                return Forbid();

            //for keeping the status same as submitted, approved or rejected.
            Invoice.Status = invoice.Status;

            if (invoice == null)
                return NotFound();

            Context.Attach(Invoice).State = EntityState.Modified;

            try
            {
                await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InvoiceExists(Invoice.InvoiceId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool InvoiceExists(int id)
        {
            return Context.Invoice.Any(e => e.InvoiceId == id);
        }
    }
}
