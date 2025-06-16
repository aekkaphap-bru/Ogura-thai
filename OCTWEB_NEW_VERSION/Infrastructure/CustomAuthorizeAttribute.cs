using OCTWEB_NET45.Context;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace OCTWEB_NET45.Infrastructure
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly int[] allowedPermission;
        public CustomAuthorizeAttribute(params int[] permission)
        {
            this.allowedPermission = permission;
        }
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            bool authorize = false;
            int userId =  Convert.ToInt32(httpContext.Session["USE_Id"]); //ex. 1152 
            //Session['UserId'] ex."28048f37-39af-4e40-9a89-297deeabbc42"

            if (userId > 0)
                using (var context = new OCTWEBTESTEntities())
                {
                    var userRights = (from ur in context.UserRights
                                      where ur.USE_Id == userId
                                      select ur.RIH_Id).ToList();
                    var userRights_list = userRights;

                    foreach (var role in allowedPermission)
                    {
                        if (userRights_list.Contains(role))
                        {
                            return true;
                        }
                    }
                }

            return authorize;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
           
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated || filterContext.HttpContext.Session["USE_Id"] == null)  
            {
                base.HandleUnauthorizedRequest(filterContext);
            }
            else
            {
                filterContext.Result = new RedirectToRouteResult(
                  new RouteValueDictionary  
                   {  
                        { "controller", "Home" },  
                        { "action", "UnAuthorized" }  
                   });
            }   
        }
    }  
}