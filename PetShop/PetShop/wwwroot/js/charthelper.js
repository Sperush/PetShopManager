window.chartHelper = {
    createPieChart: function (canvasId, labels, data, backgroundColor) {
        var ctx = document.getElementById(canvasId).getContext('2d');
        

        if (window.myPieChart) {
            window.myPieChart.destroy();
        }

        window.myPieChart = new Chart(ctx, {
            type: 'pie',
            data: {
                labels: labels,
                datasets: [{
                    data: data,
                    backgroundColor: backgroundColor,
                    borderWidth: 0
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            padding: 20,
                            font: {
                                size: 12
                            }
                        }
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                var label = context.label || '';
                                var value = context.parsed || 0;
                                var total = context.dataset.data.reduce((a, b) => a + b, 0);
                                var percentage = ((value / total) * 100).toFixed(1) + "%";
                                return label + ": " + value + " bé (" + percentage + ")";
                            }
                        }
                    }
                }
            }
        });
    }
};

