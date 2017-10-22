import Vue from 'vue'
import Router from 'vue-router'
import Positions from '@/components/Positions'
import Performance from '@/components/Performance'

Vue.use(Router)

// https://router.vuejs.org/en/advanced/data-fetching.html
export default new Router({
  routes: [
    { path: '/Positions', alias: '/', component: Positions },
    { path: '/Performance', component: Performance }
  ],
  linkActiveClass: 'active',
  linkExactActiveClass: 'active'
})
