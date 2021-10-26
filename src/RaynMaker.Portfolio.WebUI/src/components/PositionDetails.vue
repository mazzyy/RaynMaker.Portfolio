<template>
  <CCard>
    <CCardHeader>
      <CCardTitle>{{ isin }}</CCardTitle>
    </CCardHeader>
    <CCardBody>
    </CCardBody>
  </CCard>
</template>

<script>
  export default {
    name: 'PositionDetails',

    computed: {
      isin () { return this.$route.params.isin }
    }
  }
</script>
