using Distancify.Litium.Rounding.ISO4217.OrderCalculators;
using Litium.Foundation.Modules.ECommerce.Carriers;
using Litium.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Distancify.Litium.Rounding.ISO4217.Tests
{
    public class OrderGrandTotalCalculatorTests
    {
        [Fact]
        public void CalculateOverallVatPercentageBasedOnOrderRows()
        {
            var delivery = new DeliveryCarrier
            {
                DeliveryCost = 1.672268908m,
                DeliveryCostWithVAT = 1.99m
            };

            var order = new OrderCarrier
            {
                OrderRows = new List<OrderRowCarrier>
                {
                    new OrderRowCarrier
                    {
                        UnitListPrice = 185.48000000m,
                        UnitCampaignPrice = 0m,
                        VATPercentage = 0.2400m,
                        TotalPrice = 185.4800m,
                        TotalVATAmount = 44.52m,
                        Quantity = 1m,
                    },
                    new OrderRowCarrier
                    {
                        TotalPrice = 100,
                        TotalVATAmount = 0,
                        CarrierState =
                        {
                            IsMarkedForCreating = false,
                            IsMarkedForDeleting = true
                        }
                    }
                }
            };
            order.Deliveries.Add(delivery);

            var sut = new OrderGrandTotalCalculator();

            sut.Calculate(order, null);

            Assert.Equal(0.24m, order.OverallVatPercentage);
        }

        [Fact]
        public void CalculateOverallVatPercentageBasedOnOrderRows_ZeroTotalRows_DontCrash()
        {
            var delivery = new DeliveryCarrier
            {
                DeliveryCost = 1.672268908m,
                DeliveryCostWithVAT = 1.99m
            };

            var order = new OrderCarrier
            {
                OrderRows = new List<OrderRowCarrier>
                {
                    new OrderRowCarrier
                    {
                        UnitListPrice = 0,
                        UnitCampaignPrice = 0,
                        VATPercentage = 0.2400m,
                        TotalPrice = 0,
                        TotalVATAmount = 0,
                        Quantity = 1m,
                    }
                }
            };
            order.Deliveries.Add(delivery);

            var sut = new OrderGrandTotalCalculator();

            sut.Calculate(order, null);
        }

        [Fact]
        public void SalesOrder_OrderDiscountHigherThanTotal_NoNegativeOrderValue()
        {
            var order = new OrderCarrier
            {
                OrderRows = new List<OrderRowCarrier>
                {
                    new OrderRowCarrier
                    {
                        UnitListPrice = 100,
                        UnitCampaignPrice = 0m,
                        VATPercentage = 0.25m,
                        TotalPrice = 100m,
                        TotalVATAmount = 25m,
                        Quantity = 1m,
                    }
                }
            };
            order.OrderDiscounts.Add(new OrderDiscountCarrier
            {
                DiscountAmount = 1000,
                VATAmount = 250
            });

            var sut = new OrderGrandTotalCalculator();

            sut.Calculate(order, null);

            Assert.Equal(0, order.GrandTotal);
        }
    }
}
