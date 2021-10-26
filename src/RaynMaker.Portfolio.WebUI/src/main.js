import Vue from 'vue'
import Router from 'vue-router'
import App from './App'
import CoreuiVue from '@coreui/vue'

require('./assets/js/site.js')

Vue.use(CoreuiVue)
Vue.config.productionTip = false

Vue.use(Router)

const router = new Router({
  routes: [
    { path: '/Positions', alias: '/', component: () => import('@/components/Positions') },
    { path: '/Performance', component: () => import('@/components/Performance') },
    { path: '/Cashflow', component: () => import('@/components/Cashflow') },
    { path: '/ClosedPositions', component: () => import('@/components/ClosedPositions') },
    {
      path: '/PositionDetails',
      name: 'PositionDetails',
      component: () => import('@/components/PositionDetails')
    }
  ],
  linkActiveClass: 'active',
  linkExactActiveClass: 'active'
})

new Vue({
  router,
  render: h => h(App)
}).$mount('#app')
