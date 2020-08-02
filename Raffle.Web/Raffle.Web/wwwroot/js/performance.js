﻿let ordersChartctx = document.getElementById('ordersChart').getContext('2d');

function loadchart() {

}

function onlyUnique(value, index, self) {
    return self.indexOf(value) === index;
}

fetch('/api/orders')
    .then(response => response.json())
    .then(data => {
        console.log("data", data);
        let availableDate = data
            .map(i => moment(new Date(i.completedDate)).local().format('MM-DD'))
            .filter(onlyUnique);
        console.log("availableDate", availableDate);

        let outstanding = data.filter(o => o.ticketNumber === '' || o.ticketNumber === null)
            .reduce((r, a) => {
                console.log("a", a);
                console.log('r', r);
                r[moment(new Date(a.completedDate)).local().format('MM-DD')] = [...r[moment(new Date(a.completedDate)).local().format('MM-DD')] || [], a];
                return r;
            }, {});

        let complete = data.filter(o => o.ticketNumber !== null && o.ticketNumber !== '')
            .reduce((r, a) => {
                console.log("a", a);
                console.log('r', r);
                r[moment(a.completedDate).local().format('MM-DD')] = [...r[moment(a.completedDate).local().format('MM-DD')] || [], a];
                return r;
            }, {});

        console.log("outstanding", outstanding);

        let startDate = moment("2020-7-31");
        let endDate = moment("2020-08-22");
        let labels = [];

        while (startDate <= endDate) {
            labels.push(startDate.format('MM-DD'));
            startDate = startDate.add(1, "days");
        }

        console.log("labels", labels);

        var dataOutstanding = labels.map(value => {
            if (outstanding[value] === undefined) {
                return 0;
            } else {
                return outstanding[value].reduce((total, item) => { return total + item.totalPoints }, 0);
            }
        })

        console.log("dataOutstanding", dataOutstanding);


        var dataComplete = labels.map(value => {
            if (complete[value] === undefined) {
                return 0;
            } else {
                return complete[value].reduce((total, item) => { return total + item.totalPoints }, 0);
            }
        })

        console.log("dataComplete", dataComplete);

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
                            stepSize: 10
                        }
                    }],
                    xAxes: [{
                        stacked: true
                    }]
                }
            }
        });

    })