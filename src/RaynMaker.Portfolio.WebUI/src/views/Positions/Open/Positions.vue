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
  import API from '@/api'
  import PieChart from '@/components/PieChart'
  import PositionsGrid from './PositionsGrid'

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
    async created () {
      let response = await API.get('/positions')
      this.positions = response.data

      response = await API.get('/diversification')
      this.diversification.capital = response.data.capital
      this.diversification.labels = response.data.labels
    }
  }
</script>
