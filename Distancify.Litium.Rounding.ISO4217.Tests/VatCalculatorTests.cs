using System;
using System.Collections.Generic;
using Distancify.Litium.Rounding.ISO4217.OrderCalculators;
using Litium.Foundation.Modules.ECommerce.Carriers;
using Litium.Globalization;
using NSubstitute;
using Xunit;

namespace Distancify.Litium.Rounding.ISO4217.Tests
{
    public class VatCalculatorTests
    {
        // For some reason VatCalculator in default implementation is also responsible for calculating order discount totals.
        // It might be because it's the only calculator that runs after campaign engine, and total discount is needed to VatCalculator


        private VatCalculator sut;

        public VatCalculatorTests()
        {
            UseCurrency();
        }

        private void UseCurrency(string currencyCode = "IQD")
        {
            var currencyService = Substitute.For<CurrencyService>();
            currencyService.Get(Arg.Any<Guid>()).Returns(new Currency(currencyCode));

            sut = new VatCalculator(currencyService);
        }

        [Fact]
        public void CalculateTotalOrderVat()
        {
            var order = new OrderCarrier();
            order.Deliveries.Add(new DeliveryCarrier
            {
                DeliveryCost = 1.67226891m,
                VATPercentage = 0.19m
            });
            order.Fees.Add(new FeeCarrier
            {
                Amount = 1.67226891m,
                VATPercentage = 0.19m
            });
            order.OrderRows.Add(new OrderRowCarrier
            {
                UnitListPrice = 1.67226891m,
                // Currency decimals in this example = 3
                TotalPrice = 1.672m,
                VATPercentage = 0.19m,
                Quantity = 1
            });
            order.OrderDiscounts.Add(new OrderDiscountCarrier
            {
                DiscountAmountWithVAT = 1.99m,
                VATPercentage = 0.19m
            });

            sut.CalculateFromCarrier(order, null);

            Assert.Equal(0.318m * 2, order.TotalVAT);
        }

        #region CalculateOrderRowVat
        [Fact]
        public void CalculateOrderRowVat()
        {
            var row = new OrderRowCarrier
            {
                UnitListPrice = 1.67226891m,
                // Currency decimals in this example = 3
                TotalPrice = 1.672m,
                VATPercentage = 0.19m,
                Quantity = 1
            };

            var order = new OrderCarrier();
            using (CalculatorContext.Use(order))
            {
                sut.CalculateOrderRowVat(row);
            }

            Assert.Equal(0.318m, row.TotalVATAmount);
            Assert.Equal(1.99m, row.TotalPriceWithVAT);
        }

        [Fact]
        public void CalculateOrderRowVat2()
        {
            UseCurrency("EUR");
            var row = new OrderRowCarrier
            {
                UnitListPrice = 1.60483871m,
                // Currency decimals in this example = 2
                TotalPrice = 1.6m,
                VATPercentage = 0.24m,
                Quantity = 1
            };

            var order = new OrderCarrier();
            using (CalculatorContext.Use(order))
            {
                sut.CalculateOrderRowVat(row);
            }

            Assert.Equal(0.39m, row.TotalVATAmount);
            Assert.Equal(1.99m, row.TotalPriceWithVAT);
        }
        #endregion

        #region CalculateTotalOrderRowsVat
        [Fact]
        public void CalculateTotalOrderRowsVat()
        {
            var row = new OrderRowCarrier
            {
                UnitListPrice = 1.67226891m,
                // Currency decimals in this example = 3
                TotalPrice = 1.672m,
                VATPercentage = 0.19m,
                Quantity = 1
            };
            var skipRow = new OrderRowCarrier
            {
                UnitListPrice = 1.67226891m,
                // Currency decimals in this example = 3
                TotalPrice = 1.672m,
                VATPercentage = 0.19m,
                Quantity = 1,
                CarrierState =
                {
                    IsMarkedForCreating = false,
                    IsMarkedForDeleting = true
                }
            };

            var order = new OrderCarrier();
            order.OrderRows.Add(row);
            order.OrderRows.Add(row);
            order.OrderRows.Add(skipRow);

            sut.CalculateTotalOrderRowsVat(order);

            Assert.Equal(0.636m, order.TotalOrderRowVAT);
        }

        [Fact]
        public void CalculateTotalOrderRowsVat_TakeDiscountAmountIntoAccount()
        {
            var row = new OrderRowCarrier
            {
                UnitListPrice = 100,
                DiscountAmount = 40,
                TotalPrice = 160m,
                VATPercentage = 0.25m,
                Quantity = 2
            };

            var order = new OrderCarrier();
            order.OrderRows.Add(row);

            sut.CalculateTotalOrderRowsVat(order);

            Assert.Equal(40, order.TotalOrderRowVAT);
        }

