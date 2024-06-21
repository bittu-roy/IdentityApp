using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IdentityApp.Authorization;

namespace IdentityApp.Data
{
    public class SeedData
    {

        public static async Task Initialize(
            IServiceProvider serviceProvider, 
            string password)
        {

            using(var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                //create an account (manager)
                var managerUid = await EnsureUser(serviceProvider, "manager@demo.com", password);

                await EnsureRole(serviceProvider, managerUid, Constants.InvoiceManagerRole);

                //creating an account (admininstator)
                var adminUid = await EnsureUser(serviceProvider, "admin@demo.com", password);

                await EnsureRole(serviceProvider, adminUid, Constants.InvoiceAdminRole);

            }
        }


        //create a new task which will ensure that we have a user with a name
        //creatng a new user in the database only by code.

        //what this method will be doing is it will create a new account
        private static async Task<string> EnsureUser(IServiceProvider serviceProvider, string userName, string initPw)
        {
            //grabbing the userManager from the serviceProvider because in the program.cs file we can only make use of the services from the service provider itself. 
            var userManager = serviceProvider.GetService<UserManager<IdentityUser>>();

            //creating a new account but it should not exists from the previous startup
            var user = await userManager.FindByNameAsync(userName);

            //if we don't a manager account
            if(user == null)
            {
                user = new IdentityUser
                {
                    UserName = userName,
                    Email = userName,
                    EmailConfirmed = true,
                };

                var result= await userManager.CreateAsync(user, initPw);
            }

            //if user didnot got created then
            if (user == null)
                throw new Exception("User didnot get created. Password policy problem?");

            //if user got created we can simply return
            return user.Id;
        }

        // Now we have a specific user and now in this case we make a method to ensure a specific role
        private static async Task<IdentityResult> EnsureRole(
            IServiceProvider serviceProvider, string uid, string role)
        {

            //grabbing the roleManager from the serviceProvider because in the program.cs file we can only make use of the services from the service provider itself
            var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();

            IdentityResult ir;

            //checking if the particular role is present on the databse or not and if we don't then we create that particular role.
            if(await roleManager.RoleExistsAsync(role)== false)
            {
                ir = await roleManager.CreateAsync(new IdentityRole(role));
            }

            //grabbing the user
            var userManager = serviceProvider.GetService<UserManager<IdentityUser>>();

            //grabbing that particular user
            var user = await userManager.FindByIdAsync(uid);

            if (user == null)
                throw new Exception("User not existing");

            //adding a specific user to a specific role
            ir = await userManager.AddToRoleAsync(user, role);

            return ir;

        }
    }
}
