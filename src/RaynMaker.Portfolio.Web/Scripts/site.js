
function init() {
    Vue.component('pie-chart', {
        extends: VueChartJs.Pie,
        props: {
            data: Array,
            labels: Array
        },
        mounted: function () {
            var dynamicColor = function () {
                var letters = '789ABCD'.split('');
                var color = '#';
                for (var i = 0; i < 6; i++) {
                    color += letters[Math.round(Math.random() * 6)];
                }
                return color;            };

            //backgroundColor: ["#3e95cd", "#8e5ea2", "#3cba9f", "#e8c3b9", "#c45850"],
            var backgrounds = [];
            for (var i in this.data) {
                backgrounds.push(dynamicColor());
            }

            this.renderChart({
                labels: this.labels,
                datasets: [{
                    backgroundColor: backgrounds,
                    data: this.data
                }]
            }, { responsive: false, maintainAspectRatio: false })
        }
    });

    var app = new Vue({
        el: '#app',
        data: {
            positions: [],
            filter: '',
            properties: ['name', 'isin', 'open', 'close', 'duration', 'marketProfit', 'dividendProfit', 'totalProfit',
                'marketRoi', 'dividendRoi', 'totalRoi', 'marketRoiAnual', 'dividendRoiAnual', 'totalRoiAnual'],
            headers: ['Name', 'Isin', 'Open', 'Close', 'Duration', 'Market', 'Dividend', 'Total',
                'Market', 'Dividend', 'Total', 'Market', 'Dividend', 'Total'],
            performance: null,
            benchmark: null,
            diversification: {
                data: null,
                labels: null
            }
        },
        created: function () {
            this.get('/api/positions', {}, function (that, response) {
                that.positions = response
            });
            this.get('/api/performance', {}, function (that, response) {
                that.performance = response
            });
            this.get('/api/benchmark', {}, function (that, response) {
                that.benchmark = response
            });
            this.get('/api/diversification', {}, function (that, response) {
                that.diversification.data = [2478, 5267, 734, 784, 433];
                that.diversification.labels = ["Africa", "Asia", "Europe", "Latin America", "North America"];
            });
        },
        filters: {
            formatValue: formatValue
        },
        methods: {
            get: function (url, data, onDone) {
                var that = this
                $.ajax({
                    url: url,
                    data: data,
                    dataType: 'json',
                    method: 'GET'
                }).then(function (response) {
                    if (response.error) {
                        console.err("ERROR: " + response.error);
                    } else {
                        onDone(that, response);
                    }
                }).catch(function (err) {
                    console.error("EXCEPTION:" + err);
                });
            }
        }
    })
}
