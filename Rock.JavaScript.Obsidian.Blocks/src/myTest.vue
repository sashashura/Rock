<template>
    <Alert alertType="test" />
    <ATest :myValue="intValue" />
    <ViewPanel :options="intValue" />
</template>

<script lang="ts">
import { defineComponent, ref } from "vue";
import Alert from "@Obsidian/Controls/alert";
import ATest from "@Obsidian/Controls/aTest.vue";
import ViewPanel from "./Core/CampusDetail/viewPanel.partial";

export default defineComponent({
    name: "myTest",

    components: { ATest, Alert, ViewPanel },

    setup() {
        /** This is a comment on a variable. */
        const intValue = ref(23);

        return {
            intValue
        };
    }
})
</script>
