"use strict";
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
