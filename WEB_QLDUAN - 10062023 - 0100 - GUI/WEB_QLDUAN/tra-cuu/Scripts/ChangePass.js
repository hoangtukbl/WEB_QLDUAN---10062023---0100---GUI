$(document).ready(function () {

    $('#ChangePassLogin').hide();
    $("#Oldpass").focus();

    $(function () {
        setInterval(function () {
            var dayNames = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"]
            var today = new Date();

            var dd = today.getDate();
            var MM = today.getMonth() + 1;
            var yyyy = today.getFullYear();
            var hh = today.getHours();
            var mm = today.getMinutes();
            var ss = today.getSeconds();

            if (dd < 10) {
                dd = '0' + dd;
            }

            if (MM < 10) {
                MM = '0' + MM;
            }

            if (hh < 10) {
                hh = '0' + hh;
            }

            if (mm < 10) {
                mm = '0' + mm;
            }

            if (ss < 10) {
                ss = '0' + ss;
            }

            var time = dayNames[today.getDay()] + ", ngày " + dd + "/" + MM + "/" + yyyy + " " + hh + ":" + mm + ":" + ss;
            $("span#myDateTime").html(time);

        }, 1000);
    });

    $('#btnReset').off('click').on('click', function () {
        SetError('');
        $("#Oldpass").focus();
    });

    function SetError(errorMessage) {
        if (errorMessage != null && errorMessage != "") {
            $('#lbldivError').html("<div class='note-tam-giac'></div><div class='error-message' runat='server' id='divError'>" + errorMessage + "</div>");
        }
        else {
            $('#lbldivError').html("");
        }
    };

    $('#Oldpass').on('keypress', function (event) {
        var keycode = (event.keyCode ? event.keyCode : event.which);
        if (keycode == '13') {
            event.preventDefault();
            $('#Newpass1').focus();
        }
        else {
        }
    });

    $('#Newpass1').on('keypress', function (event) {
        var keycode = (event.keyCode ? event.keyCode : event.which);
        if (keycode == '13') {
            event.preventDefault();
            $('#Newpass2').focus();
        }
        else {
        }
    });

    $('#Newpass2').on('keypress', function (event) {
        var keycode = (event.keyCode ? event.keyCode : event.which);
        if (keycode == '13') {
            event.preventDefault();
            $('#btnChangePass').focus();
        }
        else {
        }
    });

    $('#btnChangePass').off('click').on('click', function (event) {
        $("#ChangePassLogin").show();
        SetError("");

        if ($('#Oldpass').val().trim() == null || $('#Oldpass').val().trim() == "") {
            SetError("Bạn vui lòng nhập mật khẩu cũ!");
            $("#ChangePassLogin").hide();
            $('#Oldpass').focus();
        }
        else if ($('#Newpass1').val().trim() == null || $('#Newpass1').val().trim() == "") {
            SetError("Bạn vui lòng nhập mật khẩu mới!");
            $("#ChangePassLogin").hide();
            $('#Newpass1').focus();
        }
        else if ($('#Newpass2').val().trim() == null || $('#Newpass2').val().trim() == "") {
            SetError("Bạn vui lòng nhập lại mật khẩu mới!");
            $("#ChangePassLogin").hide();
            $('#Newpass2').focus();
        }
        else if ($('#Newpass1').val().trim() != $('#Newpass2').val().trim()) {
            SetError("Mật khẩu mới không trùng khớp!");
            $("#ChangePassLogin").hide();
        }
        else if ($('#Oldpass').val().trim() == $('#Newpass1').val().trim() && $('#Newpass1').val().trim() == $('#Newpass2').val().trim()) {
            SetError("Mật khẩu mới không được trùng mật khẩu cũ!");
            $("#ChangePassLogin").hide();
        }
        else {
            var form = $('#frmChangePass');
            var token = $('input[name="__RequestVerificationToken"]', form).val();
            var uri = "/Project/ChangePass";
            var oldpass = $('#Oldpass').val().trim();
            var newpass1 = $('#Newpass1').val().trim();
            var newpass2 = $('#Newpass2').val().trim();

            $.ajax({
                url: uri,
                data: {
                    __RequestVerificationToken: token,
                    OLDPASS: oldpass,
                    NEWPASS1: newpass1,
                    NEWPASS2: newpass2
                },
                type: "POST",

                // contentType: "application/json; charset=utf-8",
                success: function (response) {
                    $("#ChangePassLogin").hide();

                    if (response.STATUS == "true") {
                        alert(response.MESSAGE);
                        if (response.LOGIN == "true") {
                            location.href = "/Home/Login";
                        }
                    }
                    else if (response.STATUS == "false") {
                        if (response.LOGIN == "true") {
                            alert(response.MESSAGE);
                            location.href = "/Home/Login";
                        }
                        else {
                            SetError(response.MESSAGE);
                            if (response.FOCUS == "true") {
                                $("#Oldpass").focus();
                            }
                        }
                    }
                    else {
                        SetError("Có lỗi trong quá trình cập nhật. Vui lòng kiểm tra lại!");
                    }
                },
                error: function (response) {
                    SetError("Có lỗi trong quá trình cập nhật. Vui lòng kiểm tra lại!");
                    $("#ChangePassLogin").hide();
                }
            });
        }
    });
});