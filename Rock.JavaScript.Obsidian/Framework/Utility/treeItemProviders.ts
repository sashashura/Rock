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

import { Guid } from "@Obsidian/Types";
import { emptyGuid } from "./guid";
import { post } from "./http";
import { TreeItemBag } from "@Obsidian/ViewModels/Utility/treeItemBag";
import { CategoryPickerChildTreeItemsOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/categoryPickerChildTreeItemsOptionsBag";
import { LocationPickerGetActiveChildrenOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/locationPickerGetActiveChildrenOptionsBag";
import { DataViewPickerGetDataViewsOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/dataViewPickerGetDataViewsOptionsBag";
import { WorkflowTypePickerGetWorkflowTypesOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/workflowTypePickerGetWorkflowTypesOptionsBag";
import { PagePickerGetChildrenOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/pagePickerGetChildrenOptionsBag";
import { PagePickerGetSelectedPageHierarchyOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/PagePickerGetSelectedPageHierarchyOptionsBag";
import { flatten } from "./arrayUtils";

/**
 * The methods that must be implemented by tree item providers. These methods
 * provide the TreeItem objects to be displayed when lazy loading is being used.
 */
export interface ITreeItemProvider {
    /**
     * Get the root items to be displayed in the tree list.
     *
     * @returns A collection of TreeItem objects, optionally wrapped in a Promise
     * if the loading is being performed asynchronously.
     */
    getRootItems(): Promise<TreeItemBag[]> | TreeItemBag[];

    /**
     * Get the child items of the given tree item.
     *
     * @param item The parent item whose children should be loaded.
     *
     * @returns A collection of TreeItem objects, optionally wrapped in a Promise
     * if the loading is being performed asynchronously.
     */
    getChildItems(item: TreeItemBag): Promise<TreeItemBag[]> | TreeItemBag[];
}

/**
 * Tree Item Provider for retrieving categories from the server and displaying
 * them inside a tree list.
 */
export class CategoryTreeItemProvider implements ITreeItemProvider {
    /**
     * The root category to start pulling categories from. Set to undefined to
     * begin with any category that does not have a parent.
     */
    public rootCategoryGuid?: Guid;

    /**
     * The entity type unique identifier to restrict results to. Set to undefined
     * to include all categories, regardless of entity type.
     */
    public entityTypeGuid?: Guid;

    /**
     * The value that must match in the category EntityTypeQualifierColumn
     * property. Set to undefined or an empty string to ignore.
     */
    public entityTypeQualifierColumn?: string;

    /**
     * The value that must match in the category EntityTypeQualifierValue
     * property.
     */
    public entityTypeQualifierValue?: string;

    /**
     * The security grant token that will be used to request additional access
     * to the category list.
     */
    public securityGrantToken?: string | null;

    /**
     * Gets the child items from the server.
     *
     * @param parentGuid The parent item whose children are retrieved.
     *
     * @returns A collection of TreeItem objects as an asynchronous operation.
     */
    private async getItems(parentGuid?: Guid | null): Promise<TreeItemBag[]> {
        const options: Partial<CategoryPickerChildTreeItemsOptionsBag> = {
            parentGuid: parentGuid,
            entityTypeGuid: this.entityTypeGuid,
            entityTypeQualifierColumn: this.entityTypeQualifierColumn,
            entityTypeQualifierValue: this.entityTypeQualifierValue,
            lazyLoad: false,
            securityGrantToken: this.securityGrantToken
        };

        const response = await post<TreeItemBag[]>("/api/v2/Controls/CategoryPickerChildTreeItems", {}, options);

        if (response.isSuccess && response.data) {
            return response.data;
        }
        else {
            console.log("Error", response.errorMessage);
            return [];
        }
    }

    /**
     * @inheritdoc
     */
    async getRootItems(): Promise<TreeItemBag[]> {
        return await this.getItems(this.rootCategoryGuid);
    }

    /**
     * @inheritdoc
     */
    async getChildItems(item: TreeItemBag): Promise<TreeItemBag[]> {
        return this.getItems(item.value);
    }
}

/**
 * Tree Item Provider for retrieving locations from the server and displaying
 * them inside a tree list.
 */
export class LocationTreeItemProvider implements ITreeItemProvider {
    /**
     * The security grant token that will be used to request additional access
     * to the category list.
     */
    public securityGrantToken?: string | null;

    /**
     * Gets the child items from the server.
     *
     * @param parentGuid The parent item whose children are retrieved.
     *
     * @returns A collection of TreeItem objects as an asynchronous operation.
     */
    private async getItems(parentGuid?: Guid | null): Promise<TreeItemBag[]> {
        const options: Partial<LocationPickerGetActiveChildrenOptionsBag> = {
            guid: parentGuid ?? emptyGuid,
            rootLocationGuid: emptyGuid,
            securityGrantToken: this.securityGrantToken
        };
        const url = "/api/v2/Controls/LocationPickerGetActiveChildren";
        const response = await post<TreeItemBag[]>(url, undefined, options);

        if (response.isSuccess && response.data) {
            return response.data;
        }
        else {
            console.log("Error", response.errorMessage);
            return [];
        }
    }

    /**
     * @inheritdoc
     */
    async getRootItems(): Promise<TreeItemBag[]> {
        return await this.getItems(null);
    }

    /**
     * @inheritdoc
     */
    async getChildItems(item: TreeItemBag): Promise<TreeItemBag[]> {
        return this.getItems(item.value);
    }
}

/**
 * Tree Item Provider for retrieving data views from the server and displaying
 * them inside a tree list.
 */
export class DataViewTreeItemProvider implements ITreeItemProvider {
    /**
     * The entity type unique identifier to restrict results to. Set to undefined
     * to include all categories, regardless of entity type.
     */
    public entityTypeGuid?: Guid;

    /**
     * The security grant token that will be used to request additional access
     * to the category list.
     */
    public securityGrantToken?: string | null;

    /**
     * Gets the child items from the server.
     *
     * @param parentGuid The parent item whose children are retrieved.
     *
     * @returns A collection of TreeItem objects as an asynchronous operation.
     */
    private async getItems(parentGuid?: Guid | null): Promise<TreeItemBag[]> {
        const options: Partial<DataViewPickerGetDataViewsOptionsBag> = {
            parentGuid,
            getCategorizedItems: true,
            includeCategoriesWithoutChildren: false,
            entityTypeGuidFilter: this.entityTypeGuid,
            lazyLoad: false,
            securityGrantToken: this.securityGrantToken,
        };

        const response = await post<TreeItemBag[]>("/api/v2/Controls/DataViewPickerGetDataViews", {}, options);

        if (response.isSuccess && response.data) {
            return response.data;
        }
        else {
            console.log("Error", response.errorMessage);
            return [];
        }
    }

    /**
     * @inheritdoc
     */
    async getRootItems(): Promise<TreeItemBag[]> {
        return await this.getItems();
    }

    /**
     * @inheritdoc
     */
    async getChildItems(item: TreeItemBag): Promise<TreeItemBag[]> {
        return this.getItems(item.value);
    }
}

/**
 * Tree Item Provider for retrieving categories from the server and displaying
 * them inside a tree list.
 */
export class WorkflowTypeTreeItemProvider implements ITreeItemProvider {
    /**
     * The entity type unique identifier to restrict results to. Set to undefined
     * to include all categories, regardless of entity type.
     */
    public includeInactiveItems?: boolean;

    /**
     * The security grant token that will be used to request additional access
     * to the category list.
     */
    public securityGrantToken?: string | null;

    /**
     * Gets the child items from the server.
     *
     * @param parentGuid The parent item whose children are retrieved.
     *
     * @returns A collection of TreeItem objects as an asynchronous operation.
     */
    private async getItems(parentGuid?: Guid | null): Promise<TreeItemBag[]> {
        const options: Partial<WorkflowTypePickerGetWorkflowTypesOptionsBag> = {
            parentGuid,
            includeInactiveItems: this.includeInactiveItems ?? false,
            securityGrantToken: this.securityGrantToken,
        };

        const response = await post<TreeItemBag[]>("/api/v2/Controls/WorkflowTypePickerGetWorkflowTypes", {}, options);

        if (response.isSuccess && response.data) {
            return response.data;
        }
        else {
            console.log("Error", response.errorMessage);
            return [];
        }
    }

    /**
     * @inheritdoc
     */
    async getRootItems(): Promise<TreeItemBag[]> {
        return await this.getItems();
    }

    /**
     * @inheritdoc
     */
    async getChildItems(item: TreeItemBag): Promise<TreeItemBag[]> {
        return this.getItems(item.value);
    }
}



/**
 * Tree Item Provider for retrieving pages from the server and displaying
 * them inside a tree list.
 */
export class PageTreeItemProvider implements ITreeItemProvider {
    /**
     * The security grant token that will be used to request additional access
     * to the category list.
     */
    public securityGrantToken?: string | null;

    /**
     * List of GUIDs or pages to exclude from the list.
     */
    public hidePageGuids?: Guid[] | null;

    /**
     * Currently selected page
     */
    public selectedPageGuids?: Guid[] | null;

    /**
     * Gets the child items of the given parent (or root if no parent given) from the server.
     *
     * @param parentGuid The parent item whose children are retrieved.
     *
     * @returns A collection of TreeItem objects as an asynchronous operation.
     */
    private async getItems(parentGuid?: Guid | null): Promise<TreeItemBag[]> {
        let result: TreeItemBag[];

        const options: Partial<PagePickerGetChildrenOptionsBag> = {
            guid: parentGuid ?? emptyGuid,
            rootPageGuid: null,
            hidePageGuids: this.hidePageGuids ?? [],
            securityGrantToken: this.securityGrantToken
        };
        const url = "/api/v2/Controls/PagePickerGetChildren";
        const response = await post<TreeItemBag[]>(url, undefined, options);

        if (response.isSuccess && response.data) {
            result = response.data;
        }
        else {
            console.log("Error", response.errorMessage);
            return [];
        }

        // If we're getting child nodes or if there is no selected page
        if (parentGuid || !this.selectedPageGuids) {
            return result;
        }

        // If we're getting the root elements and we have a selected page, we also want to grab
        // all the parent pages so we can pre-load the entire hierarchy to the selected page
        return this.getHierarchyToSelectedPage(result);
    }

    /**
     * Get the hierarchical list of parent pages of the selectedPageGuid
     *
     * @returns A list of GUIDs of the parent pages
     */
    private async getParentList(): Promise<Guid[]> {
        const options: PagePickerGetSelectedPageHierarchyOptionsBag = {
            selectedPageGuids: this.selectedPageGuids,
            securityGrantToken: this.securityGrantToken
        };
        const url = "/api/v2/Controls/PagePickerGetSelectedPageHierarchy";
        const response = await post<Guid[]>(url, undefined, options);

        if (response.isSuccess && response.data) {
            return response.data;
        }
        else {
            console.log("Error", response.errorMessage);
            return [];
        }
    }

    /**
     * Fill in pages to the depth of the selected page
     *
     * @param rootLayer The bottom layer of pages that we'll build depth upon
     *
     * @return The augmented `rootLayer` with the child pages
     */
    private async getHierarchyToSelectedPage(rootLayer: TreeItemBag[]): Promise<TreeItemBag[]> {
        const parents = await this.getParentList();

        if (!parents || parents.length == 0) {
            // Selected page has no parents, so we're done.
            return rootLayer;
        }

        const childLists = await Promise.all(parents.map(guid => this.getItems(guid)));
        const allPages = rootLayer.concat(flatten(childLists));

        parents.forEach((parentGuid, i) => {
            const parentPage: TreeItemBag | undefined = allPages.find(page => page.value == parentGuid);
            if (parentPage) {
                parentPage.children = childLists[i];
            }
        });

        return rootLayer;
    }


    /**
     * @inheritdoc
     */
    async getRootItems(): Promise<TreeItemBag[]> {
        return await this.getItems(null);
    }

    /**
     * @inheritdoc
     */
    async getChildItems(item: TreeItemBag): Promise<TreeItemBag[]> {
        return this.getItems(item.value);
    }
}