<template>
  <div>
    <CCard>
      <CCardHeader>
        <CCardTitle>Closed Positions</CCardTitle>
      </CCardHeader>
      <CCardBody>
        <form id="filter">
          <CLabel>Filter: </CLabel>
          <input name="query" v-model="filter">
        </form>

        <closed-positions-grid :data="positions" :filter-key="filter" style="margin-top:10px"></closed-positions-grid>
      </CCardBody>
    </CCard>
  </div>
</template>

<script>
  import * as my from '../assets/js/site.js'
  import ClosedPositionsGrid from '@/components/ClosedPositionsGrid'

  export default {
    name: 'ClosedPositions',
    data () {
      return {
        positions: [],
        filter: ''
      }
    },
    components: {
      'closed-positions-grid': ClosedPositionsGrid
    },
    created: function () {
      this.get(this, '/api/closedPositions', {}, function (that, response) {
        that.positions = response
      })
    },
    mixins: [my.webApi]
  }
</script>
