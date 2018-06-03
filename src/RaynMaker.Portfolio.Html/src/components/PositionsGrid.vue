<template>
  <table class="table table-bordered">
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
        <th @click="sortBy('marketProfitAnual')" :class="{ sortedBy: sortKey == 'marketProfitAnual' }">
          Profit Anual
          <span class="arrow" :class="sortOrders['marketProfitAnual'] > 0 ? 'asc' : 'dsc'"></span>
        </th>
        <th @click="sortBy('dividendProfitAnual')" :class="{ sortedBy: sortKey == 'dividendProfitAnual' }">
          Dividend Anual
          <span class="arrow" :class="sortOrders['dividendProfitAnual'] > 0 ? 'asc' : 'dsc'"></span>
        </th>
        <th @click="sortBy('totalProfitAnual')" :class="{ sortedBy: sortKey == 'totalProfitAnual' }">
          Total Anual
          <span class="arrow" :class="sortOrders['totalProfitAnual'] > 0 ? 'asc' : 'dsc'"></span>
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
        <th @click="sortBy('marketRoiAnual')" :class="{ sortedBy: sortKey == 'marketRoiAnual' }">
          %
          <span class="arrow" :class="sortOrders['marketRoiAnual'] > 0 ? 'asc' : 'dsc'"></span>
        </th>
        <th @click="sortBy('dividendRoiAnual')" :class="{ sortedBy: sortKey == 'dividendRoiAnual' }">
          %
          <span class="arrow" :class="sortOrders['dividendRoiAnual'] > 0 ? 'asc' : 'dsc'"></span>
        </th>
        <th @click="sortBy('totalRoiAnual')" :class="{ sortedBy: sortKey == 'totalRoiAnual' }">
          %
          <span class="arrow" :class="sortOrders['totalRoiAnual'] > 0 ? 'asc' : 'dsc'"></span>
        </th>
      </tr>
    </thead>
    <tbody>
      <tr v-for="entry in filteredData" :class="{ closed: entry.isClosed }">
        <td>
          {{ entry.name }} <br />
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
          {{ entry.marketProfit | formatValue }} <br />
          {{ entry.marketRoi | formatValue }}
        </td>
        <td>
          {{ entry.dividendProfit | formatValue }} <br />
          {{ entry.dividendRoi | formatValue }}
        </td>
        <td :class="entry.totalProfit > 0 ? 'win' : 'loss'">
          <b>
            {{ entry.totalProfit | formatValue }} <br />
            {{ entry.totalRoi | formatValue }}
          </b>
        </td>
        <td>
          {{ entry.marketProfitAnual | formatValue }} <br />
          {{ entry.marketRoiAnual | formatValue }}
        </td>
        <td>
          {{ entry.dividendProfitAnual | formatValue }} <br />
          {{ entry.dividendRoiAnual | formatValue }}
        </td>
        <td :class="entry.totalProfitAnual > 0 ? 'win' : 'loss'">
          <b>
            {{ entry.totalProfitAnual | formatValue }} <br />
            {{ entry.totalRoiAnual | formatValue }}
          </b>
        </td>
      </tr>
    </tbody>
  </table>
</template>

<script>
  import * as my from '../assets/js/site.js'

  export default {
    name: 'positions-grid',
    props: {
      data: Array,
      filterKey: String
    },
    data () {
      var sortOrders = {}
      sortOrders['name'] = 1
      sortOrders['marketProfit'] = 1
      sortOrders['dividendProfit'] = 1
      sortOrders['totalProfit'] = 1
      sortOrders['marketRoi'] = 1
      sortOrders['dividendRoi'] = 1
      sortOrders['totalRoi'] = 1
      sortOrders['marketProfitAnual'] = 1
      sortOrders['dividendProfitAnual'] = 1
      sortOrders['totalProfitAnual'] = 1
      sortOrders['marketRoiAnual'] = 1
      sortOrders['dividendRoiAnual'] = 1
      sortOrders['totalRoiAnual'] = 1
      return {
        sortKey: 'name',
        sortOrders: sortOrders
      }
    },
    computed: {
      filteredData: function () {
        var sortKey = this.sortKey
        var filterKey = this.filterKey && this.filterKey.toLowerCase()
        var order = this.sortOrders[sortKey] || 1
        var data = this.data
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
    filters: {
      formatValue: my.formatValue
    },
    methods: {
      sortBy: function (key) {
        this.sortKey = key
        this.sortOrders[key] = this.sortOrders[key] * -1
      }
    }
  }
</script>
