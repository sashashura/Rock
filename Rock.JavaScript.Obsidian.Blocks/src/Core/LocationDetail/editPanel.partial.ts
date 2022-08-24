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
import CheckBox from "@Obsidian/Controls/checkBox";
import TextBox from "@Obsidian/Controls/textBox";
import { watchPropertyChanges } from "@Obsidian/Utility/block";
import { propertyRef, updateRefValue } from "@Obsidian/Utility/component";
import { LocationBag } from "@Obsidian/ViewModels/Blocks/Core/LocationDetail/locationBag";
import { LocationDetailOptionsBag } from "@Obsidian/ViewModels/Blocks/Core/LocationDetail/locationDetailOptionsBag";

export default defineComponent({
    name: "Core.LocationDetail.EditPanel",

    props: {
        modelValue: {
            type: Object as PropType<LocationBag>,
            required: true
        },

        options: {
            type: Object as PropType<LocationDetailOptionsBag>,
            required: true
        }
    },

    components: {
        AttributeValuesContainer,
        CheckBox,
        TextBox
    },

    emits: {
        "update:modelValue": (_value: LocationBag) => true,
        "propertyChanged": (_value: string) => true
    },

    setup(props, { emit }) {
        // #region Values

        const attributes = ref(props.modelValue.attributes ?? {});
        const attributeValues = ref(props.modelValue.attributeValues ?? {});
        const isActive = propertyRef(props.modelValue.isActive ?? false, "IsActive");
        const name = propertyRef(props.modelValue.name ?? "", "Name");

        // The properties that are being edited. This should only contain
        // objects returned by propertyRef().
        const propRefs = [isActive, name];

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
            updateRefValue(isActive, props.modelValue.isActive ?? false);
            updateRefValue(name, props.modelValue.name ?? "");
        });

        // Determines which values we want to track changes on (defined in the
        // array) and then emit a new object defined as newValue.
        watch([attributeValues, ...propRefs], () => {
            const newValue: LocationBag = {
                ...props.modelValue,
                attributeValues: attributeValues.value,
                isActive: isActive.value,
                name: name.value
            };

            emit("update:modelValue", newValue);
        });

        // Watch for any changes to props that represent properties and then
        // automatically emit which property changed.
        watchPropertyChanges(propRefs, emit);

        return {
            attributes,
            attributeValues,
            isActive,
            name
        };
    },

    template: `
<fieldset>
    <div class="row">
        <div class="col-md-6">
            <TextBox v-model="name"
                label="Name"
                rules="required" />
        </div>

        <div class="col-md-6">
            <CheckBox v-model="isActive"
                label="Active" />
        </div>
    </div>

    <AttributeValuesContainer v-model="attributeValues" :attributes="attributes" isEditMode :numberOfColumns="2" />
</fieldset>
`
});
