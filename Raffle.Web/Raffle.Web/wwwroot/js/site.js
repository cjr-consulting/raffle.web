// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


(function (win) {
    jQuery(document).ready(jQuery => {
        
        jQuery(".localTime").each(function (i, obj) {
            var element = $(this);
            var utc = element.attr("utc");
            var d = moment(new Date(utc));
            element.text(d.local().format('MM/DD/YYYY h:mm:ss a'))
        })
    });

})(window);