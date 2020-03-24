using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Distancify.Litium.Rounding.ISO4217.OrderCalculators;
using Litium.Foundation.Modules.ECommerce.Carriers;
using Litium.Globalization;
using NSubstitute;
using Xunit;

namespace Distancify.Litium.Rounding.ISO4217.Tests
{
    public class OrderTotalCalculatorTests
    {
        private readonly OrderTotalCalculator sut;

        public OrderTotalCalculatorTests()
        {
            var currencyService = Substitute.For<CurrencyService>();
            currencyService.Get(Arg.Any<Guid>()).Returns(new Currency("IQD"));

            sut = new OrderTotalCalculator(currencyService);
        }

        [Fact]
        public void CalculateFromCarrier_ListPrice_RoundRowTotal()
        {
            var order = new OrderCarrier
            {
                OrderRows = new List<OrderRowCarrier>
                {
                    new OrderRowCarrier
                    {
                        UnitListPrice = 1.67226891m,
                        Quantity = 3
                    }
                }
            };

            var result = CalculateRow(order);

            Assert.Equal(5.016m, result.TotalPrice);
        }

        [Fact]
        public void CalculateFromCarrier_CampaignPrice_RoundRowTotal()
        {
            var order = new OrderCarrier
            {
                OrderRows = new List<OrderRowCarrier>
                {
                    new OrderRowCarrier
                    {
                        UnitListPrice = 3.34453782m,
                        UnitCampaignPrice = 1.67226891m,
                        Quantity = 3,
                        CampaignID = Guid.NewGuid()
                    }
                }
            };

            var result = CalculateRow(order);

            Assert.Equal(5.016m, result.TotalPrice);
        }

        [Fact]
        public void CalculateFromCarrier_CampaignPrice_DoNotSetDiscountAmount()
        {
            var order = new OrderCarrier
            {
                OrderRows = new List<OrderRowCarrier>
                {
                    new OrderRowCarrier
                    {
                        UnitListPrice = 3.34453782m,
                        UnitCampaignPrice = 1.67226891m,
                        Quantity = 3,
                        CampaignID = Guid.NewGuid()
                    }
                }
            };

            var result = CalculateRow(order);

            Assert.Equal(0, result.DiscountAmount);
        }

        [Fact]
        public void CalculateFromCarrier_UnitCampaignPriceWithDiscountPercentage_SetsDiscountAmount()
        {
            var order = new OrderCarrier
            {
                OrderRows = new List<OrderRowCarrier>
                {
                    new OrderRowCarrier
                    {
                        UnitListPrice = 3.34453782m,
                        UnitCampaignPrice = 1.67226891m,
                        DiscountPercentage = 25m,
                        Quantity = 3,
                        CampaignID = Guid.NewGuid()
                    }
                }
            };

            var result = CalculateRow(order);

            Assert.Equal(1.2542016825m, result.DiscountAmount);
        }

        [Fact]
        public void CalculateFromCarrier_UnitListPriceWithDiscountPercentage_SetsDiscountAmount()
        {
            var order = new OrderCarrier
            {
                OrderRows = new List<OrderRowCarrier>
                {
                    new OrderRowCarrier
                    {
                        UnitListPrice = 1.67226891m,
                        DiscountPercentage = 25m,
                        Quantity = 3
                    }
                }
            };

            var result = CalculateRow(order);

            Assert.Equal(1.2542016825m, result.DiscountAmount);
        }

        [Fact]
        public void CalculateFromCarrier_UnitListPriceWithDiscountPercentage_SubtractTotalWithDiscountAmount()
        {
            var order = new OrderCarrier
            {
                OrderRows = new List<OrderRowCarrier>
                {
                    new OrderRowCarrier
                    {
                        UnitListPrice = 1.67226891m,
                        DiscountPercentage = 25m,
                        Quantity = 3
                    }
                }
            };

            var result = CalculateRow(order);

            Assert.Equal(3.762m, result.TotalPrice);
        }

        [Fact]
        public void CalculateFromCarrier_DontUseCampaignPriceIfCampaignIdIsNotSet()
        {
            var order = new OrderCarrier
            {
                OrderRows = new List<OrderRowCarrier>
                {
                    new OrderRowCarrier
                    {
                        UnitListPrice = 3.34453782m,
                        UnitCampaignPrice = 1.67226891m,
                        Quantity = 3
                    }
                }
            };

            var result = CalculateRow(order);

            Assert.Equal(10.035m, result.TotalPrice);
        }

        [Fact]
        public void CalculateFromCarrier_OrderTotal()
        {
            var order = new OrderCarrier
            {
                OrderRows = new List<OrderRowCarrier>
                {
                    new OrderRowCarrier
                    {
                        UnitListPrice = 1.67226891m,
                        Quantity = 3
                    },
                    new OrderRowCarrier
                    {
                        UnitListPrice = 1.67226891m,
                        Quantity = 3
                    }
                }
            };

            CalculateOrder(order);

            Assert.Equal(10.032m, order.TotalOrderRow);
        }

        [Fact]
        public void CalculateOrderRowTotal_NoRoundingIfNoOrderAvailable()
        {
            var row = new OrderRowCarrier
            {
                UnitListPrice = 1.67226891m,
                Quantity = 1
            };

            sut.CalculateOrderRowTotal(row);

            Assert.Equal(1.67226891m, row.TotalPrice);
        }

        [Fact]
        public void CalculateOrderRowTotal_RoundBasedOnContext()
        {
            var order = new OrderCarrier
            {
                OrderRows = new List<OrderRowCarrier>
                {
                    new OrderRowCarrier
                    {
                        UnitListPrice = 1.67226891m,
                        Quantity = 1
                    }
                }
            };

            var row = order.OrderRows.Single();

            using (CalculatorContext.Use(order))
            {
                sut.CalculateOrderRowTotal(row);
            }

            Assert.Equal(1.672m, row.TotalPrice);
        }

        private void CalculateOrder(OrderCarrier order)
        {
            sut.CalculateFromCarrier(order, null);
        }

        private OrderRowCarrier CalculateRow(OrderCarrier order)
        {
            CalculateOrder(order);

            return order.OrderRows.Single();
        }

    }
}
