<template>
  <table class="table table-bordered table-sm">
    <thead>
      <tr>
        <th @click="sortBy('name')" :class="{ sortedBy: sortKey == 'name' }">
          Name
          <span class="arrow" :class="sortOrders['name'] > 0 ? 'asc' : 'dsc'"></span>
        </th>
        <th colspan="3"></th>
        <th>Buying Price</th>
        <th>Price</th>
        <th @click="sortBy('marketProfit')" :class="{ sortedBy: sortKey == 'marketProfit' }">
          Profit
          <span class="arrow" :class="sortOrders['marketProfit'] > 0 ? 'asc' : 'dsc'"></span>
        </th>
        <th @click="sortBy('dividendProfit')" :class="{ sortedBy: sortKey == 'dividendProfit' }">
          Dividend
          <span class="arrow" :class="sortOrders['dividendProfit'] > 0 ? 'asc' : 'dsc'"></span>
        </th>
        <th @click="sortBy('totalProfit')" :class="{ sortedBy: sortKey == 'totalProfit' }">
          Total
          <span class="arrow" :class="sortOrders['totalProfit'] > 0 ? 'asc' : 'dsc'"></span>
        </th>
        <th @click="sortBy('marketProfitAnnual')" :class="{ sortedBy: sortKey == 'marketProfitAnnual' }">
          Profit Annual
          <span class="arrow" :class="sortOrders['marketProfitAnnual'] > 0 ? 'asc' : 'dsc'"></span>
        </th>
        <th @click="sortBy('dividendProfitAnnual')" :class="{ sortedBy: sortKey == 'dividendProfitAnnual' }">
          Dividend Annual
          <span class="arrow" :class="sortOrders['dividendProfitAnnual'] > 0 ? 'asc' : 'dsc'"></span>
        </th>
        <th @click="sortBy('totalProfitAnnual')" :class="{ sortedBy: sortKey == 'totalProfitAnnual' }">
          Total Annual
          <span class="arrow" :class="sortOrders['totalProfitAnnual'] > 0 ? 'asc' : 'dsc'"></span>
        </th>
      </tr>
      <tr>
        <th>Isin</th>
        <th>Shares</th>
        <th>Duration</th>
        <th>Priced At</th>
        <th>Buying Value</th>
        <th>Value</th>
        <th @click="sortBy('marketRoi')" :class="{ sortedBy: sortKey == 'marketRoi' }">
          %
          <span class="arrow" :class="sortOrders['marketRoi'] > 0 ? 'asc' : 'dsc'"></span>
        </th>
        <th @click="sortBy('dividendRoi')" :class="{ sortedBy: sortKey == 'dividendRoi' }">
          %
          <span class="arrow" :class="sortOrders['dividendRoi'] > 0 ? 'asc' : 'dsc'"></span>
        </th>
        <th @click="sortBy('totalRoi')" :class="{ sortedBy: sortKey == 'totalRoi' }">
          %
          <span class="arrow" :class="sortOrders['totalRoi'] > 0 ? 'asc' : 'dsc'"></span>
        </th>
        <th @click="sortBy('marketRoiAnnual')" :class="{ sortedBy: sortKey == 'marketRoiAnnual' }">
          %
          <span class="arrow" :class="sortOrders['marketRoiAnnual'] > 0 ? 'asc' : 'dsc'"></span>
        </th>
        <th @click="sortBy('dividendRoiAnnual')" :class="{ sortedBy: sortKey == 'dividendRoiAnnual' }">
          %
          <span class="arrow" :class="sortOrders['dividendRoiAnnual'] > 0 ? 'asc' : 'dsc'"></span>
        </th>
        <th @click="sortBy('totalRoiAnnual')" :class="{ sortedBy: sortKey == 'totalRoiAnnual' }">
          %
          <span class="arrow" :class="sortOrders['totalRoiAnnual'] > 0 ? 'asc' : 'dsc'"></span>
        </th>
      </tr>
    </thead>
    <tbody>
      <tr v-for="entry in filteredData" v-bind:key="entry.isin">
        <td>
          <router-link :to="{ path: '/PositionDetails', params: { isin: entry.isin } }">{{ entry.name }}</router-link> <br />
          {{ entry.isin }}
        </td>
        <td>{{ entry.shares }}</td>
        <td>{{ entry.duration }}</td>
        <td>{{ entry.pricedAt }}</td>
        <td>
          {{ entry.buyingPrice }} <br />
          {{ entry.buyingValue }}
        </td>
        <td>
          {{ entry.currentPrice }} <br />
          {{ entry.currentValue }}
        </td>
        <td>
          {{ entry.marketProfit }} <br />
          {{ entry.marketRoi  }}
        </td>
        <td>
          {{ entry.dividendProfit }} <br />
          {{ entry.dividendRoi }}
        </td>
        <td :class="entry.totalProfit > 0 ? 'win' : 'loss'">
          <b>
            {{ entry.totalProfit }} <br />
            {{ entry.totalRoi }}
          </b>
        </td>
        <td>
          {{ entry.marketProfitAnnual }} <br />
          {{ entry.marketRoiAnnual }}
        </td>
        <td>
          {{ entry.dividendProfitAnnual }} <br />
          {{ entry.dividendRoiAnnual }}
        </td>
        <td :class="entry.totalProfitAnnual > 0 ? 'win' : 'loss'">
          <b>
            {{ entry.totalProfitAnnual }} <br />
            {{ entry.totalRoiAnnual }}
          </b>
        </td>
      </tr>
    </tbody>
  </table>
</template>

<script>
  export default {
    name: 'positions-grid',
    props: {
      data: Array,
      filterKey: String
    },
    data () {
      const sortOrders = {}
      sortOrders.name = 1
      sortOrders.marketProfit = 1
      sortOrders.dividendProfit = 1
      sortOrders.totalProfit = 1
      sortOrders.marketRoi = 1
      sortOrders.dividendRoi = 1
      sortOrders.totalRoi = 1
      sortOrders.marketProfitAnnual = 1
      sortOrders.dividendProfitAnnual = 1
      sortOrders.totalProfitAnnual = 1
      sortOrders.marketRoiAnnual = 1
      sortOrders.dividendRoiAnnual = 1
      sortOrders.totalRoiAnnual = 1
      return {
        sortKey: 'name',
        sortOrders: sortOrders
      }
    },
    computed: {
      filteredData: function () {
        const sortKey = this.sortKey
        const filterKey = this.filterKey && this.filterKey.toLowerCase()
        const order = this.sortOrders[sortKey] || 1
        let data = this.data
        if (filterKey) {
          data = data.filter(function (row) {
            return Object.keys(row).some(function (key) {
              return String(row[key]).toLowerCase().indexOf(filterKey) > -1
            })
          })
        }
        if (sortKey) {
          data = data.slice().sort(function (a, b) {
            a = a[sortKey]
            b = b[sortKey]
            return (a === b ? 0 : a > b ? 1 : -1) * order
          })
        }
        return data
      }
    },
    methods: {
      sortBy: function (key) {
        this.sortKey = key
        this.sortOrders[key] = this.sortOrders[key] * -1
      }
    }
  }
</script>
