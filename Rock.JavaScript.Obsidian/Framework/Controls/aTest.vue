<template>
    <div>{{ myValue }}</div>
</template>

<script lang="ts">
    import { defineComponent, PropType } from "vue";

    export default defineComponent({
        name: "aTest",

        props: {
            /** This is a comment. */
            myValue: {
                type: String as PropType<string>,
                required: false
            }
        },

        setup() {
            return {
                myValue: "hello"
            };
        }
    });
</script>
