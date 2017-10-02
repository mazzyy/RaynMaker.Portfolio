function init() {
    var app = new Vue({
        el: '#app',
        data: {
            closedPositions: []
        },

        created: function () {
            this.get('/api/closedPositions', {}, function (that,response) {
                that.closedPositions = response
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
