import Vue from 'vue'
import Router from 'vue-router'
import App from './App'

Vue.config.productionTip = false

Vue.use(Router)

const router = new Router({
    routes: [
        { path: '/Positions', alias: '/', component: () => import('@/components/Positions') },
        { path: '/Performance', component: () => import('@/components/Performance') },
        { path: '/Cashflow', component: () => import('@/components/Cashflow') },
        { path: '/ClosedPositions', component: () => import('@/components/ClosedPositions') }
    ],
    linkActiveClass: 'active',
    linkExactActiveClass: 'active'
})

new Vue({
  router,
  render: h => h(App)
}).$mount('#app')

require('../node_modules/bootstrap/dist/css/bootstrap.min.css')
require('./assets/css/site.css')
require('./assets/js/site.js')
