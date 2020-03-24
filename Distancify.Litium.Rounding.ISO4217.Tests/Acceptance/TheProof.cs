using System;
using Litium;
using Litium.Foundation;
using Litium.Foundation.Modules.ECommerce.Carriers;
using Litium.Globalization;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Distancify.Litium.Rounding.ISO4217.Tests.Utils;
using Litium.Foundation.Modules.ECommerce.Plugins.Campaigns;
using Litium.Foundation.Modules.ECommerce.Plugins.Campaigns.Actions;
using Litium.Foundation.Modules.ECommerce.Plugins.Orders;
using Litium.Foundation.Modules.ECommerce.Plugins.Payments;
using Xunit;
using Litium.Foundation.Modules.ECommerce;

namespace Distancify.Litium.Rounding.ISO4217.Tests.Acceptance
{
    /// <summary>
    /// This class contains seemingly identical tests as TheProblem.cs, with the difference that it uses
    /// ISO4217's calculators and expects different results.
    /// </summary>
    public class TheProof : LitiumApplicationTestBase
    {
        private readonly Guid currencySystemId;
        private readonly Guid deliveryMethodId = Guid.Parse("1CC1572A-E33D-4070-B0B9-26A9CC698900");

        public TheProof()
        {
            currencySystemId = EnsureCurrency();
            EnsureDeliveryMethod();
        }

        [Fact]
        public void VerifyReplacementImplementationIsWorking()
        {
            var order = new OrderCarrier
            {
                CurrencyID = currencySystemId,
                GrandTotal = 857.6700m,
                TotalOrderRow = 686.1360m,
                TotalOrderRowVAT = 171.5340m,
                TotalVAT = 171.5340m,
                Deliveries = new List<DeliveryCarrier>
                {
                    new DeliveryCarrier
                    {
                        DeliveryMethodID = deliveryMethodId,
                        DeliveryCost = 129.4440m,
                        VATPercentage = 0.2500m
                    }
                },
                Fees = new List<FeeCarrier>
                {
                    new FeeCarrier
                    {
                        Amount = 129.4440m,
                        VATPercentage = 0.2500m
                    }
                },
                OrderDiscounts = new List<OrderDiscountCarrier>
                {
                    new OrderDiscountCarrier
                    {
                        DiscountAmount = 129.4440m,
                        VATPercentage = 0.2500m
                    }
                },
                OrderRows = new List<OrderRowCarrier>
                {
                    new OrderRowCarrier
                    {
                        Quantity = 1,
                        TotalPrice = 129.4440m,
                        TotalVATAmount = 32.3610m,
                        UnitListPrice = 129.44400000m,
                        VATPercentage = 0.2500m
                    },
                    new OrderRowCarrier
                    {
                        Quantity = 1,
                        TotalPrice = 154.2840m,
                        TotalVATAmount = 38.5710m,
                        UnitListPrice = 154.28400000m,
                        VATPercentage = 0.2500m
                    },
                    new OrderRowCarrier
                    {
                        Quantity = 1,
                        TotalPrice = 192.9240m,
                        TotalVATAmount = 48.2310m,
                        UnitListPrice = 192.92400000m,
                        VATPercentage = 0.2500m
                    },
                    new OrderRowCarrier
                    {
                        Quantity = 1,
                        TotalPrice = 43.8840m,
                        TotalVATAmount = 10.9710m,
                        UnitListPrice = 43.88400000m,
                        VATPercentage = 0.2500m
                    },
                    new OrderRowCarrier
                    {
                        Quantity = 2,
                        TotalPrice = 165.6000m,
                        TotalVATAmount = 41.4000m,
                        UnitListPrice = 82.80000000m,
                        VATPercentage = 0.2500m
                    },
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

            CalculateOrder(order);

            var sut = IoC.Resolve<IPaymentInfoRowFactory>();

            var result = sut.Create(order, null);

            Assert.True(IoC.Resolve<IOrderGrandTotalCalculator>().Validate(order, Solution.Instance.SystemToken));

            Assert.Empty(result.Single().Rows
                .Where(r => r.ReferenceType == global::Litium.Foundation.Modules.ECommerce.Payments.PaymentInfoRowType.RoundingOffAdjustment)
                .Select(r => r.TotalAmountWithVAT));
        }

        [Fact]
        public void VerifyBuyXPayYActionCampaign()
        {
            // The BuyXPayYAction uses IOrderTotalCalculator.CalculateOrderRowTotal which is a bad method as it has
            // no access to the OrderCarrier, and therefor doesn't know which currency is being used.

            var order = new OrderCarrier
            {
                CurrencyID = currencySystemId,
                OrderRows = new List<OrderRowCarrier>
                {
                    new OrderRowCarrier
                    {
                        Quantity = 3,
                        TotalPrice = 5.01m,
                        TotalVATAmount = 0.96m,
                        UnitListPrice = 1.67226891m,
                        VATPercentage = 0.1900m
                    }
                }
            };

            var filteredOrderRow = new FilteredOrderRow();
            filteredOrderRow.SetInternalProperty(r => r.OrderRowCarrier, order.OrderRows.First());

            var args = new ActionArgs();
            args.SetInternalProperty(r => r.OrderCarrier, order);
            args.SetInternalProperty(r => r.FilteredOrderRows, new List<FilteredOrderRow> { filteredOrderRow });

            var campaignData = new BuyXPayYAction.Data
            {
                BuyQuantity = 3,
                PayQuantity = 2
            };

            var sut = new CampaignAction(campaignData);
            sut.PublicProcess(args);

            CalculateOrder(order);

            Assert.Equal(1.67m, order.TotalDiscount);
            Assert.Equal(1.99m, order.TotalDiscountWithVAT);
        }

        private void CalculateOrder(OrderCarrier order)
        {
            IoC.Resolve<IOrderCalculator>().Calculate(order, true, Solution.Instance.SystemToken);
        }

        private Guid EnsureCurrency()
        {
            var currencyService = IoC.Resolve<CurrencyService>();
            var currency = currencyService.Get("SEK");
            if (currency == null)
            {
                using (Solution.Instance.SystemToken.Use())
                {
                    currency = new Currency("SEK");
                    currency.SystemId = Guid.NewGuid();
                    currencyService.Create(currency);
                }
            }

            return currency.SystemId;
        }

        private void EnsureDeliveryMethod()
        {
            if (ModuleECommerce.Instance.DeliveryMethods.Get(deliveryMethodId, Solution.Instance.SystemToken) == null)
            {
                ModuleECommerce.Instance.DeliveryMethods.Create(new DeliveryMethodCarrier
                {
                    ID = deliveryMethodId,
                    Name = "DeliveryMethod"
                }, Solution.Instance.SystemToken);
            }
        }

        private class CampaignAction : BuyXPayYAction
        {
            public CampaignAction(Data data)
            {
                Initialize(data);
            }

            public bool PublicProcess(ActionArgs args)
            {
                return Process(args);
            }
        }
    }
}
