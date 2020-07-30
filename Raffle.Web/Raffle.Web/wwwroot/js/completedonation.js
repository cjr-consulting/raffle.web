
(function (win) {
    jQuery(document).ready(jQuery => {
        jQuery("#IsInternational").on("click", (event) => {
            if (event.currentTarget.checked) {
                jQuery("#international_address").show();
                jQuery("#us_address").hide();
            } else {
                jQuery("#international_address").hide();
                jQuery("#us_address").show();
            }
        });

        if (document.getElementById("IsInternational").checked) {
            jQuery("#international_address").show();
            jQuery("#us_address").hide();
        } else {
            jQuery("#international_address").hide();
            jQuery("#us_address").show();
        }
    });

})(window);