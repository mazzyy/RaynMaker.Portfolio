function init() {
    var app = new Vue({
        el: '#app',
        data: {
            events: []
        },

        created: function () {
            this.fetchData();
        },

        methods: {
            fetchData: function () {
                var data = {
                    lastNth: '25'
                };
                var that = this
                $.ajax({
                    url: '/api/transactions',
                    data: data,
                    dataType: 'json',
                    method: 'GET'
                }).then(function (response) {
                    if (response.error) {
                        console.err("There was an error " + response.error);
                    } else {
                        that.events = response;
                    }
                }).catch(function (err) {
                    console.error(err);
                });
            }
        }
    })
}
