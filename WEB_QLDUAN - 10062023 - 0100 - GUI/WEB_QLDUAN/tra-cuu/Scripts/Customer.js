$(document).ready(function () {

    function SetRealTime() {
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
    };

    $(function () {
        setInterval(function () {
            SetRealTime();
        }, 1000);
    });
});

