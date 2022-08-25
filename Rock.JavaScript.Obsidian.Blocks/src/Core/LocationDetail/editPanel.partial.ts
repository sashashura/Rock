/// <reference path="../../../../Rock.JavaScript.Obsidian/Framework/Controls/numberBox.ts" />
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

import { computed, defineComponent, PropType, ref, watch } from "vue";
import AttributeValuesContainer from "@Obsidian/Controls/attributeValuesContainer";
import AddressControl from "@Obsidian/Controls/addressControl";
import CheckBox from "@Obsidian/Controls/checkBox";
import DefinedValuePicker from "@Obsidian/Controls/definedValuePicker";
import DropDownList from "@Obsidian/Controls/dropDownList";
import ImageUploader from "@Obsidian/Controls/imageUploader";
import NumberBox from "@Obsidian/Controls/numberBox";
import TextBox from "@Obsidian/Controls/textBox";
import { watchPropertyChanges } from "@Obsidian/Utility/block";
import { propertyRef, updateRefValue } from "@Obsidian/Utility/component";
import { LocationBag } from "@Obsidian/ViewModels/Blocks/Core/LocationDetail/locationBag";
import { LocationDetailOptionsBag } from "@Obsidian/ViewModels/Blocks/Core/LocationDetail/locationDetailOptionsBag";
import { DefinedType } from "../../../../Rock.JavaScript.Obsidian/Framework/SystemGuids";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

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
        AddressControl,
        AttributeValuesContainer,
        CheckBox,
        DefinedValuePicker,
        DropDownList,
        ImageUploader,
        NumberBox,
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
        const parentLocation = propertyRef(props.modelValue.parentLocation, "ParentLocation");
        const isActive = propertyRef(props.modelValue.isActive ?? false, "IsActive");
        const name = propertyRef(props.modelValue.name ?? "", "Name");
        const locationTypeValue = propertyRef(props.modelValue.locationTypeValue, "LocationTypeValue");
        const printerDeviceId = propertyRef(props.modelValue.printerDeviceId, "PrinterDeviceId");
        const isGeoPointLocked = propertyRef(props.modelValue.isGeoPointLocked ?? false, "IsGeoPointLocked");
        const softRoomThreshold = propertyRef(props.modelValue.softRoomThreshold, "SoftRoomThreshold");
        const firmRoomThreshold = propertyRef(props.modelValue.firmRoomThreshold, "FirmRoomThreshold");

        // The properties that are being edited. This should only contain
        // objects returned by propertyRef().
        const propRefs = [isActive, name, parentLocation, locationTypeValue, printerDeviceId, isGeoPointLocked, softRoomThreshold, firmRoomThreshold];

        // #endregion

        // #region Computed Values

        const printerDeviceOptions = computed((): ListItemBag[] => {
            return props.options.printerDeviceOptions ?? [];
        });

        // #endregion

        // #region Functions

        // #endregion

        // #region Event Handlers

        // #endregion

        // Watch for parental changes in our model value and update all our values.
        watch(() => props.modelValue, () => {
            updateRefValue(attributes, props.modelValue.attributes ?? {});
            updateRefValue(attributeValues, props.modelValue.attributeValues ?? {});
            updateRefValue(parentLocation, props.modelValue.parentLocation);
            updateRefValue(isActive, props.modelValue.isActive ?? false);
            updateRefValue(name, props.modelValue.name ?? "");
            updateRefValue(locationTypeValue, props.modelValue.locationTypeValue);
            updateRefValue(printerDeviceId, props.modelValue.printerDeviceId);

            updateRefValue(isGeoPointLocked, props.modelValue.isGeoPointLocked ?? false);
            updateRefValue(softRoomThreshold, props.modelValue.softRoomThreshold);
            updateRefValue(firmRoomThreshold, props.modelValue.firmRoomThreshold);
        });

        // Determines which values we want to track changes on (defined in the
        // array) and then emit a new object defined as newValue.
        watch([attributeValues, ...propRefs], () => {
            const newValue: LocationBag = {
                ...props.modelValue,
                attributeValues: attributeValues.value,
                isActive: isActive.value,
                name: name.value,
                locationTypeValue: locationTypeValue.value,
                parentLocation: parentLocation.value,
                printerDeviceId: printerDeviceId.value,
                isGeoPointLocked: isGeoPointLocked.value,
                softRoomThreshold: softRoomThreshold.value,
                firmRoomThreshold: firmRoomThreshold.value
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
            name,
            locationTypeValue,
            locationTypeDefinedTypeGuid: DefinedType.LocationType,
            parentLocation,
            printerDeviceId,
            isGeoPointLocked,
            softRoomThreshold,
            firmRoomThreshold,
            printerDeviceOptions
        };
    },

    template: `
<fieldset>
    <div class="row">
        <div class="col-md-6">

            <LocationPicker v-model="location"
                label="Parent Location"
                rules="" />

            <TextBox v-model="name"
                label="Name"
                rules="required" />

            <DefinedValuePicker v-model="locationTypeValue"
                label="Location Type"
                :definedTypeGuid="locationTypeDefinedTypeGuid" />

            <DropDownList v-model="printerDeviceId"
                label="Printer"
                help="The printer that this location should use for printing."
                :items="printerDeviceOptions" />
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
