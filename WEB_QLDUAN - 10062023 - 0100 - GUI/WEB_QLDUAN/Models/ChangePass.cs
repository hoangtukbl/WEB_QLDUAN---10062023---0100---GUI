using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WEB_QLDUAN.Models
{
    public class ChangePass
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu!")]
        public string Oldpass { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới!")]
        public string Newpass1 { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu mới!")]
        public string Newpass2 { get; set; }
    }
}