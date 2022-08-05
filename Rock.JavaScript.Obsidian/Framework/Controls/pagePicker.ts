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
import { PageRouteValueBag } from "@Obsidian/ViewModels/Rest/Controls/pageRouteValueBag";

export default defineComponent({
    name: "PagePicker",

    components: {
        TreeItemPicker,
        RockButton
    },

    props: {
        modelValue: {
            type: Object as PropType<PageRouteValueBag | PageRouteValueBag[] | null>,
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

        promptForPageRoute: {
            type: Boolean as PropType<boolean>,
            default: false
        },

        multiple: {
            type: Boolean as PropType<boolean>,
            default: false
        },
    },

    emits: {
        "update:modelValue": (_value: PageRouteValueBag | PageRouteValueBag[] | null) => true
    },

    setup(props, { emit }) {
        // Enable route picker only if prop is set to true AND we're only selecting one value
        const shouldPromptForRoute = computed(() => !props.multiple && props.promptForPageRoute);

        // Extract the page value(s) from the the PageRouteValueBag(s) so they can be used with the tree picker
        const internalPageValue = computed<ListItemBag | (ListItemBag | null | undefined)[] | null | undefined>(() => {
            if (!props.modelValue) {
                return null;
            }

            if (Array.isArray(props.modelValue)) {
                if (props.multiple) {
                    return props.modelValue.map(item => item.page);
                }

                return props.modelValue[0].page;
            }

            return props.modelValue.page;
        });

        // Extract the route so it can be controlled by its own picker (if route picking enabled)
        const internalRouteValue = computed<ListItemBag | null | undefined>(() => {
            if (!props.modelValue || Array.isArray(props.modelValue) || !shouldPromptForRoute.value) {
                return null;
            }

            return props.modelValue.route;
        });

        // Initialize to true if
        const isRoutePickerVisible = ref(shouldPromptForRoute.value && !internalRouteValue.value && !!internalPageValue.value);

        function showRoutePicker(): void {
            isRoutePickerVisible.value = shouldPromptForRoute.value;
        }

        function hideRoutePicker(): void {
            isRoutePickerVisible.value = false;
        }

        function updatePage(pages: ListItemBag | ListItemBag[] | null): void {
            if (!pages) {
                emit("update:modelValue", null);
                hideRoutePicker();
                return;
            }

            if (props.multiple) {
                emit("update:modelValue", (pages as ListItemBag[]).map(page => ({ page })));
            }
            else {
                // When page is updated, no route will be picked, so just use the page property
                emit("update:modelValue", { page: pages as ListItemBag });
            }

            if (shouldPromptForRoute.value) {
                showRoutePicker();
            }
        }

        function updateRoute(route: ListItemBag): void {
            // This is only called if route selection is enabled, and a page is selected, so we can assume
            // internalPageValue is a ListItemBag
            emit("update:modelValue", {
                page: internalPageValue.value as ListItemBag,
                route
            });

            hideRoutePicker();
        }

        // Configure the item provider with our settings. These are not reactive
        // since we don't do lazy loading so there is no point.
        const itemProvider = ref(new PageTreeItemProvider());
        itemProvider.value.securityGrantToken = props.securityGrantToken;
        itemProvider.value.hidePageGuids = props.hidePageGuids;

        // TODO: deal with array of values
        if (internalPageValue.value && !Array.isArray(internalPageValue.value)) {
            itemProvider.value.selectedPageGuid = internalPageValue.value.value;
        }

        watch(() => props.modelValue, () => {
            // console.debug("model updated", updateRefValue(internalPageValue, props.modelValue ?? null));
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
                updateRefValue(internalPageValue, currentPage);
                refreshProvider();
                return;
            }

            const options: PagePickerGetPageNameOptionsBag = { pageGuid: pageGuid.value };
            const response = await post<string>("/api/v2/Controls/PagePickerGetPageName", {}, options);

            if (response.isSuccess && response.data) {
                currentPage = {
                    text: response.data,
                    value: pageGuid.value
                };
                updateRefValue(internalPageValue, currentPage);
            }
            else {
                console.error("Error", response.errorMessage);
                updateRefValue(internalPageValue, { value: pageGuid.value });
            }
            refreshProvider();
        }

        function refreshProvider(): void {
            const prov = new PageTreeItemProvider();
            prov.securityGrantToken = props.securityGrantToken;
            prov.hidePageGuids = props.hidePageGuids;

            if (internalPageValue.value && !Array.isArray(internalPageValue.value)) {
                prov.selectedPageGuid = internalPageValue.value.value;
            }

            itemProvider.value = prov;
        }

        return {
            internalPageValue,
            itemProvider,
            btnSize: BtnSize.ExtraSmall,
            btnType: BtnType.Link,
            isRoutePickerVisible,
            selectCurrentPage,
            onValueSelected,
            updatePage,
            updateRoute
        };
    },

    template: `
<TreeItemPicker
    :modelValue="internalPageValue"
    @update:modelValue="updatePage"
    formGroupClasses="location-item-picker"
    iconCssClass="fa fa-file"
    :provider="itemProvider"
    :multiple="multiple"
    @valueSelected="onValueSelected"
    :autoExpand="true">

    <template #customPickerActions>
        <RockButton @click="selectCurrentPage" :btnSize="btnSize" :btnType="btnType" title="Select Current Page"><i class="fa fa-file-o"></i></RockButton>
    </template>

</TreeItemPicker>
<div v-if="isRoutePickerVisible">
    ROUTE PROMPT
</div>
`
});
