function init() {
    $.ajaxSetup({ cache: false });

    var app = new Vue({
        el: '#app',
        data: {
            closedPositions: [],
            openPositions: []
        },

        created: function () {
            this.get('/api/closedPositions', {}, function (that, response) {
                that.closedPositions = response
            });
            this.get('/api/openPositions', {}, function (that, response) {
                that.openPositions = response
            });
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
