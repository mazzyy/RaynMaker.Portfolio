<template>
  <div class="hello">
    <h1>{{ msg }}</h1>
    <div>
      Is Production: {{ isProduction }}
    </div>
  </div>
</template>

<script>
export default {
  name: 'HelloWorld',
  data () {
    return {
      msg: 'Welcome to Your Vue.js App',
      isProduction: process.env.NODE_ENV === 'production'
    }
  }
}
</script>

