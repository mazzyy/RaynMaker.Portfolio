<template>
    <div>
        <h1>Performance</h1>

        <table>
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

        <h1>Benchmark</h1>
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
    </div>
</template>

<script>
    import * as my from '../assets/js/site.js'

    export default {
        name: 'Performance',
        data () {
            return {
                performance: null,
                benchmark: null
            }
        },
        created: function () {
            this.get(this, '/api/performance', {}, function (that, response) {
                that.performance = response
            })
            this.get(this, '/api/benchmark', {}, function (that, response) {
                that.benchmark = response
            })
        },
        mixins: [my.webApi]
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
