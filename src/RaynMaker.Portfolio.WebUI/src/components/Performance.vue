<template>
  <div>
    <CCard>
      <CCardHeader>
        <CCardTitle>Performance</CCardTitle>
      </CCardHeader>
      <CCardBody>
        <table v-if="performance">
          <tr>
            <th class="label">Investment time</th>
            <td class="value">{{ performance.investingTime }}</td>
          </tr>
          <tr>
            <th class="label">Cash limit</th>
            <td class="value">{{ performance.cashLimit }}</td>
          </tr>
          <tr>
            <th class="label">Total deposit</th>
            <td class="value">{{ performance.totalDeposit }}</td>
          </tr>
          <tr>
            <th class="label">Total disbursement</th>
            <td class="value">{{ performance.totalDisbursement }}</td>
          </tr>
          <tr>
            <th class="label">Total investment</th>
            <td class="value">{{ performance.totalInvestment }}</td>
          </tr>
          <tr>
            <th class="label">Total cash</th>
            <td class="value">{{ performance.totalCash }}</td>
          </tr>
          <tr>
            <th class="label">Total dividends</th>
            <td class="value">{{ performance.totalDividends }}</td>
          </tr>
          <tr>
            <th class="label">Current portfolio value</th>
            <td class="value">{{ performance.currentPortfolioValue }}</td>
          </tr>
          <tr>
            <th class="label">Total value</th>
            <td class="value">{{ performance.totalValue }}</td>
          </tr>
          <tr>
            <th class="label">Total profit</th>
            <td class="value">{{ performance.totalProfit }}</td>
          </tr>
        </table>
      </CCardBody>
    </CCard>

    <CCard>
      <CCardHeader>
        <CCardTitle>Benchmark</CCardTitle>
      </CCardHeader>
      <CCardBody v-if="benchmark">
        <p>
          If you would have bought the benchmark "{{ benchmark.name }}" (Isin: {{ benchmark.isin }}) instead of your stock picks you would have gained:
        </p>
        <p>
          <label>Total profit: </label> {{ benchmark.buyInstead.totalProfit }}
        </p>
        <p>
          <label>Total ROI: </label> {{ benchmark.buyInstead.totalRoi }}
        </p>
        <p>
          <label>Annual ROI: </label> {{ benchmark.buyInstead.totalRoiAnnual }}
        </p>

        <p>
          If you would have bought the benchmark "{{ benchmark.name }}" (Isin: {{ benchmark.isin }}) every month with a rate of {{ benchmark.buyPlan.rate }} you would have gained:
        </p>
        <p>
          <label>Total profit: </label> {{ benchmark.buyPlan.totalProfit }}
        </p>
        <p>
          <label>Total ROI: </label> {{ benchmark.buyPlan.totalRoi }}
        </p>
        <p>
          <label>Annual ROI: </label> {{ benchmark.buyPlan.totalRoiAnnual }}
        </p>
      </CCardBody>
    </CCard>
  </div>
</template>

<script>
  import API from '@/api'

  export default {
    name: 'Performance',
    data () {
      return {
        performance: null,
        benchmark: null
      }
    },
    async created () {
      let response = await API.get('/performance')
      this.performance = response.data

      response = await API.get('/benchmark')
      this.benchmark = response.data
    }
  }
</script>

<style scoped>
  .label {
    text-align: left;
    font-weight: bold;
  }

  .value {
    text-align: right;
  }

  th, td {
    padding: 5px;
  }
</style>
