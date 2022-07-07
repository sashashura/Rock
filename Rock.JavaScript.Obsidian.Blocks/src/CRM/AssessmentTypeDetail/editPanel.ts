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
import CheckBox from "@Obsidian/Controls/checkBox";
import TextBox from "@Obsidian/Controls/textBox";
import { propertyRef, updateRefValue } from "@Obsidian/Utility/component";
import { AssessmentTypeBag } from "@Obsidian/ViewModels/Blocks/CRM/AssessmentTypeDetail/assessmentTypeBag";
import { AssessmentTypeDetailOptionsBag } from "@Obsidian/ViewModels/Blocks/CRM/AssessmentTypeDetail/assessmentTypeDetailOptionsBag";

export default defineComponent({
    name: "CRM.AssessmentTypeDetail.EditPanel",

    props: {
        modelValue: {
            type: Object as PropType<AssessmentTypeBag>,
            required: true
        },

        options: {
            type: Object as PropType<AssessmentTypeDetailOptionsBag>,
            required: true
        }
    },

    components: {
        CheckBox,
        TextBox
    },

    emits: {
        "update:modelValue": (_value: AssessmentTypeBag) => true
    },

    setup(props, { emit }) {
        // #region Values

        const description = propertyRef(props.modelValue.description ?? "", "Description");
        const isActive = propertyRef(props.modelValue.isActive ?? false, "IsActive");

        // The properties that are being edited. This should only contain
        // objects returned by propertyRef().
        const propRefs = [description, isActive];

        // #endregion

        // #region Computed Values

        // #endregion

        // #region Functions

        // #endregion

        // #region Event Handlers

        // #endregion

        // Watch for parental changes in our model value and update all our values.
        watch(() => props.modelValue, () => {
            updateRefValue(description, props.modelValue.description ?? "");
            updateRefValue(isActive, props.modelValue.isActive ?? false);
        });

        // Determines which values we want to track changes on (defined in the
        // array) and then emit a new object defined as newValue.
        watch([...propRefs], () => {
            const newValue: AssessmentTypeBag = {
                ...props.modelValue,
                description: description.value,
                isActive: isActive.value,
            };

            emit("update:modelValue", newValue);
        });

        return {
            description,
            isActive,
        };
    },

    template: `
<fieldset>
    <div class="row">

        <div class="col-md-6">
            <CheckBox v-model="isActive"
                label="Active" />
        </div>
    </div>

    <TextBox v-model="description"
        label="Description"
        textMode="multiline" />
</fieldset>
`
});
