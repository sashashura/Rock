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
import { defineComponent, inject, InjectionKey, PropType, provide } from "vue";
import JavaScriptAnchor from "../Elements/javaScriptAnchor";
import ComponentFromUrl from "./componentFromUrl";

export type GatewayControlModel = {
    fileUrl: string;
    settings: Record<string, unknown>;
};

type SubmitPaymentObject = {
    callback?: SubmitPaymentFunction;
};

const submitPaymentCallbackSymbol: InjectionKey<SubmitPaymentObject> = Symbol("gateway-submit-payment-callback");
export type SubmitPaymentFunction = () => void;

export const prepareSubmitPayment = (): SubmitPaymentFunction => {
    const container: SubmitPaymentObject = {};
    provide(submitPaymentCallbackSymbol, container);

    return () => {
        if (container.callback) {
            container.callback();
        }
        else {
            throw "Submit payment callback has not been defined.";
        }
    };
};

export const onSubmitPayment = (callback: SubmitPaymentFunction): void => {
    const container = inject(submitPaymentCallbackSymbol);

    if (!container || container.callback) {
        throw "Submit payment callback already defined.";
    }

    container.callback = callback;
};

export enum ValidationField {
    CardNumber,
    Expiry,
    SecurityCode
}

export default defineComponent({
    name: "GatewayControl",
    components: {
        ComponentFromUrl,
        JavaScriptAnchor
    },
    props: {
        gatewayControlModel: {
            type: Object as PropType<GatewayControlModel>,
            required: true
        }
    },
    data() {
        return {
            isSuccess: false
        };
    },
    computed: {
        url(): string {
            return this.gatewayControlModel.fileUrl;
        },
        settings(): Record<string, unknown> {
            return this.gatewayControlModel.settings;
        }
    },
    methods: {
        /**
         * Intercept the success event, so that local state can reflect it.
         * @param token
         */
        async onSuccess ( token: string ) {
            this.isSuccess = true;
            this.$emit( "success", token );
        },

        /**
         * This method handles validation errors.
         * 
         * @param validationErrors
         */
        validationError(validationErrors: Record<string, string>) {
            this.$emit("validation", validationErrors);
        }
    },
    template: `
<ComponentFromUrl v-if="!isSuccess" :url="url" :settings="settings" @validationError="validationError" @successRaw="onSuccess" />
`
});
