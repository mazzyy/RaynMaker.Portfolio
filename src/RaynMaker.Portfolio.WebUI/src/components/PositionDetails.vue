<template>
  <div>
    <h1>Position Details</h1>

    <h2>{{ isin }}</h2>

  </div>
</template>

<script>
  export default {
    name: 'PositionDetails',

    computed: {
      isin () { return this.$route.params.isin }
    }
  }
</script>
