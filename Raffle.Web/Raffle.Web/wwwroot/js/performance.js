(function (window) {
    let ordersChartctx = document.getElementById('ordersChart').getContext('2d');
    let totalPointsChartctx = document.getElementById('totalPointsChart').getContext('2d');
    let raffleItemChartctx = document.getElementById('raffleItems').getContext('2d');

    function onlyUnique(value, index, self) {
        return self.indexOf(value) === index;
    }

    fetch('/api/orders')
        .then(response => response.json())
        .then(data => {
            console.debug("data", data);
            let availableDate = data
                .map(i => moment(new Date(i.completedDate)).local().format('MM-DD'))
                .filter(onlyUnique);
            console.debug("availableDate", availableDate);

            let outstanding = data.filter(o => o.ticketNumber === '' || o.ticketNumber === null)
                .reduce((r, a) => {
                    r[moment(new Date(a.completedDate)).local().format('MM-DD')] = [...r[moment(new Date(a.completedDate)).local().format('MM-DD')] || [], a];
                    return r;
                }, {});
            console.debug("outstanding", outstanding);

            let complete = data.filter(o => o.ticketNumber !== null && o.ticketNumber !== '')
                .reduce((r, a) => {
                    r[moment(a.completedDate).local().format('MM-DD')] = [...r[moment(a.completedDate).local().format('MM-DD')] || [], a];
                    return r;
                }, {});

            let totalPoints = data
                .reduce((r, a) => {
                    r[moment(new Date(a.completedDate)).local().format('MM-DD')] = [...r[moment(new Date(a.completedDate)).local().format('MM-DD')] || [], a];
                    return r;
                }, {});
            console.debug("totalPoints", totalPoints);

            let startDate = moment("2020-7-31");
            let endDate = moment("2020-08-22");
            let labels = [];

            while (startDate <= endDate) {
                labels.push(startDate.format('MM-DD'));
                startDate = startDate.add(1, "days");
            }

            console.debug("labels", labels);

            var dataOutstanding = labels.map(value => {
                if (outstanding[value] === undefined) {
                    return 0;
                } else {
                    return outstanding[value].reduce((total, item) => { return total + item.totalPoints }, 0);
                }
            });
            console.debug("dataOutstanding", dataOutstanding);

            var dataComplete = labels.map(value => {
                if (complete[value] === undefined) {
                    return 0;
                } else {
                    return complete[value].reduce((total, item) => { return total + item.totalPoints }, 0);
                }
            });
            console.debug("dataComplete", dataComplete);

            let pointsSum = 0;

            var dataTotalPoints = labels.map(value => {
                if (totalPoints[value] !== undefined) {
                    pointsSum = pointsSum + totalPoints[value].reduce((total, item) => { return total + item.totalPoints }, 0);
                }

                return pointsSum;
            });

            console.debug("dataTotalPoints", dataTotalPoints);

            var chart = new Chart(ordersChartctx, {
                // The type of chart we want to create
                type: 'bar',
                // The data for our dataset
                data: {
                    labels: labels,
                    datasets: [{
                        label: 'Outstanding',
                        backgroundColor: 'rgb(255, 99, 132)',
                        data: dataOutstanding
                    },
                    {
                        label: 'Complete',
                        backgroundColor: 'rgb(0, 255, 0)',
                        data: dataComplete
                    }]
                },

                // Configuration options go here
                options: {
                    title: {
                        display: true,
                        text: 'Completed Order By Points'
                    },
                    responsive: true,
                    aspectRatio: 1.2,
                    scales: {
                        yAxes: [{
                            stacked: true,
                            ticks: {
                                stepSize: 25
                            }
                        }],
                        xAxes: [{
                            stacked: true
                        }]
                    }
                }
            });

            var chart2 = new Chart(totalPointsChartctx, {
                // The type of chart we want to create
                type: 'line',
                // The data for our dataset
                data: {
                    labels: labels,
                    datasets: [{
                        label: 'Points',
                        borderColor: 'rgb(255, 99, 132)',
                        data: dataTotalPoints
                    }]
                },
                // Configuration options go here
                options: {
                    title: {
                        display: true,
                        text: 'Total Points Over Time'
                    },
                    responsive: true,
                    aspectRatio: 1.1,
                    scales: {
                        yAxes: [{
                            ticks: {
                                stepSize: 25
                            }
                        }]
                    }
                }
            });

        });

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