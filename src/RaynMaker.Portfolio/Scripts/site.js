function init() {
    var app = new Vue({
        el: '#app',
        data: {
            message: 'Vue',
            events: [
                { date: '2017-01-01', name: "stock bought" },
                { date: '2017-3-3', name: "stock sold" }
            ]
        },

        created: function () {
            this.fetchData();
        },

        methods: {
            fetchData: function () {
                var data = {
                    message: 'ping'
                };
                var that = this
                $.ajax({
                    url: '/api/hello',
                    data: data,
                    dataType: 'json',
                    method: 'GET'
                }).then(function (response) {
                    if (response.error) {
                        console.err("There was an error " + response.error);
                        that.message = 'Request failed :(';
                    } else {
                        that.message = response.message;
                    }
                }).catch(function (err) {
                    console.error(err);
                    that.message = "Fatal error :(";
                });
            }
        }
    })
}
