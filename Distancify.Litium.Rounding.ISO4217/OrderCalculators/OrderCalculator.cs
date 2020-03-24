using Litium.Foundation.Modules.ECommerce.Carriers;
using Litium.Foundation.Modules.ECommerce.Plugins.Campaigns;
using Litium.Foundation.Modules.ECommerce.Plugins.Deliveries;
using Litium.Foundation.Modules.ECommerce.Plugins.Fees;
using Litium.Foundation.Modules.ECommerce.Plugins.Orders;
using Litium.Foundation.Modules.ECommerce.Plugins.Vat;
using Litium.Foundation.Security;
using Litium.Owin.InversionOfControl;

namespace Distancify.Litium.Rounding.ISO4217.OrderCalculators
{
    [Plugin("Default")]
    public class OrderCalculator : IOrderCalculator
    {
        private readonly IDeliveryCostCalculator deliveryCostCalculator;
        private readonly IFeesCalculator feesCalculator;
        private readonly ICampaignCalculator campaignCalculator;
        private readonly IOrderTotalCalculator orderTotalCalculator;
        private readonly IVatCalculator vatCalculator;
        private readonly IOrderGrandTotalCalculator orderGrandTotalCalculator;

        public OrderCalculator(
            IDeliveryCostCalculator deliveryCostCalculator,
            IFeesCalculator feesCalculator,
            IOrderTotalCalculator orderTotalCalculator,
            ICampaignCalculator campaignCalculator,
            IVatCalculator vatCalculator,
            IOrderGrandTotalCalculator orderGrandTotalCalculator
        )
        {
            this.deliveryCostCalculator = deliveryCostCalculator;
            this.feesCalculator = feesCalculator;
            this.orderTotalCalculator = orderTotalCalculator;
            this.campaignCalculator = campaignCalculator;
            this.vatCalculator = vatCalculator;
            this.orderGrandTotalCalculator = orderGrandTotalCalculator;
        }

        /// <summary>Calculates the specified order carrier.</summary>
        /// <param name="orderCarrier"> The order carrier. </param>
        /// <param name="includeCampaignCalculator"> if set to <c>true</c> [include campaign calculator]. </param>
        /// <param name="securityToken"> The security token. </param>
        public virtual void Calculate(
            OrderCarrier orderCarrier,
            bool includeCampaignCalculator,
            SecurityToken securityToken)
        {
            using (CalculatorContext.Use(orderCarrier))
            {
                deliveryCostCalculator.CalculateFromCarrier(orderCarrier, securityToken);
                feesCalculator.CalculateFromCarrier(orderCarrier, securityToken);
                orderTotalCalculator.CalculateFromCarrier(orderCarrier, securityToken);
                if (includeCampaignCalculator)
                {
                    campaignCalculator.CalculateFromCarrier(orderCarrier, securityToken);
                }

                vatCalculator.CalculateFromCarrier(orderCarrier, securityToken);
                orderGrandTotalCalculator.Calculate(orderCarrier, securityToken);
            }
        }
    }
}
