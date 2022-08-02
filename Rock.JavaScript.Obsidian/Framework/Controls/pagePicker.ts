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

import { computed, defineComponent, Prop, PropType, ref, toRefs, watch } from "vue";
import { PageTreeItemProvider } from "@Obsidian/Utility/treeItemProviders";
import { updateRefValue } from "@Obsidian/Utility/component";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import TreeItemPicker from "./treeItemPicker";
import RockButton, { BtnSize, BtnType } from "./rockButton";
import { useStore } from "@Obsidian/PageState";
import { Guid } from "@Obsidian/Types";
import { post } from "@Obsidian/Utility/http";
import { PagePickerGetPageNameOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/pagePickerGetPageNameOptionsBag";

export default defineComponent({
    name: "PagePicker",

    components: {
        TreeItemPicker,
        RockButton
    },

    props: {
        modelValue: {
            type: Object as PropType<ListItemBag | ListItemBag[] | null>,
            required: false
        },

        securityGrantToken: {
            type: String as PropType<string | null>,
            required: false
        },

        showSelectCurrentPage: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        hidePageGuids: {
            type: Array as PropType<Guid[]>,
            required: false
        },

        someVal: {
            type: Number as PropType<number>
        }
    },

    emits: {
        "update:modelValue": (_value: ListItemBag | ListItemBag[] | null) => true
    },

    setup(props, { emit }) {
        const internalValue = ref(props.modelValue ?? null);

        // Configure the item provider with our settings. These are not reactive
        // since we don't do lazy loading so there is no point.
        const itemProvider = ref(new PageTreeItemProvider());
        itemProvider.value.securityGrantToken = props.securityGrantToken;
        itemProvider.value.hidePageGuids = props.hidePageGuids;

        // TODO: deal with array of values
        if (internalValue.value && !Array.isArray(internalValue.value)) {
            itemProvider.value.selectedPageGuid = internalValue.value.value;
        }

        watch(internalValue, () => {
            emit("update:modelValue", internalValue.value);
        });

        watch(() => props.modelValue, () => {
            console.debug("model updated", updateRefValue(internalValue, props.modelValue ?? null));
        });

        function onValueSelected(): void {
            console.log("value selected");
            // TODO: unset route
        }

        const pageStore = useStore();
        const pageGuid = computed(() => pageStore.state.pageGuid);
        let currentPage;

        async function selectCurrentPage(): Promise<void> {
            if (currentPage) {
                updateRefValue(internalValue, currentPage);

                return;
            }

            const options: PagePickerGetPageNameOptionsBag = { pageGuid: pageGuid.value };
            const response = await post<string>("/api/v2/Controls/PagePickerGetPageName", {}, options);

            if (response.isSuccess && response.data) {
                currentPage = {
                    text: response.data,
                    value: pageGuid.value
                };
                updateRefValue(internalValue, currentPage);
            }
            else {
                console.error("Error", response.errorMessage);
                updateRefValue(internalValue, { value: pageGuid.value });
            }
        }

        function refreshProvider() {
            const prov = new PageTreeItemProvider();
            prov.securityGrantToken = props.securityGrantToken;
            prov.hidePageGuids = props.hidePageGuids;

            itemProvider.value = prov;
        }

        return {
            internalValue,
            itemProvider,
            pageGuid,
            btnSize: BtnSize.ExtraSmall,
            btnType: BtnType.Link,
            selectCurrentPage,
            onValueSelected
        };
    },

    template: `
<TreeItemPicker v-model="internalValue"
    formGroupClasses="location-item-picker"
    iconCssClass="fa fa-file"
    :provider="itemProvider"
    :multiple="multiple"
    @valueSelected="onValueSelected"
>
    <template #customPickerActions>
        <RockButton @click="selectCurrentPage" :btnSize="btnSize" :btnType="btnType" title="Select Current Page"><i class="fa fa-file-o"></i></RockButton>
    </template>
</TreeItemPicker>
`
});
