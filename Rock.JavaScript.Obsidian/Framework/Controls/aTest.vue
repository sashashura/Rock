<template>
    <div>{{ myValue }}</div>
</template>

<script lang="ts">
    import { defineComponent } from "vue";

    export default defineComponent({
        name: "aTest",

        setup() {
            return {
                myValue: "hello"
            };
        }
    });
</script>
