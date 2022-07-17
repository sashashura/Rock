// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

import { defineComponent, PropType, ref, watch } from "vue";
import AttributeValuesContainer from "@Obsidian/Controls/attributeValuesContainer";
import PersonPicker from "@Obsidian/Controls/personPicker";
import CheckBox from "@Obsidian/Controls/checkBox";
import TextBox from "@Obsidian/Controls/textBox";
import { watchPropertyChanges } from "@Obsidian/Utility/block";
import { propertyRef, updateRefValue } from "@Obsidian/Utility/component";
import { PageRouteBag } from "@Obsidian/ViewModels/Blocks/CMS/PageRouteDetail/pageRouteBag";
import { PageRouteDetailOptionsBag } from "@Obsidian/ViewModels/Blocks/CMS/PageRouteDetail/pageRouteDetailOptionsBag";

export default defineComponent({
    name: "CMS.PageRouteDetail.EditPanel",

    props: {
        modelValue: {
            type: Object as PropType<PageRouteBag>,
            required: true
        },

        options: {
            type: Object as PropType<PageRouteDetailOptionsBag>,
            required: true
        }
    },

    components: {
        AttributeValuesContainer,
        PersonPicker,
        CheckBox,
        TextBox
    },

    emits: {
        "update:modelValue": (_value: PageRouteBag) => true,
        "propertyChanged": (_value: string) => true
    },

    setup(props, { emit }) {
        // #region Values

        const attributes = ref(props.modelValue.attributes ?? {});
        const attributeValues = ref(props.modelValue.attributeValues ?? {});
        const isSystem = ref(props.modelValue.isSystem ?? {});
        const route = propertyRef(props.modelValue.route ?? "", "Route");
        const isGlobal = propertyRef(props.modelValue.isGlobal ?? false, "IsGlobal");
        const site = ref(props.modelValue.site ?? "");
        const page = propertyRef(props.modelValue.page ?? {}, "Page");

        // The properties that are being edited. This should only contain
        // objects returned by propertyRef().
        const propRefs = [];

        // #endregion

        // #region Computed Values

        // #endregion

        // #region Functions

        // #endregion

        // #region Event Handlers

        // #endregion

        // Watch for parental changes in our model value and update all our values.
        watch(() => props.modelValue, () => {
            updateRefValue(attributes, props.modelValue.attributes ?? {});
            updateRefValue(attributeValues, props.modelValue.attributeValues ?? {});
        });

        // Determines which values we want to track changes on (defined in the
        // array) and then emit a new object defined as newValue.
        watch([attributeValues, route, isGlobal, page, ...propRefs], () => {
            const newValue: PageRouteBag = {
                ...props.modelValue,
                attributeValues: attributeValues.value,
                route: route.value,
                isGlobal: isGlobal.value,
                page: page.value
            };

            emit("update:modelValue", newValue);
        });

        // Watch for any changes to props that represent properties and then
        // automatically emit which property changed.
        watchPropertyChanges(propRefs, emit);

        return {
            attributes,
            attributeValues,
            isSystem,
            route,
            site,
            isGlobal,
            page
        };
    },

    template: `
<fieldset>
    <AttributeValuesContainer v-model="attributeValues" :attributes="attributes" isEditMode :numberOfColumns="2" />
    <Alert v-if="isSystem" alertType="info">
        <strong>Note</strong> Because this page route is used by Rock, editing is not enabled.
    </Alert>
    <div class="row">
        <div class="col-md-6">
            <PersonPicker label="Page" rules="required" />
            <Label>Site</Label>
            <p>{{site}}</p>
        </div>
        <div class="col-md-6">
            <TextBox v-model="route"
                     label="Route"
                     rules="required"/>
            <CheckBox v-model="isGlobal"
                      label="Global"
                      help="Check this if the page should be used by every site even if 'Enable Exclusive Routes' is turned on."/>
        </div>
    </div>
</fieldset>
`
});
