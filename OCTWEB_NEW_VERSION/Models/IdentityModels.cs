using Microsoft.AspNet.Identity.EntityFramework;
using System;

namespace OCTWEB_NET45.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public Nullable<int> USE_Usercode { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("AccountsContext")
        {
        }
        public static ApplicationDbContext Create()
        
        {
            return new ApplicationDbContext();
        }
        
    }
}