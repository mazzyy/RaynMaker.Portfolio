<template>
  <div>
    <CCard>
      <CCardHeader>
        <CCardTitle>{{ name }}</CCardTitle>
      </CCardHeader>
      <CCardBody>
        <table>
          <tr>
            <th>Isin:</th>
            <td>{{ isin }}</td>
          </tr>
          <tr>
            <th>Shares</th>
            <td>{{ data.shares }}</td>
          </tr>
          <tr>
            <th style="padding-right:20px">Buying Price / Buying Value</th>
            <td>{{ data.buyingPrice }} / {{ data.buyingValue }}</td>
          </tr>
          <tr>
            <th>Price / Value</th>
            <td>{{ data.currentPrice }} / {{ data.currentValue }}</td>
          </tr>
          <tr>
            <th>Profit</th>
            <td>{{ data.totalProfit }} ({{ data.totalRoi }} %)</td>
          </tr>
        </table>
      </CCardBody>
    </CCard>

    <CCard>
      <CCardHeader>
        <CCardTitle>Transactions</CCardTitle>
      </CCardHeader>
      <CCardBody>
        <CDataTable :items="data.transactions"
                    column-filter
                    :responsive="false"
                    :items-per-page="250"
                    hover>
        </CDataTable>
      </CCardBody>
    </CCard>
  </div>
</template>

<script>
  import * as my from '../assets/js/site.js'

  export default {
    name: 'PositionDetails',

    mixins: [my.webApi],

    data () {
      return {
        data: {}
      }
    },

    computed: {
      name () { return this.$route.params.name },
      isin () { return this.$route.params.isin }
    },

    created () {
      this.get(this, `/api/positionDetails?isin=${this.isin}`, { }, function (that, response) {
        that.data = response
        console.log(that.data)
      })
    }
  }
</script>