        [Fact]
        public void CalculateTotalOrderRowsVat_CalledByCalculateFromCarrier()
        {
            var row = new OrderRowCarrier
            {
                UnitListPrice = 1.67226891m,
                // Currency decimals in this example = 3
                TotalPrice = 1.672m,
                VATPercentage = 0.19m,
                Quantity = 1
            };
            var skipRow = new OrderRowCarrier
            {
                UnitListPrice = 1.67226891m,
                // Currency decimals in this example = 3
                TotalPrice = 1.672m,
                VATPercentage = 0.19m,
                Quantity = 1,
                CarrierState =
                {
                    IsMarkedForCreating = false,
                    IsMarkedForDeleting = true
                }
            };

            var order = new OrderCarrier();
            order.OrderRows.Add(row);
            order.OrderRows.Add(row);
            order.OrderRows.Add(skipRow);

            sut.CalculateFromCarrier(order, null);

            Assert.Equal(0.636m, order.TotalOrderRowVAT);
        }
        #endregion

        #region CalculateTotalDeliveryVat
        [Fact]
        public void CalculateTotalDeliveryVat_SkipMarkedForDeletion()
        {
            var delivery = new DeliveryCarrier
            {
                DeliveryCost = 1.672268908m,
                DeliveryCostWithVAT = 1.99m,
                VATPercentage = 0.19m,
                CarrierState =
                {
                    IsMarkedForCreating = false,
                    IsMarkedForDeleting = true
                }
            };

            var order = new OrderCarrier();
            order.Deliveries.Add(delivery);

            sut.CalculateTotalDeliveryVat(order);

            Assert.Equal(0, delivery.TotalDeliveryCost);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CalculateTotalDeliveryVat_CalculateTotalDeliveryCostWithDiscount(bool keepWithVatConstant)
        {
            var delivery = GetDeliveryCarrier(keepWithVatConstant, 2.172268908m, 2.58500000052m, 0.19m, 0.5m);

            var order = new OrderCarrier();
            order.Deliveries.Add(delivery);

            sut.CalculateTotalDeliveryVat(order);

            Assert.Equal(1.672m, delivery.TotalDeliveryCost);
            Assert.Equal(1.990m, delivery.TotalDeliveryCostWithVat);
            Assert.Equal(0.318m, delivery.TotalVATAmount);
            Assert.Equal(0.19m, delivery.VATPercentage);
        }

        [Fact]
        public void CalculateTotalDeliveryVat_VatPercentage_NoOrderTotal_FallbackToOrderRows()
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

            sut.CalculateTotalDeliveryVat(order);

            Assert.Equal(0.24m, delivery.VATPercentage);
        }

