

(function (win) {
    jQuery(document).ready(jQuery => {

        jQuery(".ticketInput").on("blur", (event) => {
            let id = jQuery(event.currentTarget).data("raffleItemId");
            let value =parseInt(event.currentTarget.value);
            jQuery.post("/updateorder/updateitem/" + id,
                { Amount: value }
            );
            return false;
        });
    });

})(window);