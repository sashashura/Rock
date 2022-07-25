<template>
    <Alert alertType="test" />
    <ATest myValue="test" />
    <ViewPanel options="23" />
</template>

<script lang="ts">
import { defineComponent } from "vue";
import Alert from "@Obsidian/Controls/alert";
import ATest from "../../Rock.JavaScript.Obsidian/Framework/Controls/aTest.vue";
import ViewPanel from "./Core/CampusDetail/viewPanel";

export default defineComponent({
    name: "myTest",

    components: { ATest, Alert, ViewPanel },

    setup() {

    }
})
</script>
