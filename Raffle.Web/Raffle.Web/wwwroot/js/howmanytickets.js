"use strict";
(function (window) {
    let raffleItemChartctx = document.getElementById('raffleItems').getContext('2d');

    fetch('/api/orders/raffleitems')
        .then(response => response.json())
        .then(data => {
            console.debug("data", data);

            let labels = data.map(d => d.title);
            let countData = data.map(d => d.totalTicketsEntered);


            var chart = new Chart(raffleItemChartctx, {
                // The type of chart we want to create
                type: 'horizontalBar',
                // The data for our dataset
                data: {
                    labels: labels,
                    datasets: [{
                        label: 'Tickets',
                        backgroundColor: 'rgb(255, 99, 132, 0.75)',
                        borderColor: 'rgb(255, 65, 100)',
                        borderWidth: 2,
                        data: countData
                    }]
                },

                // Configuration options go here
                options: {
                    title: {
                        display: true,
                        text: 'Raffle Items Count'
                    },
                    responsive: true,
                    aspectRatio: 2.2,
                    scales: {
                        yAxes: [{
                            stacked: true,
                            ticks: {
                                stepSize: 10
                            }
                        }],
                        xAxes: [{
                            stacked: true,
                            ticks: {
                                stepSize: 5
                            }
                        }]
                    }
                }
            });

        });
})(window);