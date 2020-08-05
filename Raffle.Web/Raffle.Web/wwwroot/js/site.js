// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


(function (win) {
    let timeElements = document.getElementsByClassName("localTime");

    for (let i = 0; i < timeElements.length; i++) {
        let utc = timeElements[i].getAttribute("utc");
        let format = timeElements[i].getAttribute("format");
        format = format || "MM/DD/YYYY h:mm a";
        let d = moment(new Date(utc));

        timeElements[i].textContent = d.local().format(format);
    }
})(window);
