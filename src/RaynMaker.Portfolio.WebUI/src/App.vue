<template>
  <CWrapper>
    <CHeader fixed with-subheader light>
      <CHeaderBrand style="padding:5px" to="/">
        RaynMaker
      </CHeaderBrand>
      <CHeaderNav class="d-md-down-none mr-auto">
        <CNavItem>
          <router-link to="/Positions">Positions</router-link>
        </CNavItem>
          <CNavItem>
            <router-link class="nav-link" to="/Performance">Performance</router-link>
          </CNavItem>
          <CNavItem>
            <router-link class="nav-link" to="/Cashflow">Cashflow</router-link>
          </CNavItem>
          <CNavItem>
            <router-link class="nav-link" to="/ClosedPositions">History</router-link>
          </CNavItem>
      </CHeaderNav>
    </CHeader>

    <div class="c-body">
      <main class="c-main">
        <CContainer fluid>
          <transition name="fade" mode="out-in">
            <router-view :key="$route.path"></router-view>
          </transition>
        </CContainer>
      </main>
    </div>

  </CWrapper>
</template>

<script>
  export default {
    name: 'app'
  }
</script>

<style lang="scss">
  @import "~@coreui/coreui/scss/coreui";
  @import "../node_modules/bootstrap/scss/bootstrap";
  @import './assets/css/site.css';
</style>
