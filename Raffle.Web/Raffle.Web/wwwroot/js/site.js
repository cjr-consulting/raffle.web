// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


(function (win) {
    jQuery(document).ready(jQuery => {
        
        jQuery(".localTime").each(function (i, obj) {
            let element = $(this);
            let utc = element.attr("utc");
            let format = element.attr("format");
            format = format || 'MM/DD/YYYY h:mm a';
            let d = moment(new Date(utc));

            element.text(d.local().format(format))
        })
    });

})(window);