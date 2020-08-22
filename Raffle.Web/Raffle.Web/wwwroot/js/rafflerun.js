"use strict";
(function () {
    let isLoaded = false;
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/RaffleRunHub")
        .withAutomaticReconnect()
        .build();

    connection
        .start()
        .catch(function (err) {
            return console.error(err.toString());
        });

    const rowTemplate = Handlebars.compile("<tr id=\"raffleItem-{{id}}\">" +
        "<td><div class='row'>{{#if image}}<div class='col-md-2'><img class='img-fluid rounded' width='150' style='margin-right:5px' src='{{image}}'/></div>{{/if}}" +
        "<div class='col-md-8'><h5>{{title}}</h5>" +
        "{{#if itemValue}}<b>Est. value of {{itemValue}}</b><br/>{{/if}} " +
        "{{#if description}}{{description}}<br/>{{/if}}" +
        "{{#if sponsor}}<b>Donated By: </b><span class='font-italic'>{{sponsor}}</span>{{/if}}" +
        "</div>" +
        "</div></td>" +
        //"<td><ul style='list-style-type: none;'>{{#each winningTickets}}<li>{{number}} {{#if for}}({{for}}){{/if}}</li>{{/each}}</ul></td>" +
        "<td nowrap='nowrap'><ul style='list-style-type: none;padding-left:0px'>{{#each winners}}<li>{{#if for}}({{for}}){{else}}[{{ticketNumber}}]{{/if}}<div>{{name}}</div></li>{{/each}}</ul></td>" +
        "</tr>");
    const raffleList = document.getElementById("raffleList");
    let raffleItems = [];

    function raffleItemsDesc(a, b) {
        if (a.updatedDate > b.updatedDate) {
            return -1;
        } else if (a.updatedDate < b.updatedDate) {
            return 1;
        }

        return 0;
    }

    function raffleItemUpdate(raffleItem) {

        var itemIndex = raffleItems.findIndex((item) => { return item.id === raffleItem.id; });
        if (itemIndex >= 0) {
            raffleItems.splice(itemIndex, 1);
        }

        if (raffleItem.winners.length > 0) {
            raffleItems.push(raffleItem);
        }

        console.log("receied update");
        console.log("raffleItem", raffleItem);        

        refreshList();

        var row = document.getElementById("raffleItem-" + raffleItem.id);
        row.setAttribute("class", "table-success");
        setTimeout(() => { row.removeAttribute("class"); }, 2500);
    }

    function refreshList() {
        let listHtml = '';
        raffleItems = raffleItems.sort(raffleItemsDesc);

        for (var i = 0; i < raffleItems.length; i++) {
            let item = raffleItems[i];
            listHtml += rowTemplate(item);
        }

        if (listHtml === '') {
            raffleList.innerHTML = "<tr class='table-warning'><td colspan='3'><h5 class='text-center'>No Raffles Drawn Just Yet.</h5></id></tr>";
            return;
        }

        raffleList.innerHTML = listHtml;
    }

    fetch('/api/raffleitems')
        .then(response => response.json())
        .then(data => {
            raffleItems = data;
            isLoaded = true;
            refreshList();
        });

    connection.on("RaffleItemUpdated", (raffleItem) => raffleItemUpdate(raffleItem));
    setTimeout(() => {
        if (!isLoaded) {
            raffleList.innerHTML = "<tr class='table-info'><td colspan='3'><h5>Loading Raffle Items</h5></td></tr>";
        }
    }, 500);
})();