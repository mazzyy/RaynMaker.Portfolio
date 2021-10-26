<template>
  <div>
    <CCard>
      <CCardHeader>
        <CCardTitle>Positions</CCardTitle>
      </CCardHeader>
      <CCardBody>
        <form id="filter">
          <label style="margin-right:10px">Filter: </label>
          <input name="query" v-model="filter" />
        </form>

        <positions-grid :data="positions" :filter-key="filter" style="margin-top:10px"></positions-grid>
      </CCardBody>
    </CCard>

    <CCard>
      <CCardHeader>
        <CCardTitle>Diversification</CCardTitle>
      </CCardHeader>
      <CCardBody>
        <p>
          Based on share count and last price
        </p>
        <pie-chart :width="500" :height="500" :data="diversification.capital" :labels="diversification.labels"></pie-chart>
      </CCardBody>
    </CCard>
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
        diversification: {
          capital: null,
          labels: null
        }
      }
    },
    components: {
      'positions-grid': PositionsGrid,
      'pie-chart': PieChart
    },
    created () {
      this.get(this, '/api/positions', {}, function (that, response) {
        that.positions = response
      })
      this.get(this, '/api/diversification', {}, function (that, response) {
        that.diversification.capital = response.capital
        that.diversification.labels = response.labels
      })
    },
    mixins: [my.webApi]
  }
</script>
