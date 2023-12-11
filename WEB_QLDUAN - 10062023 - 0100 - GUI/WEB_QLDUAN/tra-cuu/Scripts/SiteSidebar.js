function scrollFilterInvoice() {
    var address = location.pathname.split("/");
    var controller = address[1];
    var action = address[2] || "index";

    if (controller == "Project" && action != "ChangePass") {
        var body = document.body;

        if ($('body').hasClass("sidebar-open")) {
            body.classList.remove("sidebar-open");
        }

        $('#TongHopHD')[0].scrollIntoView(false);
    }
    else {
        location.href = "/Home/"
    }
}