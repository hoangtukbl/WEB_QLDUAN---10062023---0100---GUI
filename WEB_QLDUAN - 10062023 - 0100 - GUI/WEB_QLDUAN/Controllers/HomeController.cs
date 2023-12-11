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
using System.Xml.Linq;

namespace WEB_QLDUAN.Controllers
{
    public class HomeController : Controller
    {
        QLDUANEntities db = new QLDUANEntities();
        public ActionResult Index()
        {
            if (Session["Login"] == null || Session["Login"].ToString().Trim() == "")
                return RedirectToAction("Login", "Home");

            return View();
        }

        public ActionResult Error()
        {
            return PartialView();
        }

        #region Project
        public ActionResult Project()
        {
            if (Session["Login"] != null && Session["Login"].ToString().Trim() != "")
            {
                db = new QLDUANEntities();

                var project = db.Projects.Where(x => x.Status != -1).ToList();
                ViewBag.lstproject = project;

                // GET THONG TIN NGUOI DUNG
                var data = db.Users.Find(Session["Login"].ToString().Trim());
                if (data != null)
                {
                    ViewBag._fullname = data.FullName;
                    ViewBag._sex = data.Sex;

                    if (data.Birthday != null && data.Birthday.HasValue)
                    {
                        ViewBag._birthday = data.Birthday.Value.ToString("dd/MM/yyyy");
                        ViewBag._birthdayinfo = data.Birthday.Value.ToString("MM/dd/yyyy");
                    }
                    else
                    {
                        ViewBag._birthday = "";
                        ViewBag._birthdayinfo = "";
                    }
                    ViewBag._address = (data.Address != null ? data.Address : "");
                }
                return View();
            }
            else
            {
                return View("Login");
            }
        }
        [HttpPost]
        [ValidateInput(true)]
        [ValidateAntiForgeryToken]
        public ActionResult FilterProject(DateTime? TuNgay, DateTime? DenNgay, string ID = "", string Status = "")
        {
            try
            {
                if (Session["Login"] != null && Session["Login"].ToString().Trim() != "" && Session["Role"] != null && Session["Role"].ToString().Trim() != "")
                {
                    if (Session["Role"].ToString().Trim().ToUpper() == "USER")
                    {
                        // Phan quyen User
                        var login = Session["Login"].ToString().Trim();
                        db = new QLDUANEntities();

                        if (ID == "" && TuNgay == null && DenNgay == null && Status == "-1")
                        {
                            // TrHop: Owner
                            var data1 = (from p in db.Projects
                                         where p.Status != -1 && p.Owner == login
                                         select p).ToList();

                            // TrHop: member of project
                            var data2 = (from p in db.Projects
                                         join q in db.UserProjects on p.ID equals q.ProjectID
                                         where p.Status != -1 && q.UserID == login && (p.Owner != login || p.Owner == null)
                                         select p).ToList();

                            var model = data1.Concat(data2);

                            ViewBag.idfilter = ID;
                            ViewBag.tungayfilter = TuNgay;
                            ViewBag.denngayfilter = DenNgay;
                            ViewBag.statusfilter = Status;

                            return PartialView("PartialFilterProject", model);
                        }
                        else
                        {
                            if (TuNgay != null || DenNgay != null)
                            {
                                if (TuNgay != null && DenNgay == null)
                                    DenNgay = TuNgay;
                                else if (TuNgay == null && DenNgay != null)
                                    TuNgay = DenNgay;

                                // TrHop: Owner
                                var data1 = (from p in db.Projects
                                             where p.Status != -1 && p.Owner == login
                                             select p).ToList();

                                // TrHop: member of project
                                var data2 = (from p in db.Projects
                                             join q in db.UserProjects on p.ID equals q.ProjectID
                                             where p.Status != -1 && q.UserID == login && (p.Owner != login || p.Owner == null)
                                             select p).ToList();

                                var model = data1.Concat(data2);

                                if (ID != "")
                                    model = model.Where(p => p.ID == ID).ToList();
                                if (Status != "")
                                {
                                    if (Status == "0") // hoan thanh
                                        model = model.Where(p => p.Status == 0).ToList();
                                    else if (Status == "1") // dang tien hanh
                                        model = model.Where(p => p.Status == 1 && p.EndDate >= DateTime.Now).ToList();
                                    else if (Status == "2") // tre han
                                        model = model.Where(p => p.Status == 1 && p.EndDate < DateTime.Now).ToList();
                                }

                                model = model.Where(p => p.StartDate >= TuNgay && p.StartDate <= DenNgay).ToList();

                                ViewBag.idfilter = ID;
                                ViewBag.tungayfilter = TuNgay;
                                ViewBag.denngayfilter = DenNgay;
                                ViewBag.statusfilter = Status;

                                return PartialView("PartialFilterProject", model);
                            }
                            else
                            {
                                // TrHop: Owner
                                var data1 = (from p in db.Projects
                                             where p.Status != -1 && p.Owner == login
                                             select p).ToList();

                                // TrHop: member of project
                                var data2 = (from p in db.Projects
                                             join q in db.UserProjects on p.ID equals q.ProjectID
                                             where p.Status != -1 && q.UserID == login && (p.Owner != login || p.Owner == null)
                                             select p).ToList();

                                var model = data1.Concat(data2);

                                if (ID != "")
                                    model = model.Where(p => p.ID == ID).ToList();
                                if (Status != "")
                                {
                                    if (Status == "0") // hoan thanh
                                        model = model.Where(p => p.Status == 0).ToList();
                                    else if (Status == "1") // dang tien hanh
                                        model = model.Where(p => p.Status == 1 && p.EndDate >= DateTime.Now).ToList();
                                    else if (Status == "2") // tre han
                                        model = model.Where(p => p.Status == 1 && p.EndDate < DateTime.Now).ToList();
                                }

                                ViewBag.idfilter = ID;
                                ViewBag.tungayfilter = TuNgay;
                                ViewBag.denngayfilter = DenNgay;
                                ViewBag.statusfilter = Status;

                                return PartialView("PartialFilterProject", model);
                            }
                        }
                    }
                    else
                    {
                        // Phan quyen Admin
                        var login = Session["Login"].ToString().Trim();
                        db = new QLDUANEntities();

                        if (ID == "" && TuNgay == null && DenNgay == null && Status == "-1")
                        {
                            var model = db.Projects.Where(x => x.Status != -1).ToList();

                            ViewBag.idfilter = ID;
                            ViewBag.tungayfilter = TuNgay;
                            ViewBag.denngayfilter = DenNgay;
                            ViewBag.statusfilter = Status;

                            return PartialView("PartialFilterProject", model);
                        }
                        else
                        {
                            if (TuNgay != null || DenNgay != null)
                            {
                                if (TuNgay != null && DenNgay == null)
                                    DenNgay = TuNgay;
                                else if (TuNgay == null && DenNgay != null)
                                    TuNgay = DenNgay;

                                var model = db.Projects.Where(x => x.Status != -1).ToList();

                                if (ID != "")
                                    model = model.Where(p => p.ID == ID).ToList();
                                if (Status != "")
                                {
                                    if (Status == "0") // hoan thanh
                                        model = model.Where(p => p.Status == 0).ToList();
                                    else if (Status == "1") // dang tien hanh
                                        model = model.Where(p => p.Status == 1 && p.EndDate >= DateTime.Now).ToList();
                                    else if (Status == "2") // tre han
                                        model = model.Where(p => p.Status == 1 && p.EndDate < DateTime.Now).ToList();
                                }

                                model = model.Where(p => p.StartDate >= TuNgay && p.StartDate <= DenNgay).ToList();

                                ViewBag.idfilter = ID;
                                ViewBag.tungayfilter = TuNgay;
                                ViewBag.denngayfilter = DenNgay;
                                ViewBag.statusfilter = Status;

                                return PartialView("PartialFilterProject", model);
                            }
                            else
                            {
                                var model = db.Projects.Where(x => x.Status != -1).ToList();

                                if (ID != "")
                                    model = model.Where(p => p.ID == ID).ToList();
                                if (Status != "")
                                {
                                    if (Status == "0") // hoan thanh
                                        model = model.Where(p => p.Status == 0).ToList();
                                    else if (Status == "1") // dang tien hanh
                                        model = model.Where(p => p.Status == 1 && p.EndDate >= DateTime.Now).ToList();
                                    else if (Status == "2") // tre han
                                        model = model.Where(p => p.Status == 1 && p.EndDate < DateTime.Now).ToList();
                                }

                                ViewBag.idfilter = ID;
                                ViewBag.tungayfilter = TuNgay;
                                ViewBag.denngayfilter = DenNgay;
                                ViewBag.statusfilter = Status;

                                return PartialView("PartialFilterProject", model);
                            }
                        }
                    }
                }
                else
                {
                    return View("Login");
                }
            }
            catch
            {
                return Json(new
                {
                    STATUS = "false",
                    MESSAGE = "Rất tiếc, đã xảy ra lỗi, vui lòng kiểm tra lại thông tin"
                });
            }
        }
        public ActionResult AddProject()
        {
            if (Session["Login"] != null && Session["Login"].ToString().Trim() != "")
            {
                // GET THONG TIN NGUOI DUNG
                var data = db.Users.Find(Session["Login"].ToString().Trim());
                if (data != null)
                {
                    ViewBag._fullname = data.FullName;
                    ViewBag._sex = data.Sex;

                    if (data.Birthday != null && data.Birthday.HasValue)
                    {
                        ViewBag._birthday = data.Birthday.Value.ToString("dd/MM/yyyy");
                        ViewBag._birthdayinfo = data.Birthday.Value.ToString("MM/dd/yyyy");
                    }
                    else
                    {
                        ViewBag._birthday = "";
                        ViewBag._birthdayinfo = "";
                    }
                    ViewBag._address = (data.Address != null ? data.Address : "");
                }
                return View();
            }
            else
            {
                return View("Login");
            }
        }
        public ActionResult AddTaskProject(String ID)
        {
            if (Session["Login"] != null && Session["Login"].ToString().Trim() != "")
            {
                if (string.IsNullOrEmpty(ID))
                {
                    Session["ProjectID"] = "";
                    return View("Project");
                }
                else
                {
                    Session["ProjectID"] = ID;
                    db = new QLDUANEntities();
                    var model = db.Projects.Find(ID);

                    // GET THONG TIN NGUOI DUNG
                    var data = db.Users.Find(Session["Login"].ToString().Trim());
                    if (data != null)
                    {
                        ViewBag._fullname = data.FullName;
                        ViewBag._sex = data.Sex;

                        if (data.Birthday != null && data.Birthday.HasValue)
                        {
                            ViewBag._birthday = data.Birthday.Value.ToString("dd/MM/yyyy");
                            ViewBag._birthdayinfo = data.Birthday.Value.ToString("MM/dd/yyyy");
                        }
                        else
                        {
                            ViewBag._birthday = "";
                            ViewBag._birthdayinfo = "";
                        }
                        ViewBag._address = (data.Address != null ? data.Address : "");
                    }
                    return View(model);
                }
            }
            else
            {
                return View("Login");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(true)]
        public ActionResult ProjectThemMoi(string ID, string ProjectName, DateTime? StartDate, DateTime? EndDate, string Description, string User, string Status)
        {
            ViewData["DialogProject"] = "true";

            ViewData["DialogProject_CapNhat"] = null;
            ViewData["PROJECT_CAPNHAT"] = null;

            try
            {
                if (Session["Login"] != null && Session["Login"].ToString().Trim() != "")
                {
                    db = new QLDUANEntities();

                    var _project = db.Projects.Where(x => x.Status != -1).ToList();
                    ViewBag.lstproject = _project;

                    if (string.IsNullOrEmpty(ID) || string.IsNullOrEmpty(ProjectName)
                        || !(StartDate != null && StartDate.HasValue) || !(EndDate != null && EndDate.HasValue)
                        || string.IsNullOrEmpty(User) || Status == null || Status.Trim() == "-1")
                    {
                        return Json(new
                        {
                            STATUS = "false",
                            MESSAGE = "Vui lòng nhập đầy đủ dữ liệu!"
                        });
                    }
                    else if (ModelState.IsValid)
                    {
                        var data = db.Projects.Find(ID.Trim());
                        if (data != null)
                        {
                            return Json(new
                            {
                                STATUS = "false",
                                MESSAGE = "Trùng mã Project ID!"
                            });
                        }
                        else
                        {
                            // 1. Insert Project
                            Project project = new Project();

                            project.ID = ID.Trim().ToUpper();
                            project.ProjectName = ProjectName;

                            if (StartDate != null && StartDate.HasValue)
                                project.StartDate = StartDate;
                            else project.StartDate = null;

                            if (EndDate != null && EndDate.HasValue)
                                project.EndDate = EndDate;
                            else project.EndDate = null;

                            project.Owner = Session["Login"].ToString().Trim();

                            project.Description = Description;

                            if (Status != null && (Status.Trim() == "0" || Status.Trim() == "1"))
                                project.Status = Convert.ToInt16(Status.Trim());
                            else project.Status = 1; // Default

                            int count = 0;

                            // 2. Insert UserProject
                            if (!string.IsNullOrEmpty(User))
                            {
                                string[] lstUser = User.Split(',');
                                if (lstUser.Count() > 0)
                                {
                                    count = lstUser.Count();
                                    for (int i = 0; i < lstUser.Count(); i++)
                                    {
                                        if (lstUser[i].ToString().Trim() != "")
                                        {
                                            UserProject us = new UserProject();
                                            us.ProjectID = project.ID;
                                            us.UserID = lstUser[i];
                                            us.JoinDate = DateTime.Now;
                                            db.UserProjects.Add(us);
                                        }
                                    }
                                }
                            }
                            project.Quantity = count;

                            db.Projects.Add(project);
                            db.SaveChanges();

                            _project = db.Projects.Where(p => p.Status != -1).ToList();
                            ViewBag.lstproject = _project;

                            return Json(new
                            {
                                STATUS = "true",
                                MESSAGE = "Thêm dữ liệu thành công!"
                            });
                        }
                    }
                    else
                    {
                        return Json(new
                        {
                            STATUS = "false",
                            MESSAGE = "Rất tiếc đã xảy ra lỗi, vui lòng kiểm tra lại thông tin nhập vào!"
                        });
                    }
                }
                else
                {
                    return Json(new
                    {
                        STATUS = "false",
                        MESSAGE = "Bạn chưa đăng nhập!"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    STATUS = "false",
                    MESSAGE = "Rất tiếc đã xảy ra lỗi, vui lòng kiểm tra lại thông tin nhập vào!"
                });
            }
        }

        [HttpPost]
        public ActionResult LoadProject(string ID)
        {
            try
            {
                if (!string.IsNullOrEmpty(ID))
                {
                    db = new QLDUANEntities();

                    List<string> lstUser = new List<string>();
                    var _user = db.UserProjects.Where(p => p.ProjectID.Trim().ToUpper() == ID.Trim().ToUpper());
                    foreach (var s in _user.ToList())
                    {
                        lstUser.Add(s.UserID.ToString());
                    }

                    var data = (from p in db.Projects
                                where p.ID.Trim().ToUpper() == ID.Trim().ToUpper()
                                select p).FirstOrDefault();

                    if (data != null)
                    {
                        return Json(new
                        {
                            ID = data.ID,
                            ProjectName = data.ProjectName,
                            StartDate = data.StartDate,
                            EndDate = data.EndDate,
                            Quantity = data.Quantity,
                            Description = data.Description,
                            Status = data.Status,
                            User = string.Join(",", lstUser),
                            Owner = data.Owner
                        });
                    }
                    else return Json(new
                    {
                        ID = "",
                        ProjectName = "",
                        StartDate = "",
                        EndDate = "",
                        Quantity = "",
                        Description = "",
                        Status = "",
                        User = "",
                        Owner = ""
                    });
                }
                else return Json(new
                {
                    ID = "",
                    ProjectName = "",
                    StartDate = "",
                    EndDate = "",
                    Quantity = "",
                    Description = "",
                    Status = "",
                    User = "",
                    Owner = ""
                });
            }
            catch
            {
                return Json(new
                {
                    ID = "",
                    ProjectName = "",
                    StartDate = "",
                    EndDate = "",
                    Quantity = "",
                    Description = "",
                    Status = "",
                    User = "",
                    Owner = ""
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(true)]
        public ActionResult ProjectCapNhat(string ID, string ProjectName, DateTime? StartDate, DateTime? EndDate, string Description, string User, String Status)
        {
            ViewData["DialogProject_CapNhat"] = "true";

            ViewData["DialogProject"] = null;
            ViewData["PROJECT_THEMMOI"] = null;

            try
            {
                if (Session["Login"] != null && Session["Login"].ToString().Trim() != "")
                {
                    db = new QLDUANEntities();

                    var _project = db.Projects.Where(x => x.Status != -1).ToList();
                    ViewBag.lstproject = _project;

                    if (string.IsNullOrEmpty(ID))
                    {
                        return Json(new
                        {
                            STATUS = "false",
                            MESSAGE = "Không xác định được đối tượng cần sửa dữ liệu!"
                        });
                    }
                    else if (string.IsNullOrEmpty(ProjectName)
                        || !(StartDate != null && StartDate.HasValue) || !(EndDate != null && EndDate.HasValue)
                        || string.IsNullOrEmpty(User) || Status == null || Status.Trim() == "-1")
                    {
                        return Json(new
                        {
                            STATUS = "false",
                            MESSAGE = "Vui lòng nhập đầy đủ dữ liệu!"
                        });
                    }
                    else if (ModelState.IsValid)
                    {
                        // 1. Update PROJECT
                        var project = db.Projects.Find(ID.Trim());

                        project.ProjectName = ProjectName;

                        if (StartDate != null && StartDate.HasValue)
                            project.StartDate = StartDate;
                        else project.StartDate = null;

                        if (EndDate != null && EndDate.HasValue)
                            project.EndDate = EndDate;
                        else project.EndDate = null;

                        project.Description = Description;
                        project.Owner = Session["Login"].ToString().Trim();

                        if (Status != null && (Status.Trim() == "0" || Status.Trim() == "1"))
                            project.Status = Convert.ToInt16(Status.Trim());
                        else project.Status = 1; // Default

                        int count = 0;

                        // 2. Delete UserProject - OLD
                        var userproject_old = (from p in db.UserProjects
                                               where p.ProjectID.Trim() == ID.Trim()
                                               select p).ToList();

                        if (userproject_old != null && userproject_old.Count > 0)
                        {
                            foreach (var u in userproject_old)
                                db.UserProjects.Remove(u);
                        }
                        db.SaveChanges();

                        // 3. Insert UserProject - NEW
                        if (!string.IsNullOrEmpty(User))
                        {
                            string[] lstUser = User.Split(',');
                            if (lstUser.Count() > 0)
                            {
                                count = lstUser.Count();
                                for (int i = 0; i < lstUser.Count(); i++)
                                {
                                    if (lstUser[i].ToString().Trim() != "")
                                    {
                                        UserProject us = new UserProject();
                                        us.ProjectID = project.ID;
                                        us.UserID = lstUser[i];
                                        us.JoinDate = DateTime.Now;
                                        db.UserProjects.Add(us);
                                    }
                                }
                            }
                        }
                        project.Quantity = count;

                        db.SaveChanges();

                        _project = db.Projects.Where(x => x.Status != -1).ToList();
                        ViewBag.lstproject = _project;

                        return Json(new
                        {
                            STATUS = "true",
                            MESSAGE = "Cập nhật dữ liệu thành công!"
                        });
                    }
                    else
                    {
                        return Json(new
                        {
                            STATUS = "true",
                            MESSAGE = "Rất tiếc đã xảy ra lỗi, vui lòng kiểm tra lại thông tin nhập vào!"
                        });
                    }
                }
                else
                {
                    return Json(new
                    {
                        STATUS = "false",
                        MESSAGE = "Bạn chưa đăng nhập!"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    STATUS = "false",
                    MESSAGE = "Rất tiếc đã xảy ra lỗi, vui lòng kiểm tra lại thông tin nhập vào!"
                });
            }
        }

        [HttpPost]
        public ActionResult DeleteProject(string ID)
        {
            try
            {
                if (!String.IsNullOrEmpty(ID))
                {
                    db = new QLDUANEntities();

                    // 1. UPDATE STATUS = -1; 
                    var project = db.Projects.Find(ID);
                    if (project != null)
                    {
                        project.Status = -1; // DELETE 
                        db.SaveChanges();
                    }

                    return Json("Đã xóa dữ liệu thành công!");
                }
                else return Json("Không xác định được đối tượng cần xóa dữ liệu!");
            }
            catch
            {
                return Json("Rất tiếc, đã xảy ra lỗi. Có thể do ràng buộc toàn vẹn về dữ liệu. Vui lòng kiểm tra và thử lại sau!");
            }
        }
        #endregion

        #region Task
        public ActionResult Task()
        {
            if (Session["Login"] != null && Session["Login"].ToString().Trim() != "")
            {
                db = new QLDUANEntities();
                var login = Session["Login"].ToString().Trim().ToUpper();

                var project = db.Projects.Where(x => x.Status != -1).ToList();
                var task = db.Tasks.Where(x => x.Status != -1).ToList();
                var taskproject = db.Projects.Where(x => x.Status != -1 && x.Owner.Trim().ToUpper() == login).ToList();

                ViewBag.lstproject = project;
                ViewBag.lsttaskproject = taskproject;
                ViewBag.lsttask = task;

                // GET THONG TIN NGUOI DUNG
                var data = db.Users.Find(Session["Login"].ToString().Trim());
                if (data != null)
                {
                    ViewBag._fullname = data.FullName;
                    ViewBag._sex = data.Sex;

                    if (data.Birthday != null && data.Birthday.HasValue)
                    {
                        ViewBag._birthday = data.Birthday.Value.ToString("dd/MM/yyyy");
                        ViewBag._birthdayinfo = data.Birthday.Value.ToString("MM/dd/yyyy");
                    }
                    else
                    {
                        ViewBag._birthday = "";
                        ViewBag._birthdayinfo = "";
                    }
                    ViewBag._address = (data.Address != null ? data.Address : "");
                }
                return View();
            }
            else
            {
                return View("Login");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(true)]
        public ActionResult FilterTask(string PROJECTID, string TASKID, string STATUS)
        {
            try
            {
                if (Session["Login"] != null && Session["Login"].ToString().Trim() != "")
                {
                    if (ModelState.IsValid)
                    {
                        db = new QLDUANEntities();
                        var login = Session["Login"].ToString().Trim();

                        // TrHop: Owner
                        var data1 = (from p in db.Tasks
                                     join q in db.Projects on p.ProjectID equals q.ID
                                     where p.Status != -1 && q.Owner == login
                                     select p).ToList();

                        // TrHop: User nhan Task
                        var data2 = (from p in db.Tasks
                                     join q in db.UserTasks on p.ID equals q.TaskID
                                     where p.Status != -1 && q.UserID == login && (p.Project.Owner != login || p.Project.Owner == null)
                                     select p).ToList();

                        var data = data1.Concat(data2);

                        if (data != null)
                        {
                            if (PROJECTID.Trim() != "")
                            {
                                data = data.Where(x => x.ProjectID.Trim().ToUpper() == PROJECTID.Trim().ToUpper()).ToList();
                            }

                            if (TASKID.Trim() != "")
                            {
                                data = data.Where(x => x.ID.Trim().ToUpper() == TASKID.Trim().ToUpper()).ToList();
                            }

                            if (STATUS.Trim() != "")
                            {
                                if (STATUS == "0") // HOAN THANH
                                {
                                    data = data.Where(x => x.Status == 0).ToList();
                                }
                                else if (STATUS == "1") // DANG TIEN HANH
                                {
                                    data = data.Where(x => x.Status == 1 && x.EndDate >= DateTime.Now).ToList();
                                }
                                else if (STATUS == "2")// TRE HAN
                                {
                                    data = data.Where(x => x.Status == 1 && x.EndDate < DateTime.Now).ToList();
                                }
                            }

                            return PartialView("PartialViewFilterTask", data);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("TaskTK", "Đã có lỗi trong quá trình thống kê, vui lòng thử lại sau!");
                    }
                    return View("Task");
                }
                else
                {
                    return View("Login");
                }
            }
            catch
            {
                ModelState.AddModelError("TaskTK", "Đã có lỗi trong quá trình thống kê, vui lòng thử lại sau!");
                return View("Task");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(true)]
        public ActionResult TaskThemMoi(string ID, string TaskName, DateTime? StartDate, DateTime? EndDate, int? Duration, string Description, string ProjectID, string Status, string User)
        {
            ViewData["DialogTask"] = "true";

            ViewData["DialogTask_CapNhat"] = null;
            ViewData["TASK_CAPNHAT"] = null;

            try
            {
                if (Session["Login"] != null && Session["Login"].ToString().Trim() != "")
                {
                    db = new QLDUANEntities();
                    var login = Session["Login"].ToString().Trim().ToUpper();

                    var project = db.Projects.Where(x => x.Status != -1).ToList();
                    var _task = db.Tasks.Where(x => x.Status != -1).ToList();
                    var taskproject = db.Projects.Where(x => x.Status != -1 && x.Owner.Trim().ToUpper() == login).ToList();

                    ViewBag.lstproject = project;
                    ViewBag.lsttask = _task;
                    ViewBag.lsttaskproject = taskproject;

                    if (string.IsNullOrEmpty(ID) || string.IsNullOrEmpty(TaskName) || string.IsNullOrEmpty(ProjectID)
                        || !(StartDate != null && StartDate.HasValue) || !(EndDate != null && EndDate.HasValue)
                        || string.IsNullOrEmpty(User) || Status == null || Status == "-1")
                    {
                        return Json(new
                        {
                            STATUS = "false",
                            MESSAGE = "Vui lòng nhập đầy đủ dữ liệu!"
                        });
                    }
                    else if (ModelState.IsValid)
                    {
                        var data = db.Tasks.Find(ID.Trim());
                        if (data != null)
                        {
                            return Json(new
                            {
                                STATUS = "false",
                                MESSAGE = "Trùng mã task id!"
                            });
                        }
                        else
                        {
                            // 1. Insert TASK
                            Task task = new Task();

                            task.ID = ProjectID + "_" + ID;
                            task.TaskName = TaskName;

                            if (StartDate != null && StartDate.HasValue)
                                task.StartDate = StartDate;
                            else task.StartDate = null;

                            if (EndDate != null && EndDate.HasValue)
                                task.EndDate = EndDate;
                            else task.EndDate = null;

                            if (Duration != null && Duration.ToString().Trim() != "")
                                task.Duration = Duration;
                            else task.Duration = null;

                            task.Description = Description;

                            if (Status != null && (Status == "0" || Status == "1"))
                                task.Status = Convert.ToInt16(Status);
                            else task.Status = 1; // Dang tien hanh

                            task.ProjectID = ProjectID;

                            // 2. Insert UserTask
                            if (!string.IsNullOrEmpty(User))
                            {
                                string[] lstUser = User.Split(',');
                                if (lstUser.Count() > 0)
                                {
                                    for (int i = 0; i < lstUser.Count(); i++)
                                    {
                                        if (lstUser[i].ToString().Trim() != "")
                                        {
                                            UserTask us = new UserTask();
                                            us.TaskID = task.ID;
                                            us.UserID = lstUser[i];
                                            us.JoinDate = DateTime.Now;
                                            db.UserTasks.Add(us);
                                        }
                                    }
                                }
                            }

                            db.Tasks.Add(task);
                            db.SaveChanges();

                            _task = db.Tasks.Where(x => x.Status != -1).ToList();
                            ViewBag.lsttask = _task;

                            return Json(new
                            {
                                STATUS = "true",
                                MESSAGE = "Thêm dữ liệu thành công!"
                            });
                        }
                    }
                    else
                    {
                        return Json(new
                        {
                            STATUS = "false",
                            MESSAGE = "Rất tiếc đã xảy ra lỗi, vui lòng kiểm tra lại thông tin nhập vào!"
                        });
                    }
                }
                else
                {
                    return Json(new
                    {
                        STATUS = "false",
                        MESSAGE = "Bạn chưa đăng nhập!"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    STATUS = "false",
                    MESSAGE = "Rất tiếc đã xảy ra lỗi, vui lòng kiểm tra lại thông tin nhập vào!"
                });
            }
        }

        [HttpPost]
        public ActionResult LoadTask(string ID)
        {
            try
            {
                if (!string.IsNullOrEmpty(ID))
                {
                    db = new QLDUANEntities();

                    List<string> lstUser = new List<string>();
                    var _user = db.UserTasks.Where(p => p.TaskID.Trim().ToUpper() == ID.Trim().ToUpper());
                    foreach (var s in _user.ToList())
                    {
                        lstUser.Add(s.UserID.ToString());
                    }

                    var data = (from p in db.Tasks
                                where p.ID.Trim().ToUpper() == ID.Trim().ToUpper()
                                select p).FirstOrDefault();

                    if (data != null)
                    {
                        return Json(new
                        {
                            ID = data.ID,
                            TaskName = data.TaskName,
                            StartDate = data.StartDate,
                            EndDate = data.EndDate,
                            Duration = data.Duration,
                            Description = data.Description,
                            ProjectID = data.ProjectID,
                            User = string.Join(",", lstUser),
                            Status = data.Status
                        });
                    }
                    else return Json(new
                    {
                        ID = "",
                        TaskName = "",
                        StartDate = "",
                        EndDate = "",
                        Duration = "",
                        Description = "",
                        ProjectID = "",
                        User = "",
                        Status = ""
                    });
                }
                else return Json(new
                {
                    ID = "",
                    TaskName = "",
                    StartDate = "",
                    EndDate = "",
                    Duration = "",
                    Description = "",
                    ProjectID = "",
                    User = "",
                    Status = ""
                });
            }
            catch
            {
                return Json(new
                {
                    ID = "",
                    TaskName = "",
                    StartDate = "",
                    EndDate = "",
                    Duration = "",
                    Description = "",
                    ProjectID = "",
                    User = "",
                    Status = ""
                });
            }
        }

        [HttpPost]
        public ActionResult LoadAllTask()
        {
            try
            {
                if (Session["Login"] != null && Session["Login"].ToString().Trim() != "")
                {
                    db = new QLDUANEntities();
                    var login = Session["Login"].ToString().Trim().ToUpper();

                    // TrHop: Owner
                    var data1 = (from p in db.Tasks
                                 join q in db.Projects on p.ProjectID equals q.ID
                                 where p.Status != -1 && q.Owner == login
                                 select p).ToList();

                    // TrHop: User nhan Task
                    var data2 = (from p in db.Tasks
                                 join q in db.UserTasks on p.ID equals q.TaskID
                                 where p.Status != -1 && q.UserID == login && (p.Project.Owner != login || p.Project.Owner == null)
                                 select p).ToList();

                    var data = data1.Concat(data2);

                    if (data != null)
                    {
                        var kq = from p in data
                                 select new
                                 {
                                     p.ID,
                                     p.TaskName
                                 };
                        return Json(kq.ToList());
                    }
                    else return Json("");
                }
                else return Json("");
            }
            catch
            {
                return Json("");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(true)]
        public ActionResult TaskCapNhat(string ID, string TaskName, DateTime? StartDate, DateTime? EndDate, int? Duration, string Description, string ProjectID, string Status, string User)
        {
            ViewData["DialogTask_CapNhat"] = "true";

            ViewData["DialogTask"] = null;
            ViewData["TASK_THEMMOI"] = null;

            try
            {
                if (Session["Login"] != null && Session["Login"].ToString().Trim() != "")
                {
                    db = new QLDUANEntities();
                    var login = Session["Login"].ToString().Trim().ToUpper();

                    var project = db.Projects.Where(x => x.Status != -1).ToList();
                    var _task = db.Tasks.Where(x => x.Status != -1).ToList();
                    var taskproject = db.Projects.Where(x => x.Status != -1 && x.Owner.Trim().ToUpper() == login).ToList();

                    ViewBag.lstproject = project;
                    ViewBag.lsttask = _task;
                    ViewBag.lsttaskproject = taskproject;

                    if (string.IsNullOrEmpty(ID))
                    {
                        return Json(new
                        {
                            STATUS = "false",
                            MESSAGE = "Không xác định được đối tượng cần sửa dữ liệu!"
                        });
                    }
                    else if (string.IsNullOrEmpty(TaskName) || string.IsNullOrEmpty(ProjectID)
                        || !(StartDate != null && StartDate.HasValue) || !(EndDate != null && EndDate.HasValue)
                        || string.IsNullOrEmpty(User) || Status == null || Status == "-1")
                    {
                        return Json(new
                        {
                            STATUS = "false",
                            MESSAGE = "Vui lòng nhập đầy đủ dữ liệu!"
                        });
                    }
                    else if (ModelState.IsValid)
                    {
                        // 1. Update TASK
                        var task = db.Tasks.Find(ID.Trim());

                        task.TaskName = TaskName;

                        if (StartDate != null && StartDate.HasValue)
                            task.StartDate = StartDate;
                        else task.StartDate = null;

                        if (EndDate != null && EndDate.HasValue)
                            task.EndDate = EndDate;
                        else task.EndDate = null;

                        if (Duration != null && Duration.ToString().Trim() != "")
                            task.Duration = Duration;
                        else task.Duration = null;

                        task.Description = Description;

                        if (Status != null && (Status == "0" || Status == "1"))
                            task.Status = Convert.ToInt16(Status);
                        else task.Status = 1; // Default; Dang tien hanh

                        task.ProjectID = ProjectID;

                        // 2. Delete UserTask - OLD
                        var usertask_old = (from p in db.UserTasks
                                            where p.TaskID.Trim() == ID.Trim()
                                            select p).ToList();
                        if (usertask_old != null && usertask_old.Count > 0)
                        {
                            foreach (var u in usertask_old)
                                db.UserTasks.Remove(u);
                        }
                        db.SaveChanges();

                        // 3. Insert UserTask - NEW
                        if (!string.IsNullOrEmpty(User))
                        {
                            string[] lstUser = User.Split(',');
                            if (lstUser.Count() > 0)
                            {
                                for (int i = 0; i < lstUser.Count(); i++)
                                {
                                    if (lstUser[i].ToString().Trim() != "")
                                    {
                                        UserTask us = new UserTask();
                                        us.TaskID = task.ID;
                                        us.UserID = lstUser[i];
                                        us.JoinDate = DateTime.Now;
                                        db.UserTasks.Add(us);
                                    }
                                }
                            }
                        }
                        db.SaveChanges();

                        _task = db.Tasks.Where(x => x.Status != -1).ToList();
                        ViewBag.lsttask = _task;

                        return Json(new
                        {
                            STATUS = "true",
                            MESSAGE = "Cập nhật dữ liệu thành công!"
                        });
                    }
                    else
                    {
                        return Json(new
                        {
                            STATUS = "true",
                            MESSAGE = "Rất tiếc đã xảy ra lỗi, vui lòng kiểm tra lại thông tin nhập vào!"
                        });
                    }
                }
                else
                {
                    return Json(new
                    {
                        STATUS = "false",
                        MESSAGE = "Bạn chưa đăng nhập!"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    STATUS = "false",
                    MESSAGE = "Rất tiếc đã xảy ra lỗi, vui lòng kiểm tra lại thông tin nhập vào!"
                });
            }
        }

        [HttpPost]
        public ActionResult DeleteTask(string ID)
        {
            try
            {
                if (!String.IsNullOrEmpty(ID))
                {
                    db = new QLDUANEntities();

                    // 1. UPDATE STATUS = -1; 
                    var task = db.Tasks.Find(ID);
                    if (task != null)
                    {
                        task.Status = -1; // DELETE 
                        db.SaveChanges();
                    }

                    return Json("Đã xóa dữ liệu thành công!");
                }
                else return Json("Không xác định được đối tượng cần xóa dữ liệu!");
            }
            catch
            {
                return Json("Rất tiếc, đã xảy ra lỗi. Có thể do ràng buộc toàn vẹn về dữ liệu. Vui lòng kiểm tra và thử lại sau!");
            }
        }
        #endregion

        #region Note
        public ActionResult Note()
        {
            if (Session["Login"] != null && Session["Login"].ToString().Trim() != "")
            {
                db = new QLDUANEntities();

                var note = db.Notes.ToList();
                ViewBag.lstnote = note;

                // GET THONG TIN NGUOI DUNG
                var data = db.Users.Find(Session["Login"].ToString().Trim());
                ViewBag._fullname = data.FullName;
                ViewBag._sex = data.Sex;

                if (data.Birthday != null && data.Birthday.HasValue)
                {
                    ViewBag._birthday = data.Birthday.Value.ToString("dd/MM/yyyy");
                    ViewBag._birthdayinfo = data.Birthday.Value.ToString("MM/dd/yyyy");
                }
                else
                {
                    ViewBag._birthday = "";
                    ViewBag._birthdayinfo = "";
                }
                ViewBag._address = (data.Address != null ? data.Address : "");

                return View();
            }
            else
            {
                return View("Login");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(true)]
        public ActionResult FilterNote(int? ID, DateTime? TUNGAY, DateTime? DENNGAY)
        {
            try
            {
                if (Session["Login"] != null && Session["Login"].ToString().Trim() != "")
                {
                    var login = Session["Login"].ToString().Trim().ToUpper();

                    if (ModelState.IsValid)
                    {
                        db = new QLDUANEntities();
                        var data = db.Notes.Where(x => x.UserID.Trim().ToUpper() == login).ToList();

                        if (ID != null && ID.ToString().Trim() != "")
                        {
                            data = data.Where(x => x.ID == ID).ToList();
                        }

                        if (TUNGAY != null && TUNGAY.ToString().Trim() != "" && DENNGAY != null && DENNGAY.ToString().Trim() != "")
                        {
                            data = data.Where(x => x.CreatedDate >= TUNGAY && x.CreatedDate <= DENNGAY).ToList();
                        }

                        return PartialView("PartialViewFilterNote", data);
                    }
                    else
                    {
                        ModelState.AddModelError("NoteTK", "Đã có lỗi trong quá trình thống kê, vui lòng thử lại sau!");
                    }
                    return View("Note");
                }
                else
                {
                    return View("Login");
                }
            }
            catch
            {
                ModelState.AddModelError("NoteTK", "Đã có lỗi trong quá trình thống kê, vui lòng thử lại sau!");
                return View("Note");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(true)]
        public ActionResult NoteThemMoi(Note n)
        {
            ViewData["DialogNote"] = "true";

            ViewData["DialogNote_CapNhat"] = null;
            ViewData["NOTE_CAPNHAT"] = null;

            try
            {
                if (Session["Login"] != null && Session["Login"].ToString().Trim() != "")
                {
                    db = new QLDUANEntities();
                    var login = Session["Login"].ToString().Trim().ToUpper();

                    var _note = db.Notes.Where(x => x.UserID.Trim().ToUpper() == login).ToList();
                    ViewBag.lstnote = _note;

                    var _notify = Request.Form["Notify"];

                    if (n == null || string.IsNullOrEmpty(n.Title) || (_notify != null && !(n.NotifyDate != null && n.NotifyDate.HasValue)))
                    {
                        ModelState.AddModelError("NOTE_THEMMOI", "Vui lòng nhập đầy đủ dữ liệu!");
                    }
                    else if (ModelState.IsValid)
                    {
                        // Insert NOTE
                        Note note = new Note();
                        note.Title = n.Title;
                        note.Description = n.Description;
                        note.UserID = Session["Login"].ToString().Trim();
                        note.CreatedDate = DateTime.Now;

                        if (_notify != null)
                        {
                            note.Notify = true;
                            if (n.NotifyDate != null && n.NotifyDate.HasValue)
                                note.NotifyDate = n.NotifyDate;
                            else note.NotifyDate = null;
                        }
                        else
                        {
                            note.Notify = false;
                            note.NotifyDate = null;
                        }

                        db.Notes.Add(note);
                        db.SaveChanges();

                        ViewData["NOTE_THEMMOI"] = "Thêm dữ liệu thành công!";
                        ViewData["DialogNote"] = null;

                        _note = db.Notes.Where(x => x.UserID.Trim().ToUpper() == login).ToList();
                        ViewBag.lstnote = _note;

                    }
                    return View("Note");
                }
                else
                {
                    return View("Login");
                }
            }
            catch
            {
                return RedirectToAction("Note");
            }
        }

        [HttpPost]
        public ActionResult LoadNote(int? ID)
        {
            try
            {
                if (ID != null && ID.ToString().Trim() != "")
                {
                    db = new QLDUANEntities();
                    var data = from p in db.Notes
                               where p.ID == ID
                               select new
                               {
                                   p.ID,
                                   p.Title,
                                   p.Description,
                                   p.CreatedDate,
                                   p.UserID,
                                   p.Notify,
                                   p.NotifyDate
                               };

                    if (data != null)
                    {
                        return Json(data.ToList());
                    }
                    else return Json("");
                }
                else return Json("Không thể load dữ liệu note! Vui lòng thử lại sau.");
            }
            catch
            {
                return Json("");
            }
        }

        [HttpPost]
        public ActionResult LoadAllNote()
        {
            try
            {
                if (Session["Login"] != null && Session["Login"].ToString().Trim() != "")
                {
                    db = new QLDUANEntities();
                    var login = Session["Login"].ToString().Trim().ToUpper();

                    var data = from p in db.Notes
                               where p.UserID.Trim().ToUpper() == login
                               select new
                               {
                                   p.ID,
                                   p.Title,
                                   p.Description,
                                   p.CreatedDate,
                                   p.UserID,
                                   p.Notify,
                                   p.NotifyDate
                               };

                    if (data != null)
                    {
                        return Json(data.ToList());
                    }
                    else return Json("");
                }
                else return Json("");
            }
            catch
            {
                return Json("");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(true)]
        public ActionResult NoteCapNhat(Note n)
        {
            ViewData["DialogNote_CapNhat"] = "true";

            ViewData["DialogNote"] = null;
            ViewData["NOTE_THEMMOI"] = null;

            try
            {
                if (Session["Login"] != null && Session["Login"].ToString().Trim() != "")
                {
                    var login = Session["Login"].ToString().Trim().ToUpper();

                    var _note = db.Notes.Where(x => x.UserID.Trim().ToUpper() == login).ToList();
                    ViewBag.lstnote = _note;

                    var _notify = Request.Form["Notify"];

                    if (n == null || n.ID == null || n.ID.ToString().Trim() == "")
                    {
                        ModelState.AddModelError("NOTE_CAPNHAT", "Không xác định được đối tượng cần sửa dữ liệu!");
                    }
                    else if (string.IsNullOrEmpty(n.Title) || (_notify != null && !(n.NotifyDate != null && n.NotifyDate.HasValue)))
                    {
                        ModelState.AddModelError("NOTE_CAPNHAT", "Vui lòng nhập đầy đủ dữ liệu!");
                    }
                    else if (ModelState.IsValid)
                    {
                        // UPDATE NOTE
                        var note = db.Notes.Find(n.ID);

                        note.Title = n.Title;
                        note.Description = n.Description;
                        note.UserID = Session["Login"].ToString().Trim();
                        note.CreatedDate = DateTime.Now;

                        if (_notify != null)
                        {
                            note.Notify = true;
                            if (n.NotifyDate != null && n.NotifyDate.HasValue)
                                note.NotifyDate = n.NotifyDate;
                            else note.NotifyDate = null;
                        }
                        else
                        {
                            note.Notify = false;
                            note.NotifyDate = null;
                        }

                        db.SaveChanges();

                        ViewData["NOTE_CAPNHAT"] = "Cập nhật dữ liệu thành công!";
                        ViewData["DialogNote"] = null;
                    }
                    return View("Note");
                }
                else
                {
                    return View("Login");
                }
            }
            catch (Exception ex)
            {
                ViewData["NOTE_CAPNHAT"] = "Rất tiếc, đã xảy ra lỗi trong quá trình cập nhật dữ liệu!";
                return RedirectToAction("Note");
            }
        }

        [HttpPost]
        public ActionResult DeleteNote(int? ID)
        {
            try
            {
                if (ID != null && ID.ToString().Trim() != "")
                {
                    db = new QLDUANEntities();
                    var login = Session["Login"].ToString().Trim().ToUpper();

                    var note = db.Notes.Find(ID);
                    if (note != null)
                    {
                        db.Notes.Remove(note);
                        db.SaveChanges();

                        var _note = db.Notes.Where(x => x.UserID.Trim().ToUpper() == login).ToList();
                        ViewBag.lstnote = _note;
                    }

                    return Json("Đã xóa dữ liệu thành công!");
                }
                else return Json("Không xác định được đối tượng cần xóa dữ liệu!");
            }
            catch
            {
                return Json("Rất tiếc, đã xảy ra lỗi. Có thể do ràng buộc toàn vẹn về dữ liệu. Vui lòng kiểm tra và thử lại sau!");
            }
        }
        #endregion

        #region Worklog
        public ActionResult Worklog()
        {
            if (Session["Login"] != null && Session["Login"].ToString().Trim() != "")
            {
                db = new QLDUANEntities();
                var model = db.ToDoes.ToList();

                // GET THONG TIN NGUOI DUNG
                var user = db.Users.Find(Session["Login"].ToString().Trim());
                ViewBag._fullname = user.FullName;
                ViewBag._sex = user.Sex;

                if (user.Birthday != null && user.Birthday.HasValue)
                {
                    ViewBag._birthday = user.Birthday.Value.ToString("dd/MM/yyyy");
                    ViewBag._birthdayinfo = user.Birthday.Value.ToString("MM/dd/yyyy");
                }
                else
                {
                    ViewBag._birthday = "";
                    ViewBag._birthdayinfo = "";
                }
                ViewBag._address = (user.Address != null ? user.Address : "");

                return View(model);
            }
            else
            {
                return View("Login");
            }
        }

        [HttpPost]
        public ActionResult LoadToDoByTask(string TaskID)
        {
            try
            {
                if (String.IsNullOrEmpty(TaskID))
                {
                    return Json(new
                    {
                        STATUS = "false",
                        MESSAGE = "Rất tiếc, đã xảy ra lỗi, không thể load comment người dùng"
                    });
                }
                else
                {
                    db = new QLDUANEntities();

                    var user = db.Users.Find(Session["Login"].ToString().Trim());
                    if (user != null)
                    {
                        var model = (from p in db.ToDoes
                                     where p.TaskID.Trim().ToUpper() == TaskID.Trim().ToUpper()
                                     select p).ToList();

                        return PartialView("PartialToDoByTask", model);
                    }
                    else
                    {
                        return Json(new
                        {
                            STATUS = "false",
                            MESSAGE = "Rất tiếc, đã xảy ra lỗi, không thể worklog người dùng!"
                        });
                    }
                }
            }
            catch
            {
                return Json(new
                {
                    STATUS = "false",
                    MESSAGE = "Rất tiếc, đã xảy ra lỗi, không thể load worklog người dùng!"
                });
            }
        }

        [HttpPost]
        public ActionResult LoadTaskListAjax()
        {
            try
            {
                db = new QLDUANEntities();
                var model = from p in db.Tasks
                            select new
                            {
                                p.ID,
                                p.TaskName
                            };

                return Json(model);
            }
            catch
            {
                return Json("");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(true)]
        public ActionResult WorklogUpdate(ToDo worklog, string FLAG)
        {
            ViewData["DialogWorklog"] = "true";
            ViewData["DialogWorklog_CapNhat"] = null;
            ViewData["WORKLOG_CAPNHAT"] = null;

            try
            {
                if (Session["Login"] != null && Session["Login"].ToString().Trim() != "")
                {
                    db = new QLDUANEntities();
                    var login = Session["Login"].ToString().Trim().ToUpper();

                    if (string.IsNullOrEmpty(FLAG))
                    {
                        return Json(new
                        {
                            STATUS = "false",
                            MESSAGE = "Không xác định được thao tác người dùng! Vui lòng refesh lại trang web."
                        });
                    }
                    else if (string.IsNullOrEmpty(worklog.Title) || !(worklog.Date != null && worklog.Date.HasValue)
                        || !(worklog.HourWork != null && worklog.HourWork.HasValue) || string.IsNullOrEmpty(worklog.TaskID))
                    {
                        return Json(new
                        {
                            STATUS = "false",
                            MESSAGE = "Vui lòng nhập đầy đủ dữ liệu!"
                        });
                    }
                    else
                    {
                        if (FLAG.Trim().ToUpper() == "0")
                        {
                            #region Insert Worklog
                            var data = (from p in db.ToDoes
                                        where p.TaskID == worklog.TaskID
                                             && p.Date.Value == worklog.Date.Value
                                             && p.UserID.Trim().ToUpper() == login
                                        select p).FirstOrDefault();
                            if (data != null)
                            {
                                return Json(new
                                {
                                    STATUS = "false",
                                    MESSAGE = "Bạn đã nhập thông tin Worklog ứng với TaskID và ngày này rồi!"
                                });
                            }
                            else
                            {
                                ToDo log = new ToDo();

                                log.Title = worklog.Title;

                                if (worklog.Date != null && worklog.Date.HasValue)
                                    log.Date = worklog.Date;
                                else log.Date = null;

                                if (worklog.HourWork != null && worklog.HourWork.HasValue)
                                    log.HourWork = worklog.HourWork;
                                else log.HourWork = 0;

                                if (worklog.TaskID != null && worklog.TaskID.Trim() != "")
                                    log.TaskID = worklog.TaskID;
                                else log.TaskID = null;

                                if (worklog.FinishTask != null)
                                    log.FinishTask = worklog.FinishTask;
                                else log.FinishTask = false;

                                log.Description = worklog.Description;

                                if (Session["Login"] != null)
                                    log.UserID = Session["Login"].ToString().Trim();
                                else log.UserID = null;

                                db.ToDoes.Add(log);

                                if (log.TaskID != null && log.TaskID.Trim() != "")
                                {
                                    var task = (from p in db.Tasks
                                                where p.ID.Trim().ToUpper() == log.TaskID.Trim().ToUpper()
                                                select p).FirstOrDefault();
                                    if (log.FinishTask != null)
                                    {
                                        if (log.FinishTask == true)
                                            task.Status = 0;
                                        else task.Status = 1;
                                    }
                                }

                                db.SaveChanges();

                                return Json(new
                                {
                                    STATUS = "true",
                                    MESSAGE = "Thêm dữ liệu thành công!"
                                });
                            }
                            #endregion
                        }
                        else if (FLAG.Trim().ToUpper() == "1")
                        {
                            #region Update Worklog
                            if (worklog.ID != null && worklog.ID != 0)
                            {
                                var log = db.ToDoes.Find(worklog.ID);
                                if (log != null)
                                {
                                    log.Title = worklog.Title;

                                    if (worklog.Date != null && worklog.Date.HasValue)
                                        log.Date = worklog.Date;
                                    else log.Date = null;

                                    if (worklog.HourWork != null && worklog.HourWork.HasValue)
                                        log.HourWork = worklog.HourWork;
                                    else log.HourWork = 0;

                                    if (worklog.TaskID != null && worklog.TaskID.Trim() != "")
                                        log.TaskID = worklog.TaskID;
                                    else log.TaskID = null;

                                    if (worklog.FinishTask != null)
                                        log.FinishTask = worklog.FinishTask;
                                    else log.FinishTask = false;

                                    log.Description = worklog.Description;

                                    if (Session["Login"] != null)
                                        log.UserID = Session["Login"].ToString().Trim();
                                    else log.UserID = null;

                                    if (log.TaskID != null && log.TaskID.Trim() != "")
                                    {
                                        var task = (from p in db.Tasks
                                                    where p.ID.Trim().ToUpper() == log.TaskID.Trim().ToUpper()
                                                    select p).FirstOrDefault();
                                        if (log.FinishTask != null)
                                        {
                                            if (log.FinishTask == true)
                                                task.Status = 0;
                                            else task.Status = 1;
                                        }
                                    }

                                    db.SaveChanges();

                                    return Json(new
                                    {
                                        STATUS = "true",
                                        MESSAGE = "Cập nhật dữ liệu thành công!"
                                    });
                                }
                                else
                                {
                                    return Json(new
                                    {
                                        STATUS = "false",
                                        MESSAGE = "Không xác định được đối tượng cần sửa dữ liệu!"
                                    });
                                }
                            }
                            else
                            {
                                return Json(new
                                {
                                    STATUS = "false",
                                    MESSAGE = "Không xác định được đối tượng cần sửa dữ liệu!"
                                });
                            }
                            #endregion
                        }
                        else
                        {
                            return Json(new
                            {
                                STATUS = "false",
                                MESSAGE = "Không xác định được thao tác người dùng! Vui lòng refesh lại trang web."
                            });
                        }
                    }
                }
                else
                {
                    return Json(new
                    {
                        STATUS = "false",
                        MESSAGE = "Bạn chưa đăng nhập!"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    STATUS = "false",
                    MESSAGE = "Rất tiếc đã xảy ra lỗi, vui lòng kiểm tra lại thông tin nhập vào!"
                });
            }
        }

        [HttpPost]
        public ActionResult LoadWorklog(int? ID)
        {
            try
            {
                if (ID != null && ID.ToString().Trim() != "0")
                {
                    db = new QLDUANEntities();
                    var data = (from p in db.ToDoes
                                where p.ID == ID
                                select new
                                {
                                    p.ID,
                                    p.Title,
                                    p.Date,
                                    p.HourWork,
                                    p.Description,
                                    p.TaskID,
                                    p.FinishTask
                                }).FirstOrDefault();

                    if (data != null)
                    {
                        return Json(data);
                    }
                    else return Json("");
                }
                else return Json("Không thể load dữ liệu Worklog! Vui lòng thử lại sau.");
            }
            catch
            {
                return Json("");
            }
        }

        [HttpPost]
        public ActionResult DeleteWorklog(int? ID)
        {
            try
            {
                if (ID != null && ID != 0)
                {
                    db = new QLDUANEntities();

                    var worklog = db.ToDoes.Find(ID);
                    if (worklog != null)
                    {
                        db.ToDoes.Remove(worklog); // DELETE 
                        db.SaveChanges();
                    }

                    return Json("Đã xóa dữ liệu thành công!");
                }
                else return Json("Không xác định được đối tượng cần xóa dữ liệu!");
            }
            catch
            {
                return Json("Rất tiếc, đã xảy ra lỗi. Có thể do ràng buộc toàn vẹn về dữ liệu. Vui lòng kiểm tra và thử lại sau!");
            }
        }
        #endregion

        #region Gantt Chart Task Project

        [HttpPost]
        public JsonResult LoadUserProjectAjax()
        {
            if (Session["ProjectID"] != null && Session["ProjectID"].ToString().Trim() != "")
            {
                string project_id = Session["ProjectID"].ToString().Trim().ToUpper();
                var db = new QLDUANEntities();

                var data = from t in db.UserProjects.Where(x => x.ProjectID.Trim().ToUpper() == project_id.ToUpper()).AsEnumerable()
                           select new
                           {
                               id = t.ID,
                               text = t.UserID != null ? t.User.FullName : "",
                               unit = "hours/day",
                               default_value = 8,
                               type = "work"
                           };

                return Json(new
                {
                    Data = data.ToList()
                });
            }
            else
            {
                return Json(new
                {
                    Data = ""
                });
            }
        }

        [HttpGet]
        public JsonResult Data()
        {
            db = new QLDUANEntities();

            if (Session["ProjectID"] != null && Session["ProjectID"].ToString().Trim() != "")
            {
                string project_id = Session["ProjectID"].ToString().Trim().ToUpper();
                var a = db.Tasks.Where(x => x.ProjectID.Trim().ToUpper() == project_id && (x.Status == null || x.Status != -1)).Count();

                var jsonData = new
                {
                    // create tasks array
                    data = (
                        from t in db.Tasks.Where(x => x.ProjectID.Trim().ToUpper() == project_id && (x.Status == null || x.Status != -1)).AsEnumerable()
                        select new
                        {
                            id = t.ID,
                            text = t.TaskName,
                            start_date = t.StartDate.Value.ToString("u"),
                            duration = t.Duration,
                            order = t.SortOrder,
                            progress = t.Progress,
                            open = true,
                            parent = t.ParentID
                            // type = (t.Type != null) ? t.Type : String.Empty
                        }
                    ).ToArray(),
                    // create links array
                    links = (
                        from l in db.Links.AsEnumerable()
                        select new
                        {
                            id = l.ID,
                            source = l.SourceTaskID,
                            target = l.TargetTaskID,
                            type = l.Type
                        }
                    ).ToArray()
                };
                return new JsonResult { Data = jsonData, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            else return new JsonResult { Data = "", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        /// <summary>
        /// Update Gantt tasks/links: insert/update/delete 
        /// </summary>
        /// <param name="form">Gantt data</param>
        /// <returns>XML response</returns>
        [HttpPost]
        public ContentResult Save(FormCollection form, string ID)
        {
            var dataActions = GanttRequest.Parse(form, Request.QueryString["gantt_mode"]);
            try
            {
                foreach (var ganttData in dataActions)
                {
                    switch (ganttData.Mode)
                    {
                        case GanttMode.Tasks:
                            UpdateTasks(ganttData);
                            break;
                        case GanttMode.Links:
                            UpdateLinks(ganttData);
                            break;
                    }
                }
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                // return error to client if something went wrong
                dataActions.ForEach(g => { g.Action = GanttAction.Error; });
                return GanttRespose(dataActions);
            }
            return GanttRespose(dataActions);
        }

        /// <summary>
        /// Update gantt tasks
        /// </summary>
        /// <param name="ganttData">GanttData object</param>
        private void UpdateTasks(GanttRequest ganttData)
        {
            if (Session["ProjectID"] != null && Session["ProjectID"].ToString().Trim() != "")
            {
                var task = db.Tasks.ToList().OrderByDescending(x => x.rowID).FirstOrDefault();
                switch (ganttData.Action)
                {
                    case GanttAction.Inserted:
                        if (task == null)
                            ganttData.UpdatedTask.ID = Session["ProjectID"].ToString().Trim() + "_T.1";
                        else ganttData.UpdatedTask.ID = Session["ProjectID"].ToString().Trim() + "_T." + (task.rowID + 1).ToString();

                        ganttData.UpdatedTask.Status = 0;
                        ganttData.UpdatedTask.ProjectID = Session["ProjectID"].ToString().Trim();

                        // add new gantt task entity
                        db.Tasks.Add(ganttData.UpdatedTask);
                        break;
                    case GanttAction.Deleted:
                        // remove gantt tasks
                        db.Tasks.Remove(db.Tasks.Find(ganttData.SourceId));
                        break;
                    case GanttAction.Updated:
                        // update gantt task
                        // db.Entry(db.Tasks.Find(ganttData.UpdatedTask.ID)).CurrentValues.SetValues(ganttData.UpdatedTask);

                        var updateTask = db.Tasks.Find(ganttData.UpdatedTask.ID);
                        if (updateTask != null)
                        {
                            if (ganttData.UpdatedTask.StartDate != null && ganttData.UpdatedTask.StartDate.HasValue)
                                updateTask.StartDate = ganttData.UpdatedTask.StartDate;
                            else updateTask.StartDate = null;

                            if (ganttData.UpdatedTask.Duration != null && ganttData.UpdatedTask.Duration.HasValue)
                                updateTask.Duration = ganttData.UpdatedTask.Duration;
                            else updateTask.Duration = 0;

                            if (ganttData.UpdatedTask.EndDate != null && ganttData.UpdatedTask.EndDate.HasValue)
                                updateTask.EndDate = ganttData.UpdatedTask.EndDate;
                            else updateTask.EndDate = null;
                        }
                        break;
                    default:
                        ganttData.Action = GanttAction.Error;
                        break;
                }
            }
            else
            {
                ganttData.Action = GanttAction.Error;
            }
        }

        /// <summary>
        /// Update gantt links
        /// </summary>
        /// <param name="ganttData">GanttData object</param>
        private void UpdateLinks(GanttRequest ganttData)
        {
            switch (ganttData.Action)
            {
                case GanttAction.Inserted:
                    // add new gantt link
                    db.Links.Add(ganttData.UpdatedLink);
                    break;
                case GanttAction.Deleted:
                    // remove gantt link
                    db.Links.Remove(db.Links.Find(Convert.ToInt32(ganttData.SourceId)));
                    break;
                case GanttAction.Updated:
                    // update gantt link
                    db.Entry(db.Links.Find(ganttData.UpdatedLink.ID)).CurrentValues.SetValues(ganttData.UpdatedLink);
                    break;
                default:
                    ganttData.Action = GanttAction.Error;
                    break;
            }
        }

        /// <summary>
        /// Create XML response for gantt
        /// </summary>
        /// <param name="ganttData">Gantt data</param>
        /// <returns>XML response</returns>
        private ContentResult GanttRespose(List<GanttRequest> ganttDataCollection)
        {
            var actions = new List<XElement>();
            foreach (var ganttData in ganttDataCollection)
            {
                if (ganttData.Action.ToString() != "Error")
                {
                    var action = new XElement("action");
                    action.SetAttributeValue("type", ganttData.Action.ToString().ToLower());
                    action.SetAttributeValue("sid", ganttData.SourceId);

                    // action.SetAttributeValue("tid", (ganttData.Mode == GanttMode.Tasks) ? ganttData.UpdatedTask.ID.ToString() : ganttData.UpdatedLink.ID.ToString());
                    if (ganttData.Mode == GanttMode.Tasks)
                    {
                        if (ganttData.UpdatedTask != null)
                            action.SetAttributeValue("tid", ganttData.UpdatedTask.ID.ToString());
                        else action.SetAttributeValue("tid", null);
                    }
                    else
                    {
                        if (ganttData.UpdatedLink != null)
                            action.SetAttributeValue("tid", ganttData.UpdatedLink.ID);
                        else action.SetAttributeValue("tid", null);
                    }

                    actions.Add(action);
                }
                else
                {
                    return Content("Error", "text/xml");
                }
            }

            var data = new XDocument(new XElement("data", actions));
            data.Declaration = new XDeclaration("1.0", "utf-8", "true");
            return Content(data.ToString(), "text/xml");
        }
        #endregion

        #region Role
        [HttpPost]
        public ActionResult LoadRoleListAjax()
        {
            try
            {
                db = new QLDUANEntities();
                var model = from p in db.Roles
                            select new
                            {
                                p.ID,
                                p.Name
                            };

                return Json(model);
            }
            catch
            {
                return Json("");
            }
        }
        #endregion

        #region User list
        public ActionResult UserList()
        {
            db = new QLDUANEntities();

            if (Session["Login"] != null && Session["Login"].ToString().Trim() != "")
            {
                var model = db.Users.ToList();

                // GET THONG TIN NGUOI DUNG
                var user = db.Users.Find(Session["Login"].ToString().Trim());
                ViewBag._fullname = user.FullName;
                ViewBag._sex = user.Sex;

                if (user.Birthday != null && user.Birthday.HasValue)
                {
                    ViewBag._birthday = user.Birthday.Value.ToString("dd/MM/yyyy");
                    ViewBag._birthdayinfo = user.Birthday.Value.ToString("MM/dd/yyyy");
                }
                else
                {
                    ViewBag._birthday = "";
                    ViewBag._birthdayinfo = "";
                }
                ViewBag._address = (user.Address != null ? user.Address : "");

                return View(model);
            }
            else
            {
                return View("Login");
            }
        }

        [HttpPost]
        public ActionResult LoadUserListAjax()
        {
            try
            {
                db = new QLDUANEntities();
                var model = from p in db.Users
                            where p.ID.Trim().ToUpper() != "ADMIN"
                            select new
                            {
                                MA = p.ID,
                                TEN = p.FullName + (p.Email != null ? " (" + p.Email + ")" : ""),
                                FILE = "user-default.png"
                            };

                return Json(model);
            }
            catch
            {
                return Json("");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(true)]
        public ActionResult UserUpdate(User us, string FLAG)
        {
            ViewData["DialogUser"] = "true";
            ViewData["DialogUser_CapNhat"] = null;
            ViewData["USER_CAPNHAT"] = null;

            try
            {
                if (Session["Login"] != null && Session["Login"].ToString().Trim() != "")
                {
                    db = new QLDUANEntities();

                    if (string.IsNullOrEmpty(FLAG))
                    {
                        return Json(new
                        {
                            STATUS = "false",
                            MESSAGE = "Không xác định được thao tác người dùng! Vui lòng refesh lại trang web."
                        });
                    }
                    else if (string.IsNullOrEmpty(us.ID) || string.IsNullOrEmpty(us.FullName) || string.IsNullOrEmpty(us.RoleID)
                        || string.IsNullOrEmpty(us.Password))
                    {
                        return Json(new
                        {
                            STATUS = "false",
                            MESSAGE = "Vui lòng nhập đầy đủ dữ liệu!"
                        });
                    }
                    else if (ModelState.IsValid)
                    {
                        if (FLAG.Trim().ToUpper() == "0")
                        {
                            #region Insert USER
                            var data = db.Users.Find(us.ID.Trim());
                            if (data != null)
                            {
                                return Json(new
                                {
                                    STATUS = "false",
                                    MESSAGE = "Trùng mã người dùng. Vui lòng nhập mã khác!"
                                });
                            }
                            else
                            {
                                User user = new User();

                                user.ID = us.ID.Trim().ToUpper();
                                user.FullName = us.FullName.Trim().ToUpper();
                                user.Sex = us.Sex;
                                user.Address = us.Address;
                                user.Phone = us.Phone;
                                user.Email = us.Email;
                                user.UserName = us.ID;
                                user.Password = us.Password;
                                user.Description = us.Description;

                                if (us.Birthday != null && us.Birthday.HasValue)
                                    user.Birthday = us.Birthday;
                                else user.Birthday = null;

                                if (us.RoleID != null && us.RoleID.Trim() != "")
                                    user.RoleID = us.RoleID;
                                else user.RoleID = null;

                                user.CreatedDate = DateTime.Now;

                                db.Users.Add(user);
                                db.SaveChanges();

                                return Json(new
                                {
                                    STATUS = "true",
                                    MESSAGE = "Thêm dữ liệu thành công!"
                                });
                            }
                            #endregion
                        }
                        else if (FLAG.Trim().ToUpper() == "1")
                        {
                            #region Update USER
                            var user = db.Users.Find(us.ID.Trim());
                            if (user != null)
                            {
                                user.FullName = us.FullName.Trim().ToUpper();
                                user.Sex = us.Sex;
                                user.Address = us.Address;
                                user.Phone = us.Phone;
                                user.Email = us.Email;
                                user.UserName = us.ID;
                                user.Password = us.Password;
                                user.Description = us.Description;

                                if (us.Birthday != null && us.Birthday.HasValue)
                                    user.Birthday = us.Birthday;
                                else user.Birthday = null;

                                if (us.RoleID != null && us.RoleID.Trim() != "")
                                    user.RoleID = us.RoleID;
                                else user.RoleID = null;

                                user.CreatedDate = DateTime.Now;

                                db.SaveChanges();

                                return Json(new
                                {
                                    STATUS = "true",
                                    MESSAGE = "Cập nhật dữ liệu thành công!"
                                });
                            }
                            else
                            {
                                return Json(new
                                {
                                    STATUS = "false",
                                    MESSAGE = "Không xác định được đối tượng cần sửa dữ liệu!"
                                });
                            }
                            #endregion
                        }
                        else
                        {
                            return Json(new
                            {
                                STATUS = "false",
                                MESSAGE = "Không xác định được thao tác người dùng! Vui lòng refesh lại trang web."
                            });
                        }
                    }
                    else
                    {
                        return Json(new
                        {
                            STATUS = "false",
                            MESSAGE = "Rất tiếc đã xảy ra lỗi, vui lòng kiểm tra lại thông tin nhập vào!"
                        });
                    }
                }
                else
                {
                    return Json(new
                    {
                        STATUS = "false",
                        MESSAGE = "Bạn chưa đăng nhập!"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    STATUS = "false",
                    MESSAGE = "Rất tiếc đã xảy ra lỗi, vui lòng kiểm tra lại thông tin nhập vào!"
                });
            }
        }

        [HttpPost]
        public ActionResult LoadUser(string ID)
        {
            try
            {
                if (ID != null && ID.ToString().Trim() != "")
                {
                    db = new QLDUANEntities();
                    var data = (from p in db.Users
                                where p.ID == ID
                                select new
                                {
                                    p.ID,
                                    p.FullName,
                                    p.Birthday,
                                    p.Sex,
                                    p.Address,
                                    p.Phone,
                                    p.Email,
                                    p.Password,
                                    p.Description,
                                    p.CreatedDate,
                                    p.RoleID
                                }).FirstOrDefault();

                    if (data != null)
                    {
                        return Json(data);
                    }
                    else return Json("");
                }
                else return Json("Không thể load dữ liệu user! Vui lòng thử lại sau.");
            }
            catch
            {
                return Json("");
            }
        }

        [HttpPost]
        public ActionResult DeleteUser(string ID)
        {
            try
            {
                if (!String.IsNullOrEmpty(ID))
                {
                    db = new QLDUANEntities();

                    // 1. UPDATE STATUS = -1; 
                    var user = db.Users.Find(ID);
                    if (user != null)
                    {
                        db.Users.Remove(user); // DELETE 
                        db.SaveChanges();
                    }

                    return Json("Đã xóa dữ liệu thành công!");
                }
                else return Json("Không xác định được đối tượng cần xóa dữ liệu!");
            }
            catch
            {
                return Json("Rất tiếc, đã xảy ra lỗi. Có thể do ràng buộc toàn vẹn về dữ liệu. Vui lòng kiểm tra và thử lại sau!");
            }
        }
        #endregion

        #region Post Comment
        [HttpPost]
        public ActionResult LoadCommentByTask(string ID)
        {
            try
            {
                if (String.IsNullOrEmpty(ID))
                {
                    return Json(new
                    {
                        STATUS = "false",
                        MESSAGE = "Rất tiếc, đã xảy ra lỗi, không thể load comment người dùng"
                    });
                }
                else
                {
                    db = new QLDUANEntities();

                    var user = db.Users.Find(Session["Login"].ToString().Trim());
                    if (user != null)
                    {
                        var model = (from p in db.Tasks
                                     where p.ID.Trim().ToUpper() == ID.Trim().ToUpper()
                                            && p.Status != -1
                                     select p).FirstOrDefault();

                        return PartialView("PartialPostComment", model);
                    }
                    else
                    {
                        return Json(new
                        {
                            STATUS = "false",
                            MESSAGE = "Rất tiếc, đã xảy ra lỗi, không thể load comment người dùng!"
                        });
                    }
                }
            }
            catch
            {
                return Json(new
                {
                    STATUS = "false",
                    MESSAGE = "Rất tiếc, đã xảy ra lỗi, không thể load comment người dùng!"
                });
            }
        }

        [HttpPost]
        public ActionResult PostComment(string TaskID = "", string Comment = "")
        {
            try
            {
                db = new QLDUANEntities();
                if (Session["Login"].ToString().Trim() != null && TaskID.Trim() != "" && Comment.Trim() != "")
                {
                    Comment c = new Comment();
                    c.TaskID = TaskID;
                    c.UserID = Session["Login"].ToString().Trim();
                    c.CreatedDate = DateTime.Now;
                    c.Description = Comment;

                    db.Comments.Add(c);
                    db.SaveChanges();

                    var user = db.Users.Find(Session["Login"].ToString().Trim());
                    if (user != null)
                    {
                        var model = (from p in db.Tasks
                                     where p.ID.Trim().ToUpper() == TaskID.Trim().ToUpper()
                                            && p.Status != -1
                                     select p).FirstOrDefault();

                        return PartialView("PartialPostComment", model);
                    }
                    else
                    {
                        return Json(new
                        {
                            STATUS = "false",
                            MESSAGE = "Rất tiếc, đã xảy ra lỗi, không thể load comment người dùng!"
                        });
                    }
                }
                else
                {
                    return Json(new
                    {
                        STATUS = "false",
                        MESSAGE = "Rất tiếc, đã xảy ra lỗi, vui lòng kiểm tra lại thông tin nhập vào!"
                    });
                }
            }
            catch
            {
                return Json(new
                {
                    STATUS = "false",
                    MESSAGE = "Rất tiếc, đã xảy ra lỗi, vui lòng kiểm tra lại thông tin nhập vào!"
                });
            }
        }
        #endregion

        #region Login - Logout - SignUp
        [HttpGet]
        public ActionResult Login()
        {
            if (Session["Login"] != null && Session["Login"].ToString().Trim() != "")
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Login f)
        {
            try
            {
                if (f.UserID == null || f.UserID.Trim() == "")
                {
                    Session["Login"] = null;
                    return Json(new
                    {
                        STATUS = "false",
                        MESSAGE = "Vui lòng nhập ID đăng nhập!"
                    });
                }
                else if (f.Password == null || f.Password.Trim() == "")
                {
                    Session["Login"] = null;
                    return Json(new
                    {
                        STATUS = "false",
                        MESSAGE = "Vui lòng nhập mật khẩu!"
                    });
                }
                else if (ModelState.IsValid)
                {
                    db = new QLDUANEntities();
                    var user = db.Users.Find(f.UserID.Trim());

                    if (user != null)
                    {
                        // Check Mat khau
                        if (user.Password == f.Password)
                        {
                            Session["Login"] = user.ID;
                            if (user.ID.Trim().ToUpper() == "ADMIN")
                                Session["Role"] = "Admin";
                            else Session["Role"] = "User";

                            return Json(new
                            {
                                STATUS = "true",
                                MESSAGE = "Đăng nhập thành công!"
                            });
                        }
                        else
                        {
                            Session["Login"] = null;
                            return Json(new
                            {
                                STATUS = "false",
                                MESSAGE = "Mật khẩu hoặc tên đăng nhập không đúng!"
                            });
                        }
                    }
                    else
                    {
                        Session["Login"] = null;
                        return Json(new
                        {
                            STATUS = "false",
                            MESSAGE = "Mật khẩu hoặc tên đăng nhập không đúng!"
                        });
                    }
                }
                else
                {
                    Session["Login"] = null;
                    return Json(new
                    {
                        STATUS = "false",
                        MESSAGE = "Đã xảy ra lỗi trong quá trình đăng nhập, vui lòng kiểm tra lại!"
                    });
                }
            }
            catch
            {
                Session["Login"] = null;
                return Json(new
                {
                    STATUS = "false",
                    MESSAGE = "Đã xảy ra lỗi trong quá trình đăng nhập, vui lòng kiểm tra lại!"
                });
            }
        }

        [HttpGet]
        public ActionResult Logout()
        {
            try
            {
                if (Session["Login"] != null && Session["Login"].ToString().Trim() != "")
                {
                    Session.Abandon();
                }
                return RedirectToAction("Login", "Home");
            }
            catch
            {
                return RedirectToAction("Login", "Home");
            }
        }

        public ActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(true)]
        public ActionResult SignUp(User us)
        {
            try
            {
                if (us != null)
                {
                    db = new QLDUANEntities();

                    if (string.IsNullOrEmpty(us.ID) || string.IsNullOrEmpty(us.FullName)
                        || string.IsNullOrEmpty(us.Email) || string.IsNullOrEmpty(us.Password))
                    {
                        ModelState.AddModelError("SignUp", "Vui lòng nhập đầy đủ các trường dữ liệu được đánh dấu (*) !");
                    }
                    else
                    {
                        var data = db.Users.Find(us.ID.Trim());
                        if (data != null)
                        {
                            ModelState.AddModelError("SignUp", "Mã UserID đã tồn tại! Vui lòng nhập mã khác.");
                        }
                        else if (ModelState.IsValid)
                        {
                            User user = new User();
                            user.ID = us.ID.Trim().ToUpper();
                            user.FullName = us.FullName.Trim().ToUpper();

                            if (us.Birthday != null && us.Birthday.HasValue)
                                user.Birthday = us.Birthday;
                            else user.Birthday = null;

                            user.Sex = us.Sex;
                            user.Address = us.Address;
                            user.Phone = us.Phone;
                            user.Email = us.Email;
                            user.UserName = us.ID;
                            user.Password = us.Password;
                            user.CreatedDate = DateTime.Now;
                            user.Description = us.Description;

                            user.RoleID = "USER"; // Default

                            db.Users.Add(user);
                            db.SaveChanges();

                            ViewData["SignUp"] = "Đăng ký tài khoản thành công!.";
                            return View();
                        }
                        else
                        {
                            ViewData["SignUp"] = "Lỗi dữ liệu, vui lòng kiểm tra lại dữ liệu nhập vào!";
                            return View();
                        }
                    }
                    return View();
                }
                else
                {
                    ViewData["SignUp"] = "Đã xảy ra lỗi, vui lòng thử lại sau!";
                    return View();
                }
            }
            catch
            {
                ViewData["SignUp"] = "Đã xảy ra lỗi, vui lòng thử lại sau!";
                return View();
            }
        }
        #endregion

        #region Change Password
        [HttpGet]
        public ActionResult ChangePass()
        {
            var user = db.Users.Find(Session["Login"].ToString().Trim());
            if (user != null)
            {
                // GET THONG TIN NGUOI DUNG
                ViewBag._fullname = user.FullName;
                ViewBag._sex = user.Sex;

                if (user.Birthday != null && user.Birthday.HasValue)
                {
                    ViewBag._birthday = user.Birthday.Value.ToString("dd/MM/yyyy");
                    ViewBag._birthdayinfo = user.Birthday.Value.ToString("MM/dd/yyyy");
                }
                else
                {
                    ViewBag._birthday = "";
                    ViewBag._birthdayinfo = "";
                }
                ViewBag._address = (user.Address != null ? user.Address : "");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(true)]
        public ActionResult ChangePass(string txtMATKHAU, string txtMATKHAU_NEW, string txtMATKHAU_RENEW)
        {
            try
            {
                ViewBag.MATKHAU = "";
                ViewBag.MATKHAU_NEW = "";
                ViewBag.MATKHAU_RENEW = "";

                if (Session["Login"] != null && Session["Login"].ToString().Trim() != "")
                {
                    db = new QLDUANEntities();
                    var user = db.Users.Find(Session["Login"].ToString().Trim().ToUpper());

                    if (string.IsNullOrEmpty(txtMATKHAU) || string.IsNullOrEmpty(txtMATKHAU_NEW) || string.IsNullOrEmpty(txtMATKHAU_RENEW))
                    {
                        ModelState.AddModelError("MATKHAU", "Mật khẩu không được để trống!");

                        ViewBag.MATKHAU = txtMATKHAU;
                        ViewBag.MATKHAU_NEW = txtMATKHAU_NEW;
                        ViewBag.MATKHAU_RENEW = txtMATKHAU_RENEW;
                    }
                    else if (txtMATKHAU_NEW.Trim() != txtMATKHAU_RENEW.Trim())
                    {
                        ModelState.AddModelError("MATKHAU", "Mật khẩu mới nhập không trùng nhau!");

                        ViewBag.MATKHAU = txtMATKHAU;
                        ViewBag.MATKHAU_NEW = txtMATKHAU_NEW;
                        ViewBag.MATKHAU_RENEW = txtMATKHAU_RENEW;
                    }
                    else
                    {
                        if (user != null)
                        {
                            if (user.Password.Trim() != txtMATKHAU.Trim())
                            {
                                ModelState.AddModelError("MATKHAU", "Mật khẩu hiện tại không chính xác!");

                                ViewBag.MATKHAU = txtMATKHAU;
                                ViewBag.MATKHAU_NEW = txtMATKHAU_NEW;
                                ViewBag.MATKHAU_RENEW = txtMATKHAU_RENEW;
                            }
                            else
                            {
                                user.Password = txtMATKHAU_NEW.Trim();
                                db.SaveChanges();

                                ViewBag.MATKHAU = "";
                                ViewBag.MATKHAU_NEW = "";
                                ViewBag.MATKHAU_RENEW = "";

                                ViewData["CHANGEPASS"] = "Đổi mật khẩu thành công! Vui lòng đăng nhập lại.";
                                Session.Abandon();
                                return View("Login");
                            }
                        }
                        else
                        {
                            return View("Login");
                        }
                    }
                    return View(user);
                }
                else
                {
                    return View("Login");
                }
            }
            catch
            {
                ViewData["PASSWORD"] = "Đã xảy ra lỗi, vui lòng thử lại sau!";

                ViewBag.MATKHAU = txtMATKHAU;
                ViewBag.MATKHAU_NEW = txtMATKHAU_NEW;
                ViewBag.MATKHAU_RENEW = txtMATKHAU_RENEW;
                return View();
            }
        }
        #endregion

        #region Profile
        public ActionResult UserProfile()
        {
            if (Session["Login"] != null && Session["Login"].ToString().Trim() != "")
            {
                var data = db.Users.Find(Session["Login"].ToString().Trim());

                // GET THONG TIN NGUOI DUNG
                ViewBag._fullname = data.FullName;
                ViewBag._sex = data.Sex;

                if (data.Birthday != null && data.Birthday.HasValue)
                {
                    ViewBag._birthday = data.Birthday.Value.ToString("dd/MM/yyyy");
                    ViewBag._birthdayinfo = data.Birthday.Value.ToString("MM/dd/yyyy");
                }
                else
                {
                    ViewBag._birthday = "";
                    ViewBag._birthdayinfo = "";
                }
                ViewBag._address = (data.Address != null ? data.Address : "");

                return View(data);
            }
            else
            {
                return View("Login");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(true)]
        public ActionResult UpdateInfoUser(User us)
        {
            try
            {
                if (Session["Login"] != null && Session["Login"].ToString().Trim() != "")
                {
                    db = new QLDUANEntities();
                    var data = db.Users.Find(Session["Login"].ToString().Trim().ToUpper());
                    if (data != null)
                    {
                        if (us != null)
                        {
                            data.FullName = us.FullName.Trim();

                            if (us.Birthday != null && us.Birthday.HasValue)
                                data.Birthday = us.Birthday;
                            else data.Birthday = null;

                            data.Sex = us.Sex;
                            data.Address = us.Address;
                            data.Phone = us.Phone;
                            data.Email = us.Email;
                            data.UserName = us.ID;
                            data.Password = us.Password;
                            data.CreatedDate = DateTime.Now;
                            data.Description = us.Description;

                            db.SaveChanges();

                            // GET THONG TIN NGUOI DUNG
                            ViewBag._fullname = data.FullName;
                            ViewBag._sex = data.Sex;

                            if (data.Birthday != null && data.Birthday.HasValue)
                            {
                                ViewBag._birthday = data.Birthday.Value.ToString("dd/MM/yyyy");
                                ViewBag._birthdayinfo = data.Birthday.Value.ToString("MM/dd/yyyy");
                            }
                            else
                            {
                                ViewBag._birthday = "";
                                ViewBag._birthdayinfo = "";
                            }
                            ViewBag._address = (data.Address != null ? data.Address : "");

                            ViewData["UpdateInfo"] = "Bạn đã cập nhật thông tin thành công!";
                            return View("UserProfile", data);
                        }
                        else
                        {
                            ViewData["UpdateInfo"] = "Không xác định được đối tượng cần sửa dữ liệu!";
                        }
                    }
                    return View("UserProfile");
                }
                else return View("Login");
            }
            catch
            {
                ViewData["UpdateInfo"] = "Cập nhật thất bại. Vui lòng thử lại sau!";
                return View("UserProfile");
            }
        }
        #endregion
    }
}