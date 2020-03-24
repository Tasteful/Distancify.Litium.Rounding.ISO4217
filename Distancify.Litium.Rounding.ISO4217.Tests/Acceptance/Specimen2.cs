using Litium.Foundation.Modules.ECommerce.Carriers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Distancify.Litium.Rounding.ISO4217.Tests.Acceptance
{
    public class Specimen2 : SpecimenBase
    {
        protected override string CurrencyId => "SEK";

        protected override ICollection<DeliveryMethodCostCarrier> DeliveryMethodCosts => new List<DeliveryMethodCostCarrier>
        {
            new DeliveryMethodCostCarrier(DeliveryMethodId, CurrencySystemId, 5.9000m, true, 0.2400m)
        };

        protected override OrderCarrier Order()
        {
            return new OrderCarrier
            {
                CurrencyID = CurrencySystemId,
                GrandTotal = 0m,
                TotalOrderRow = 100m,
                TotalOrderRowVAT = 25m,
                TotalVAT = 25m,
                TotalDeliveryCost = 0m,
                TotalDeliveryCostVAT = 0m,
                TotalFee = 0,
                TotalFeeVAT = 0,
                TotalDiscount = 1000,
                TotalDiscountVAT = 250,
                OverallVatPercentage = .25m,
                OrderRows = new List<OrderRowCarrier>
                {
                    new OrderRowCarrier
                    {
                        UnitListPrice = 100,
                        UnitCampaignPrice = 0m,
                        VATPercentage = 0.2500m,
                        TotalPrice = 100m,
                        TotalVATAmount = 25m,
                        Quantity = 1m,
                    }
                },
                OrderDiscounts = new List<OrderDiscountCarrier>
                {
                    new OrderDiscountCarrier
                    {
                        DiscountAmount = 1000,
                        VATAmount = 250
                    }
                },
                PaymentInfo = new List<PaymentInfoCarrier>
                {
                    new PaymentInfoCarrier
                    {
                        PaymentProvider = "DirectPay",
                        PaymentMethod = "DirectDebit"
                    }
                }
            };
        }
    }
}
