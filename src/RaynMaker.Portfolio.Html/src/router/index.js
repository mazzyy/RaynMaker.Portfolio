import Vue from 'vue'
import Router from 'vue-router'
// import HelloWorld from '@/components/HelloWorld'

Vue.use(Router)

const Positions = Vue.extend({
  template: '<p>{{firstName}} {{lastName}} aka {{alias}}</p>',
  data: function () {
    return {
      firstName: 'Walter',
      lastName: 'White',
      alias: 'Heisenberg'
    }
  }
})

const Performance = { template: '<div>bar</div>' }

// https://router.vuejs.org/en/advanced/data-fetching.html
export default new Router({
  routes: [
    { path: '/Positions', alias: '/', component: Positions },
    { path: '/Performance', component: Performance }
  ],
  linkActiveClass: 'active',
  linkExactActiveClass: 'active'
})
