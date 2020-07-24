
(function (win) {
    jQuery(document).ready(jQuery => {
        jQuery(".ticketInput").on("blur", (event) => {
            let id = jQuery(event.currentTarget).data("raffleItemId");
            let value =parseInt(event.currentTarget.value);
            jQuery.post("/updateorder/updateitem/" + id,
                { Amount: value },
                (data) => {
                    console.log(data);
                }
            );
            return false;
        });

        var key = $("#timeCountdown").each(function (i, obj) {
            var element = $(this);
            var utc = element.attr("utc");
            var d = new Date(utc);
            var l = d.toLocaleString();
            let dueDate = moment(d);
            let today = moment();
            element.text("That's just " + moment.duration(dueDate.diff(today, "minutes"), "minutes").format() + " away!!");
        });
    });

})(window);