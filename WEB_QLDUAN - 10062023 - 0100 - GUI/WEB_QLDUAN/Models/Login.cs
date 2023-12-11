using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WEB_QLDUAN.Models
{
    public class Login
    {
        [Required(ErrorMessage = "Vui lòng nhập ID đăng nhập!")]
        public string UserID { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu!")]
        public string Password { get; set; }
    }
}