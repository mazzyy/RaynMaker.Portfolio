
<script>
  import { Pie } from 'vue-chartjs'
  import * as my from '../assets/js/site.js'

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
        var backgrounds = []
        if (this.chartData) {
          this.chartData.forEach(_ => {
            backgrounds.push(my.randomColor())
          })
        }

        this.renderChart({
          labels: this.labels,
          datasets: [{
            backgroundColor: backgrounds,
            data: this.chartData
          }]
        }, { responsive: false, maintainAspectRatio: false })
      }
    }
  }
</script>
