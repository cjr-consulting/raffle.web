
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
            let dueDate = moment(d);
            element.text("That's just " + dueDate.fromNow(true) + " away!!");
        });

        jQuery(".localTime").each(function (i, obj) {
            var element = $(this);
            var utc = element.attr("utc");
            var d = moment(new Date(utc));
            element.text(d.local().format('MM/DD/YYYY h:mm:ss a'))
        })
    });

})(window);