<template>
  <table class="table table-bordered">
    <thead>
      <tr>
        <th colspan="5"></th>
        <th colspan="3" style="text-align:center">Profit</th>
        <th colspan="3" style="text-align:center">ROI (%)</th>
        <th colspan="3" style="text-align:center">Anual (%)</th>
      </tr>
      <tr>
        <th @click="sortBy('name')" :class="{ sortedBy: sortKey == 'name' }">
          Name / Isin
          <span class="arrow" :class="sortOrders['name'] > 0 ? 'asc' : 'dsc'"></span>
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
          {{ entry.name }} <br/>
          {{ entry.isin }}
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
