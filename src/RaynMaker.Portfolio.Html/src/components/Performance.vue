<template>
  <div>
    <h1>Performance</h1>
    <p>
      <label>Total investment: </label> {{ performance.totalInvestment | formatValue }}
    </p>
    <p>
      <label>Total profit: </label> {{ performance.totalProfit | formatValue }}
    </p>

    <h1>Benchmark</h1>
    <p>
      If you would have bought the benchmark "{{ benchmark.name }}" (Isin: {{ benchmark.isin }}) instead of your stock picks you would have gained:
    </p>
    <p>
      <label>Total profit: </label> {{ benchmark.buyInstead.totalProfit | formatValue }}
    </p>
    <p>
      <label>Total ROI: </label> {{ benchmark.buyInstead.totalRoi | formatValue }}
    </p>
    <p>
      <label>Anual ROI: </label> {{ benchmark.buyInstead.totalRoiAnual | formatValue }}
    </p>

    <p>
      If you would have bought the benchmark "{{ benchmark.name }}" (Isin: {{ benchmark.isin }}) every month with a rate of {{ benchmark.buyPlan.rate }} yout would have gained:
    </p>
    <p>
      <label>Total profit: </label> {{ benchmark.buyPlan.totalProfit | formatValue }}
    </p>
    <p>
      <label>Total ROI: </label> {{ benchmark.buyPlan.totalRoi | formatValue }}
    </p>
    <p>
      <label>Anual ROI: </label> {{ benchmark.buyPlan.totalRoiAnual | formatValue }}
    </p>
  </div>
</template>

<script>
  import * as my from '../assets/js/site.js'

  export default {
    name: 'Performance',
    data () {
      return {
        performance: null,
        benchmark: null
      }
    },
    created: function () {
      this.get(this, '/api/performance', {}, function (that, response) {
        that.performance = response
      })
      this.get(this, '/api/benchmark', {}, function (that, response) {
        that.benchmark = response
      })
    },
    filters: {
      formatValue: my.formatValue
    },
    mixins: [my.webApi]
  }
</script>

