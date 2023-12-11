using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WEB_QLDUAN.Filters
{
    public class ProtectAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            /*
            //if (filterContext.HttpContext.Request.HttpMethod == "GET")
            //{
            // var isAjaxRequest = filterContext.HttpContext.Request.IsAjaxRequest();

            var user = HttpContext.Current.Session["Login"];
            if (user == null)
            {
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    // filterContext.Controller.TempData["Message"] = "Phiên làm việc đã hết hạn, vui lòng đăng nhập lại!";
                    filterContext.HttpContext.Response.StatusCode = 403;
                    filterContext.Result = new JsonResult { Data = "Logout" };
                }
                else
                {
                    var values = new System.Web.Routing.RouteValueDictionary(new
                    {
                        action = "Login",
                        controller = "Home"
                    });

                    filterContext.Controller.TempData["Message"] = "Vui lòng đăng nhập!";
                    // HttpContext.Current.Session["Message"] = "Vui lòng đăng nhập";
                    filterContext.Result = new RedirectToRouteResult(values);
                }
            }
            base.OnActionExecuting(filterContext);*/
        }
    }
}