        [Fact]
        public void CalculateTotalDeliveryVat_VatPercentage_NoOrderTotal_ZeroRowTotal_DontCrash()
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
                        UnitCampaignPrice = 0m,
                        VATPercentage = 0.2400m,
                        TotalPrice = 0,
                        TotalVATAmount = 0,
                        Quantity = 1m,
                    }
                }
            };
            order.Deliveries.Add(delivery);

            sut.CalculateTotalDeliveryVat(order);
        }

        [Fact]
        public void CalculateTotalDeliveryVat_VatPercentage_NoOrderRows_UseExistingVatPercentage()
        {
            var delivery = new DeliveryCarrier
            {
                DeliveryCost = 1.672268908m,
                DeliveryCostWithVAT = 1.99m,
                VATPercentage = 0.32m
            };

            var order = new OrderCarrier();
            order.Deliveries.Add(delivery);

            sut.CalculateTotalDeliveryVat(order);

            Assert.Equal(0.32m, delivery.VATPercentage);
        }

        [Fact]
        public void CalculateTotalDeliveryVat_OrderDeliveryCostTotals()
        {
            var delivery = new DeliveryCarrier
            {
                DeliveryCost = 1.672268908m,
                DeliveryCostWithVAT = 1.99m,
                VATPercentage = 0.19m
            };

            var order = new OrderCarrier();
            order.Deliveries.Add(delivery);
            order.Deliveries.Add(delivery);

            sut.CalculateTotalDeliveryVat(order);

            Assert.Equal(3.344m, order.TotalDeliveryCost);
            Assert.Equal(0.636m, order.TotalDeliveryCostVAT);
        }

        [Fact]
        public void CalculateTotalDeliveryVat_CalledByCalculateFromCarrier()
        {
            var delivery = new DeliveryCarrier
            {
                DeliveryCost = 1.672268908m,
                DeliveryCostWithVAT = 1.99m,
                VATPercentage = 0.19m
            };

            var order = new OrderCarrier();
            order.Deliveries.Add(delivery);
            order.Deliveries.Add(delivery);

            sut.CalculateFromCarrier(order, null);

            Assert.Equal(3.344m, order.TotalDeliveryCost);
            Assert.Equal(0.636m, order.TotalDeliveryCostVAT);
        }

        private DeliveryCarrier GetDeliveryCarrier(bool keepWithVatConstant, decimal cost, decimal costWithVat, decimal vatPercentage = 0, decimal discount = 0)
        {
            if (keepWithVatConstant)
            {
                return new DeliveryCarrier
                {
                    KeepDeliveryCostWithVatConstant = true,
                    DeliveryCostWithVAT = costWithVat,
                    DiscountAmount = discount,
                    VATPercentage = vatPercentage
                };
            }
            return new DeliveryCarrier
            {
                DeliveryCost = cost,
                VATPercentage = vatPercentage,
                DiscountAmount = discount
            };
        }
        #endregion


        #region CalculateTotalFeeVat
        [Fact]
        public void CalculateTotalFeeVat_SkipMarkedForDeletion()
        {
            var fee = new FeeCarrier
            {
                Amount = 1.672268908m,
                AmountWithVAT = 1.99m,
                VATPercentage = 0.19m,
                CarrierState =
                {
                    IsMarkedForCreating = false,
                    IsMarkedForDeleting = true
                }
            };

            var order = new OrderCarrier();
            order.Fees.Add(fee);

            sut.CalculateTotalFeeVat(order);

            Assert.Equal(0, fee.TotalAmount);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CalculateTotalFeeVatCalculateTotalDeliveryCostWithDiscount(bool keepWithVatConstant)
        {
            var fee = GetFeeCarrier(keepWithVatConstant, 2.172268908m, 2.58500000052m, 0.19m, 0.5m);

            var order = new OrderCarrier();
            order.Fees.Add(fee);

            sut.CalculateTotalFeeVat(order);

            Assert.Equal(1.672m, fee.TotalAmount);
            Assert.Equal(1.990m, fee.TotalAmountWithVAT);
            Assert.Equal(0.318m, fee.TotalVATAmount);
            Assert.Equal(0.19m, fee.VATPercentage);
        }

        [Fact]
        public void CalculateTotalFeeVat_VatPercentage_NoOrderTotal_FallbackToOrderRows()
        {
            var fee = new FeeCarrier
            {
                Amount = 1.672268908m,
                AmountWithVAT = 1.99m
            };

            var order = new OrderCarrier
            {
                OrderRows = new List<OrderRowCarrier>
                {
                    new OrderRowCarrier
                    {
                        TotalPrice = 100,
                        TotalVATAmount = 10,
                        VATPercentage = 0.1m
                    },
                    new OrderRowCarrier
                    {
                        TotalPrice = 100,
                        TotalVATAmount = 20,
                        VATPercentage = 0.2m
                    },
                    new OrderRowCarrier
                    {
                        TotalPrice = 100,
                        TotalVATAmount = 10,
                        VATPercentage = 0.1m,
                        CarrierState =
                        {
                            IsMarkedForCreating = false,
                            IsMarkedForDeleting = true
                        }
                    }
                }
            };
            order.Fees.Add(fee);

            sut.CalculateTotalFeeVat(order);

            Assert.Equal(0.15m, fee.VATPercentage);
        }

        [Fact]
        public void CalculateTotalFeeVat_VatPercentage_NoOrderRows_UseExistingVatPercentage()
        {
            var fee = new FeeCarrier
            {
                Amount = 1.672268908m,
                AmountWithVAT = 1.99m,
                VATPercentage = 0.32m
            };

            var order = new OrderCarrier();
            order.Fees.Add(fee);

            sut.CalculateTotalFeeVat(order);

            Assert.Equal(0.32m, fee.VATPercentage);
        }

        [Fact]
        public void CalculateTotalFeeVat_OrderFeeTotals()
        {
            var fee = new FeeCarrier
            {
                Amount = 1.672268908m,
                AmountWithVAT = 1.99m,
                VATPercentage = 0.19m
            };

            var order = new OrderCarrier();
            order.Fees.Add(fee);
            order.Fees.Add(fee);

            sut.CalculateTotalFeeVat(order);

            Assert.Equal(3.344m, order.TotalFee);
            Assert.Equal(0.636m, order.TotalFeeVAT);
        }

        [Fact]
        public void CalculateTotalFeeVat_CalledByCalculateFromCarrier()
        {
            var fee = new FeeCarrier
            {
                Amount = 1.672268908m,
                AmountWithVAT = 1.99m,
                VATPercentage = 0.19m
            };

            var order = new OrderCarrier();
            order.Fees.Add(fee);
            order.Fees.Add(fee);

            sut.CalculateFromCarrier(order, null);

            Assert.Equal(3.344m, order.TotalFee);
            Assert.Equal(0.636m, order.TotalFeeVAT);
        }

        private FeeCarrier GetFeeCarrier(bool keepWithVatConstant, decimal amount, decimal amountWithVat, decimal vatPercentage = 0, decimal discount = 0)
        {
            if (keepWithVatConstant)
            {
                return new FeeCarrier
                {
                    KeepAmountWithVATConstant = true,
                    AmountWithVAT = amountWithVat,
                    DiscountAmount = discount,
                    VATPercentage = vatPercentage
                };
            }
            return new FeeCarrier
            {
                Amount = amount,
                VATPercentage = vatPercentage,
                DiscountAmount = discount
            };
        }
        #endregion

        #region CalculateTotalDiscount
        [Fact]
        public void CalculateTotalDiscount_DiscountWithVAT_CalculateDiscountAmountWithoutVAT()
        {
            var discount = new OrderDiscountCarrier
            {
                DiscountAmountWithVAT = 1.99m,
                VATPercentage = 0.19m
            };

            var order = new OrderCarrier();
            order.OrderDiscounts.Add(discount);

            sut.CalculateTotalDiscount(order);

            Assert.Equal(1.672m, discount.DiscountAmount);
        }

        [Fact]
        public void CalculateTotalDiscount_DiscountWithoutVAT_CalculateDiscountAmountWithVAT()
        {
            var discount = new OrderDiscountCarrier
            {
                DiscountAmount = 1.672268908m,
                VATPercentage = 0.19m
            };

            var order = new OrderCarrier();
            order.OrderDiscounts.Add(discount);

            sut.CalculateTotalDiscount(order);

            Assert.Equal(1.99m, discount.DiscountAmountWithVAT);
        }

        [Fact]
        public void CalculateTotalDiscount_SkipMarkedForDeletion()
        {
            var discount = new OrderDiscountCarrier
            {
                DiscountAmount = 1.672268908m,
                VATPercentage = 0.19m,
                CarrierState =
                {
                    IsMarkedForCreating = false,
                    IsMarkedForDeleting = true
                }
            };

            var order = new OrderCarrier();
            order.OrderDiscounts.Add(discount);

            sut.CalculateTotalDiscount(order);

            Assert.Equal(0, order.TotalDiscount);
        }

        [Fact]
        public void CalculateTotalDiscount_VatPercentage_NoOrderTotal_FallbackToOrderRows()
        {
            var discount = new OrderDiscountCarrier
            {
                DiscountAmount = 1.672268908m
            };

            var order = new OrderCarrier
            {
                OrderRows = new List<OrderRowCarrier>
                {
                    new OrderRowCarrier
                    {
                        TotalPrice = 100,
                        TotalVATAmount = 10,
                        VATPercentage = 0.1m
                    },
                    new OrderRowCarrier
                    {
                        TotalPrice = 100,
                        TotalVATAmount = 20,
                        VATPercentage = 0.2m
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
            order.OrderDiscounts.Add(discount);

            sut.CalculateTotalDiscount(order);

            Assert.Equal(0.15m, discount.VATPercentage);
        }

        [Fact]
        public void CalculateTotalDiscount_VatPercentage_NoOrderRows_UseExistingVatPercentage()
        {
            var discount = new OrderDiscountCarrier
            {
                DiscountAmount = 1.672268908m,
                VATPercentage = 0.32m
            };

            var order = new OrderCarrier();
            order.OrderDiscounts.Add(discount);

            sut.CalculateTotalDiscount(order);

            Assert.Equal(0.32m, discount.VATPercentage);
        }

        [Fact]
        public void CalculateTotalDiscount_OrderDiscountTotals()
        {
            var discount = new OrderDiscountCarrier
            {
                DiscountAmount = 1.672268908m,
                VATPercentage = 0.19m
            };

            var order = new OrderCarrier();
            order.OrderDiscounts.Add(discount);
            order.OrderDiscounts.Add(discount);

            sut.CalculateTotalDiscount(order);

            Assert.Equal(3.344m, order.TotalDiscount);
            Assert.Equal(0.636m, order.TotalDiscountVAT);
        }

        [Fact]
        public void CalculateTotalDiscount_CalledByCalculateFromCarrier()
        {
            var discount = new OrderDiscountCarrier
            {
                DiscountAmount = 1.672268908m,
                VATPercentage = 0.19m
            };

            var order = new OrderCarrier();
            order.OrderDiscounts.Add(discount);
            order.OrderDiscounts.Add(discount);

            sut.CalculateFromCarrier(order, null);

            Assert.Equal(3.344m, order.TotalDiscount);
            Assert.Equal(0.636m, order.TotalDiscountVAT);
        }
        #endregion
    }
}
