using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WEB_QLDUAN.Models;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using WEB_QLDUAN.Filters;

namespace WEB_QLDUAN.Controllers
{
    public class ProjectController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}