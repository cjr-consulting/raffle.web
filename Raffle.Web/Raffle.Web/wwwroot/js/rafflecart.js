
(function (win) {
    async function postData(url = '', data = {}) {
        const response = await fetch(url, {
            method: 'POST',
            mode: 'same-origin',
            credentials: 'same-origin', // include, *same-origin, omit
            headers: {
                'Content-Type': 'application/json'
            },
            redirect: 'follow',
            referrerPolicy: 'no-referrer', // no-referrer, *no-referrer-when-downgrade, origin, origin-when-cross-origin, same-origin, strict-origin, strict-origin-when-cross-origin, unsafe-url
            body: JSON.stringify(data)
        });
        return response.json();
    }

    function setupCountdown() {
        let timeCountDown = document.getElementById("timeCountdown");
        let utc = timeCountDown.getAttribute("utc");
        let format = timeCountDown.getAttribute("format");
        format = format || "MM/DD/YYYY h:mm a";
        let d = moment(new Date(utc));
        timeCountDown.textContent = "That's just " + d.fromNow(true) + " away!!";
    }

    function setupTicketInput() {
        var elements = document.getElementsByClassName("ticketInput")
        for (var i = 0; i < elements.length; i++) {
            elements[i].addEventListener("blur", updateOrder, true);
        }
    }

    function updateOrder(event) {
        let id = event.currentTarget.dataset.raffleItemId;
        let value = parseInt(event.currentTarget.value);
        postData("/updateorder/updateitem/" + id, { Amount: value })
            .then(data => {
                console.debug(data);
            });
    }

    setupTicketInput();
    setupCountdown();

})(window);
