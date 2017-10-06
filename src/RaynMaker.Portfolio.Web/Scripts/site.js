function formatValue(value) {
    if (typeof value === 'string' || value instanceof String) return value
    let val = (value / 1).toFixed(2).replace('.', ',')
    return val.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ".")
}

function init() {
    $.ajaxSetup({ cache: false });

    Vue.component('grid', {
        template: '#grid-template',
        props: {
            data: Array,
            columns: Array,
            columnHeaders: Array,
            filterKey: String
        },
        data: function () {
            var sortOrders = {}
            this.columns.forEach(function (key) {
                sortOrders[key] = 1
            })
            return {
                sortKey: '',
                sortOrders: sortOrders
            }
        },
        computed: {
            filteredData: function () {
                var sortKey = this.sortKey
                var filterKey = this.filterKey && this.filterKey.toLowerCase()
                var order = this.sortOrders[sortKey] || 1
                var data = this.data
                if (filterKey) {
                    data = data.filter(function (row) {
                        return Object.keys(row).some(function (key) {
                            return String(row[key]).toLowerCase().indexOf(filterKey) > -1
                        })
                    })
                }
                if (sortKey) {
                    data = data.slice().sort(function (a, b) {
                        a = a[sortKey]
                        b = b[sortKey]
                        return (a === b ? 0 : a > b ? 1 : -1) * order
                    })
                }
                return data
            }
        },
        filters: {
            formatValue: formatValue
        },
        methods: {
            sortBy: function (key) {
                this.sortKey = key
                this.sortOrders[key] = this.sortOrders[key] * -1
            }
        }
    })

    var app = new Vue({
        el: '#app',
        data: {
            positions: [],
            filter: '',
            properties: ['name', 'isin', 'open', 'close', 'duration', 'marketProfit', 'dividendProfit', 'totalProfit',
                'marketRoi', 'dividendRoi', 'totalRoi', 'marketRoiAnual', 'dividendRoiAnual', 'totalRoiAnual'],
            headers: ['Name', 'Isin', 'Open', 'Close', 'Duration', 'Market', 'Dividend', 'Total',
                'Market', 'Dividend', 'Total', 'Market', 'Dividend', 'Total'],
            performance: null
        },
        created: function () {
            this.get('/api/positions', {}, function (that, response) {
                that.positions = response
            });
            this.get('/api/performance', {}, function (that, response) {
                that.performance = response
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
