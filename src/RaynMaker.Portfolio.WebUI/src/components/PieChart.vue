
<script>
  import { Pie } from 'vue-chartjs'
  import * as pl from '../../packages/GooglePaletteJs/palette.js'

  export default {
    extends: Pie,
    name: 'pie-chart',
    props: {
      data: Array,
      labels: Array
    },
    mounted () {
      this.render()
    },
    computed: {
      chartData () {
        return this.data
      }
    },
    watch: {
      data () {
        this.render()
      }
    },
    methods: {
      render () {
        let backgrounds = []
        if (this.chartData) {
          backgrounds = pl.palette('tol', Math.min(this.data.length, 12)).map(function (hex) {
            return '#' + hex
          })
        }

        this.renderChart({
          labels: this.labels,
          datasets: [{
            data: this.chartData,
            backgroundColor: backgrounds
          }]
        }, { responsive: false, maintainAspectRatio: false })
      }
    }
  }
</script>
