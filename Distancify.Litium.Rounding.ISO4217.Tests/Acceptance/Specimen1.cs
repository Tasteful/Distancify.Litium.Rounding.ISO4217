using Litium.Foundation.Modules.ECommerce.Carriers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Distancify.Litium.Rounding.ISO4217.Tests.Acceptance
{
    public class Specimen1 : SpecimenBase
    {
        protected override string CurrencyId => "EUR";

        protected override ICollection<DeliveryMethodCostCarrier> DeliveryMethodCosts => new List<DeliveryMethodCostCarrier>
        {
            new DeliveryMethodCostCarrier(DeliveryMethodId, CurrencySystemId, 5.9000m, true, 0.2400m)
        };

        protected override OrderCarrier Order()
        {
            return new OrderCarrier
            {
                CurrencyID = CurrencySystemId,
                GrandTotal = 229.9928m,
                TotalOrderRow = 185.4800m,
                TotalOrderRowVAT = 44.5152m,
                TotalVAT = 44.5147m,
                TotalDeliveryCost = -0.0019m,
                TotalDeliveryCostVAT = -0.0005m,
                TotalFee = 0,
                TotalFeeVAT = 0,
                TotalDiscount = 0,
                TotalDiscountVAT = 0,
                OverallVatPercentage = 0,
                OrderRows = new List<OrderRowCarrier>
                {
                    new OrderRowCarrier
                    {
                        UnitListPrice = 185.48000000m,
                        UnitCampaignPrice = 0m,
                        VATPercentage = 0.2400m,
                        TotalPrice = 185.4800m,
                        TotalVATAmount = 44.5152m,
                        Quantity = 1m,
                    }
                },
                Deliveries = new List<DeliveryCarrier>
                {
                    new DeliveryCarrier
                    {
                        DeliveryMethodID = DeliveryMethodId,
                        DiscountAmount = 4.7600m
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
