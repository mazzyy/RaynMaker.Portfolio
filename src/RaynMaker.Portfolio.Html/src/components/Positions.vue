<template>
  <div>
    <h1>Positions</h1>

    <form id="filter">
        Filter <input name="query" v-model="filter">
    </form>

    <positions-grid :data="positions" :columns="properties" :filter-key="filter" :column-headers="headers" style="margin-top:10px"></positions-grid>

    <h2>Diversification</h2>
    <pie-chart :width="500" :height="500" :data="diversification.data" :labels="diversification.labels"></pie-chart>
  </div>
</template>

<script>
  import * as my from '../assets/js/site.js'
  import PositionsGrid from '@/components/PositionsGrid'
  import PieChart from '@/components/PieChart'

  export default {
    name: 'Positions',
    data () {
      return {
        positions: [],
        filter: '',
        properties: ['name', 'isin', 'open', 'close', 'duration', 'marketProfit', 'dividendProfit', 'totalProfit',
          'marketRoi', 'dividendRoi', 'totalRoi', 'marketRoiAnual', 'dividendRoiAnual', 'totalRoiAnual'],
        headers: ['Name', 'Isin', 'Open', 'Close', 'Duration', 'Market', 'Dividend', 'Total',
          'Market', 'Dividend', 'Total', 'Market', 'Dividend', 'Total'],
        diversification: {
          data: null,
          labels: null
        }
      }
    },
    components: {
      'positions-grid': PositionsGrid,
      'pie-chart': PieChart
    },
    created: function () {
      this.get('/api/positions', {}, function (that, response) {
        that.positions = response
      })
      this.get('/api/diversification', {}, function (that, response) {
        that.diversification.data = response.data
        that.diversification.labels = response.labels
      })
    },
    filters: {
      formatValue: my.formatValue
    },
    methods: {
      get: function (url, data, onDone) {
        let baseUrl = process.env.NODE_ENV === 'production' ? '' : 'http://localhost:2525'
        var that = this
        $.ajax({
          url: baseUrl + url,
          data: data,
          dataType: 'json',
          method: 'GET'
        }).then(function (response) {
          if (response.error) {
            console.err('ERROR: ' + response.error)
          } else {
            onDone(that, response)
          }
        }).catch(function (err) {
          console.error('EXCEPTION: ' + err)
        })
      }
    }
  }
</script>

