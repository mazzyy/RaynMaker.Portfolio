<template>
  <div>
    <h1>Positions</h1>

    <form id="filter">
      <label>Filter: </label>
      <input name="query" v-model="filter">
    </form>

    <positions-grid :data="positions" :columns="properties" :filter-key="filter" :column-headers="headers" style="margin-top:10px"></positions-grid>

    <h2>Diversification</h2>
    <p>
      Based on share count and last price
    </p>
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
        properties: ['pricedAt', 'duration', 'marketProfit', 'dividendProfit', 'totalProfit',
          'marketRoi', 'dividendRoi', 'totalRoi', 'marketRoiAnual', 'dividendRoiAnual', 'totalRoiAnual'],
        headers: ['Priced At', 'Duration', 'Market', 'Dividend', 'Total',
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
      this.get(this, '/api/positions', {}, function (that, response) {
        that.positions = response
      })
      this.get(this, '/api/diversification', {}, function (that, response) {
        that.diversification.data = response.data
        that.diversification.labels = response.labels
      })
    },
    filters: {
      formatValue: my.formatValue
    },
    mixins: [ my.webApi ]
  }
</script>

