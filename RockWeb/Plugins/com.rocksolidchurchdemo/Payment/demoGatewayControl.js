System.register(["vue", "/Obsidian/Controls/gatewayControl"], function (exports_1, context_1) {
    "use strict";

    var vue_1, gatewayControl_1;
    var __moduleName = context_1 && context_1.id;

    return {
        setters: [
            function (vue_1_1) {
                vue_1 = vue_1_1;
            },
            function (gatewayControl_1_1) {
                gatewayControl_1 = gatewayControl_1_1;
            }
        ],
        execute: function () {
            exports_1("default", vue_1.defineComponent({
                name: "DemoGatewayControl",
                components: {
                },
                props: {
                    settings: {
                        type: Object,
                        required: true
                    }
                },

                setup(props, { emit }) {
                    // This calculates and provides the value from the Block Setting on
                    // if the Credit Card Payment Type is enabled. You do not need to modify
                    // any of this code. You can access the value like:
                    //
                    // if (hasCreditCardPaymentType.value) { /* do stuff */ }
                    const hasCreditCardPaymentType = vue_1.computed(() => {
                        var _a, _b;
                        return (_b = (_a = props.settings.enabledPaymentTypes) === null || _a === void 0 ? void 0 : _a.includes(0)) !== null && _b !== void 0 ? _b : false;
                    });

                    // This calculates and provides the value from the Block Setting on
                    // if the ACH Payment Type is enabled. You do not need to modify
                    // any of this code. You can access the value like:
                    //
                    // if (hasBankAccountPaymentType.value) { /* do stuff */ }
                    const hasBankAccountPaymentType = vue_1.computed(() => {
                        var _a, _b;
                        return (_b = (_a = props.settings.enabledPaymentTypes) === null || _a === void 0 ? void 0 : _a.includes(1)) !== null && _b !== void 0 ? _b : false;
                    });

                    // hasPaid.value will be true when the checkbox is checked.
                    // This is not needed but is provided as an example of how to
                    // tie something in the UI to a backend value.
                    const hasPaid = vue_1.ref(false);

                    gatewayControl_1.onSubmitPayment(() => {
                        // This method is called when the page Submit button is clicked.
                        // You can do any logic you need to start the payment here.

                        // Timeout is just to simulate an async call to perform actual payment.
                        setTimeout(() => {
                            if (hasPaid.value) {
                                // Pretend that the payment was charged.
                                handleTokenResponse("raw-token-value");
                            }
                            else {
                                // Pretend the payment had some kind of error, treat it as
                                // a validation error since that is likely the problem.
                                handleValidationErrors({ "Payment Information": "is not valid" });
                            }
                        }, 500);
                    });

                    // Emits any validation errors that prevent payment from proceeding.
                    // "validationErrors" is an Object whose keys are the field name and values are text error messages.
                    const handleValidationErrors = (validationErrors) => {
                        emit("validation", validationErrors);
                    };

                    // Emits the payment token back to the page for processing.
                    const handleTokenResponse = (token) => {
                        emit("success", token);
                    };

                    vue_1.onMounted(() => {
                        // Any initialization that needs to happen after the DOM has fully loaded.
                    });

                    return {
                        hasPaid
                    };
                },
                template: `
<div>
    <div>
        Payment Details Content
    </div>

    <div>
        <input v-model="hasPaid" type="checkbox" />&nbsp;Paid
    </div>
</div>`
            }));
        }
    };
});
