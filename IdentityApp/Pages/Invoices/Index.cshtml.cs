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
    //AllowAnonymous- allows everyone to view a certain page even when they are not logged in.
    [AllowAnonymous]
    public class IndexModel : DI_BasePageModel
    {
        

        public IndexModel(ApplicationDbContext context,
            IAuthorizationService authorizationService,
            UserManager<IdentityUser> userManager)
            : base(context, authorizationService, userManager)
        {
            
        }

        public IList<Invoice> Invoice { get;set; }

        public async Task OnGetAsync()
        {
            //getting all the invoices
            var invoices= from i in Context.Invoice select i;
                         
            //checking if the current user is a manager
            var isManager = User.IsInRole(Constants.InvoiceManagerRole);

            //checking if the user is an admin
            var isAdmin = User.IsInRole(Constants.InvoiceAdminRole);

            //checking if I am the invoice owner
            var currentUserId = UserManager.GetUserId(User);

            //if I am not the manager i.e. (normal user / accountant) give me the invoice which I own.
            if(!isManager== false && isAdmin == false)
            {
                invoices = invoices.Where(i => i.CreatorId == currentUserId);
            }

            //if I am the manager or an admin then give me all the invoices.
            Invoice = await invoices.ToListAsync();
        }
    }
}
