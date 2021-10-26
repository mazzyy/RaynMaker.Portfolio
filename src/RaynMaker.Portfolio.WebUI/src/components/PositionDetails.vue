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
            <td>{{ data.buyingPrice }} {{currency}} / {{ data.buyingValue }} {{currency}}</td>
          </tr>
          <tr>
            <th>Price / Value</th>
            <td>{{ data.currentPrice }} {{currency}} / {{ data.currentValue }} {{currency}}</td>
          </tr>
          <tr>
            <th>Profit</th>
            <td>{{ data.totalProfit }} {{currency}} ({{ data.totalRoi }} %)</td>
          </tr>
          <tr>
            <th>Dividends</th>
            <td>{{ data.totalDividends}} {{currency}} ({{ data.dividendsRoi }} %)</td>
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
          <template #price="{item}">
            <td>{{ item.price }} {{currency}}</td>
          </template>
          <template #value="{item}">
            <td>{{ item.value }} {{currency}}</td>
          </template>
        </CDataTable>
      </CCardBody>
    </CCard>

    <CCard>
      <CCardHeader>
        <CCardTitle>Dividends</CCardTitle>
      </CCardHeader>
      <CCardBody>
        <CDataTable :items="data.dividends"
                    column-filter
                    :responsive="false"
                    :items-per-page="250"
                    hover>
          <template #value="{item}">
            <td>{{ item.value }} {{currency}}</td>
          </template>
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
      isin () { return this.$route.params.isin },
      currency () { return this.data.currency }
    },

    created () {
      this.get(this, `/api/positionDetails?isin=${this.isin}`, { }, function (that, response) {
        that.data = response
        console.log(that.data)
      })
    }
  }
</script>
