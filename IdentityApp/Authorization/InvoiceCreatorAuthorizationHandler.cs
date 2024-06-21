//This class basically checks if I am the creator of the current Invoice or not

using Microsoft.AspNetCore.Identity;
using IdentityApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace IdentityApp.Authorization
{
    public class InvoiceCreatorAuthorizationHandler:
        AuthorizationHandler<OperationAuthorizationRequirement, Invoice>
    {
        UserManager<IdentityUser> _userManager;
        
        //checking if the current user is the actual owner of the authenticated model.
        public InvoiceCreatorAuthorizationHandler(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, 
            OperationAuthorizationRequirement requirement, 
            Invoice invoice)
        {
            //Checking if the user exists
            if(context.User == null || invoice == null)
            {
                return Task.CompletedTask;
            }

            //validating the requirement for  a CRUD operations
            if(requirement.Name != Constants.CreateOperationName && 
                requirement.Name != Constants.ReadOperationName &&
                requirement.Name != Constants.UpdateOperationName &&
                requirement.Name != Constants.DeleteOperationName)
            { 
                return Task.CompletedTask; 
            }

            if(invoice.CreatorId== _userManager.GetUserId(context.User))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
                
        }
    }
}
