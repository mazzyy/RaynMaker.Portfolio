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
        <th colspan="3" style="text-align:center">Anual (%)</th>
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
        <th v-for="(key,idx) in columns" @click="sortBy(key)" :class="{ sortedBy: sortKey == key }">
          {{ columnHeaders[idx] }}
          <span class="arrow" :class="sortOrders[key] > 0 ? 'asc' : 'dsc'"></span>
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
          {{ entry.marketProfit }} <br />
          {{ entry.marketRoi }}
        </td>
        <td>
          {{ entry.dividendProfit }} <br />
          {{ entry.dividendRoi }}
        </td>
        <td>
          {{ entry.totalProfit }} <br />
          {{ entry.totalRoi }}
        </td>
        <td v-for="key in columns">
          {{ entry[key] | formatValue }}
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
      columns: Array,
      columnHeaders: Array,
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
      this.columns.forEach(function (key) {
        sortOrders[key] = 1
      })
      return {
        sortKey: '',
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
