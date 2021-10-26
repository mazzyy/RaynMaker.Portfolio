<template>
  <div id="app">
    <div class="container-fluid">
      <nav class="navbar navbar-expand-lg navbar-light bg-light">
        <div class="collapse navbar-collapse">
          <ul class="navbar-nav mr-auto">
            <li class="nav-item">
              <router-link class="nav-link" to="/Positions">Home</router-link>
            </li>
            <li class="nav-item">
              <router-link class="nav-link" to="/Performance">Performance</router-link>
            </li>
            <li class="nav-item">
              <router-link class="nav-link" to="/Cashflow">Cashflow</router-link>
            </li>
            <li class="nav-item">
              <router-link class="nav-link" to="/ClosedPositions">History</router-link>
            </li>
          </ul>
        </div>
      </nav>

      <router-view />
    </div>
  </div>
</template>

<script>
  export default {
    name: 'app'
  }
</script>

<style lang="scss">
  @import "../node_modules/bootstrap/scss/bootstrap";
  @import './assets/css/site.css';
</style>
