using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock;
using Rock.Financial;
using Rock.Model;
using Rock.Web.Cache;

namespace com.rocksolidchurchdemo.Payment
{
    /// <summary>
    /// Test Payment Gateway
    /// </summary>
    [Description( "Demo Payment Gateway" )]
    [Export( typeof( GatewayComponent ) )]
    [ExportMetadata( "ComponentName", "DemoGateway" )]

    public class DemoGateway : GatewayComponent, IObsidianHostedGatewayComponent
    {
        #region Obsidian

        /// <summary>
        /// Creates the customer account using a token received and returns a customer account token that can be used for future transactions.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public string CreateCustomerAccount( FinancialGateway financialGateway, ReferencePaymentInfo paymentInfo, out string errorMessage )
        {
            errorMessage = string.Empty;
            return Guid.NewGuid().ToString( "N" );
        }

        /// <summary>
        /// Gets the obsidian control file URL.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <returns></returns>
        public string GetObsidianControlFileUrl( FinancialGateway financialGateway )
        {
            return "/Plugins/com.rocksolidchurchdemo/Payment/demoGatewayControl.js";
        }

        /// <inheritdoc/>
        public object GetObsidianControlSettings( FinancialGateway financialGateway, HostedPaymentInfoControlOptions options )
        {
            return new
            {
            };
        }

        /// <inheritdoc/>
        public bool TryGetPaymentTokenFromParameters( FinancialGateway financialGateway, IDictionary<string, string> parameters, out string paymentToken )
        {
            paymentToken = null;

            return false;
        }

        /// <inheritdoc/>
        public bool IsPaymentTokenCharged( FinancialGateway financialGateway, string paymentToken )
        {
            return false;
        }

        /// <inheritdoc/>
        public FinancialTransaction FetchPaymentTokenTransaction( Rock.Data.RockContext rockContext, FinancialGateway financialGateway, int? fundId, string paymentToken )
        {
            // This method is not required in our implementation.
            throw new NotImplementedException();
        }

        #endregion

        #region Gateway Component Implementation

        /// <summary>
        /// Charges the specified payment info.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="paymentInfo">The payment info.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public override FinancialTransaction Charge( FinancialGateway financialGateway, PaymentInfo paymentInfo, out string errorMessage )
        {
            errorMessage = string.Empty;

            System.Diagnostics.Debug.WriteLine( $"Charging with token {( paymentInfo as ReferencePaymentInfo )?.ReferenceNumber}" );

            var transaction = new FinancialTransaction();
            transaction.TransactionCode = "T" + RockDateTime.Now.ToString( "yyyyMMddHHmmssFFF" );

            transaction.FinancialPaymentDetail = new FinancialPaymentDetail()
            {
                ExpirationMonth = ( paymentInfo as ReferencePaymentInfo )?.PaymentExpirationDate?.Month,
                ExpirationYear = ( paymentInfo as ReferencePaymentInfo )?.PaymentExpirationDate?.Year,
                CurrencyTypeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.AsGuid() ),
                AccountNumberMasked = paymentInfo.MaskedNumber,
                CreditCardTypeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.CREDITCARD_TYPE_VISA.AsGuid() )
            };

            return transaction;
        }

        public override FinancialTransaction Credit( FinancialTransaction origTransaction, decimal amount, string comment, out string errorMessage )
        {
            throw new NotImplementedException();
        }

        public override FinancialScheduledTransaction AddScheduledPayment( FinancialGateway financialGateway, PaymentSchedule schedule, PaymentInfo paymentInfo, out string errorMessage )
        {
            throw new NotImplementedException();
        }

        public override bool UpdateScheduledPayment( FinancialScheduledTransaction transaction, PaymentInfo paymentInfo, out string errorMessage )
        {
            throw new NotImplementedException();
        }

        public override bool CancelScheduledPayment( FinancialScheduledTransaction transaction, out string errorMessage )
        {
            throw new NotImplementedException();
        }

        public override bool ReactivateScheduledPayment( FinancialScheduledTransaction transaction, out string errorMessage )
        {
            throw new NotImplementedException();
        }

        public override bool GetScheduledPaymentStatus( FinancialScheduledTransaction transaction, out string errorMessage )
        {
            throw new NotImplementedException();
        }

        public override List<Rock.Financial.Payment> GetPayments( FinancialGateway financialGateway, DateTime startDate, DateTime endDate, out string errorMessage )
        {
            errorMessage = string.Empty;
            return new List<Rock.Financial.Payment>();
        }

        public override string GetReferenceNumber( FinancialTransaction transaction, out string errorMessage )
        {
            errorMessage = string.Empty;
            return string.Empty;
        }

        public override string GetReferenceNumber( FinancialScheduledTransaction scheduledTransaction, out string errorMessage )
        {
            errorMessage = string.Empty;
            return string.Empty;
        }

        #endregion
    }
}
