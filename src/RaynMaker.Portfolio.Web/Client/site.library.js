$.ajaxSetup({ cache: false });

function formatValue(value) {
    if (typeof value === 'string' || value instanceof String) return value
    let val = (value / 1).toFixed(2).replace('.', ',')
    return val.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ".")
}

function randomColor() {
    function randomInt(min, max) {
        return Math.floor(Math.random() * (max - min + 1)) + min;
    }

    var h = randomInt(0, 360);
    var s = randomInt(42, 98);
    var l = randomInt(40, 90);
    return "hsl(" + h + "," + s + "%," + l +"%)";
}

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

Vue.component('pie-chart', {
    extends: VueChartJs.Pie,
    props: {
        data: Array,
        labels: Array
    },
    mounted: function () {
        var backgrounds = [];
        for (var i in this.data) {
            backgrounds.push(randomColor());
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

