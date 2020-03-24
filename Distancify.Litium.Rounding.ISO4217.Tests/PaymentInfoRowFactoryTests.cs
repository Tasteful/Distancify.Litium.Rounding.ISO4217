using Distancify.Litium.Rounding.ISO4217.PaymentInfo;
using Litium.Foundation.Modules.ECommerce;
using Litium.Foundation.Modules.ECommerce.Carriers;
using Litium.Foundation.Modules.ECommerce.Deliveries;
using Litium.Foundation.Security;
using Litium.Globalization;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Distancify.Litium.Rounding.ISO4217.Tests
{
    public class PaymentInfoRowFactoryTests
    {
        private readonly PaymentInfoRowFactory sut;
        private readonly IDeliveryMethodService deliveryMethodService;

        public PaymentInfoRowFactoryTests()
        {
            deliveryMethodService = Substitute.For<IDeliveryMethodService>();
            deliveryMethodService.GetPaymentInfoDescription(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns("DeliveryDisplayName");

            sut = new PaymentInfoRowFactory(deliveryMethodService);
        }

        [Fact]
        public void SkipMarkedForDeletion()
        {
            var order = new OrderCarrier
            {
                PaymentInfo = new List<PaymentInfoCarrier>
                {
                    new PaymentInfoCarrier
                    {
                        CarrierState = new CarrierState
                        {
                            IsMarkedForCreating = false,
                            IsMarkedForDeleting = true
                        }
                    }
                }
            };

            Assert.Empty(sut.Create(order, null));
        }

        [Fact]
        public void MarkExistingRowsForDeleting()
        {
            var row = new PaymentInfoRowCarrier
            {
                CarrierState = new CarrierState
                {
                    IsMarkedForCreating = false
                }
            };

            var order = new OrderCarrier
            {
                PaymentInfo = new List<PaymentInfoCarrier>
                {
                    new PaymentInfoCarrier
                    {
                        Rows = new List<PaymentInfoRowCarrier>
                        {
                            row
                        }
                    }
                }
            };


            sut.Create(order, null).ToList();

            Assert.True(row.CarrierState.IsMarkedForDeleting);
        }

        [Fact]
        public void RemoveTransientRows()
        {
            var row = new PaymentInfoRowCarrier();

            var order = new OrderCarrier
            {
                PaymentInfo = new List<PaymentInfoCarrier>
                {
                    new PaymentInfoCarrier
                    {
                        Rows = new List<PaymentInfoRowCarrier>
                        {
                            new PaymentInfoRowCarrier(),
                            new PaymentInfoRowCarrier
                            {
                                CarrierState = new CarrierState
                                {
                                    IsMarkedForCreating = false
                                } 
                            }
                        }
                    }
                }
            };


            sut.Create(order, null).ToList();

            Assert.Single(order.PaymentInfo.Single().Rows);
        }

        [Fact]
        public void AddOrderRows()
        {
            var paymentInfo = new PaymentInfoCarrier();
            var order = new OrderCarrier();
            order.PaymentInfo.Add(paymentInfo);
            order.OrderRows.Add(new OrderRowCarrier
            {
                TotalPrice = 1.999999m,
                TotalVATAmount = 1.888888m
            });

            sut.Create(order, null).ToList();
            var result = order.PaymentInfo.Single().Rows.Single();

            Assert.Equal(1.999999m, result.TotalPrice);
            Assert.Equal(1.888888m, result.TotalVatAmount);
        }

        [Fact]
        public void AddFees()
        {
            var paymentInfo = new PaymentInfoCarrier();
            var order = new OrderCarrier();
            order.PaymentInfo.Add(paymentInfo);
            order.Fees.Add(new FeeCarrier
            {
                TotalAmount = 1.999999m,
                TotalVATAmount = 1.888888m
            });

            sut.Create(order, null).ToList();
            var result = order.PaymentInfo.Single().Rows.Single();

            Assert.Equal(1.999999m, result.TotalPrice);
            Assert.Equal(1.888888m, result.TotalVatAmount);
        }

        [Fact]
        public void AddDeliveries()
        {
            var paymentInfo = new PaymentInfoCarrier();
            var order = new OrderCarrier();
            order.PaymentInfo.Add(paymentInfo);
            order.Deliveries.Add(new DeliveryCarrier
            {
                TotalDeliveryCost = 1.999999m,
                TotalVATAmount = 1.888888m
            });

            sut.Create(order, null).ToList();
            var result = order.PaymentInfo.Single().Rows.Single();

            Assert.Equal(1.999999m, result.TotalPrice);
            Assert.Equal(1.888888m, result.TotalVatAmount);
        }

        [Fact]
        public void SetDescriptionToDeliveryDisplayName()
        {
            var channelId = Guid.NewGuid();
            var deliveryMethodId = Guid.NewGuid();

            var paymentInfo = new PaymentInfoCarrier();
            var order = new OrderCarrier();
            order.ChannelID = channelId;
            order.PaymentInfo.Add(paymentInfo);
            order.Deliveries.Add(new DeliveryCarrier
            {
                DeliveryMethodID = deliveryMethodId,
                TotalDeliveryCost = 1.999999m,
                TotalVATAmount = 1.888888m
            });

            sut.Create(order, null).ToList();
            var result = order.PaymentInfo.Single().Rows.Single();

            Assert.Equal("DeliveryDisplayName", result.Description);
            deliveryMethodService.Received().GetPaymentInfoDescription(deliveryMethodId, channelId);
        }

        [Fact]
        public void AddDiscounts()
        {
            var paymentInfo = new PaymentInfoCarrier();
            var order = new OrderCarrier();
            order.PaymentInfo.Add(paymentInfo);
            order.OrderRows.Add(new OrderRowCarrier
            {
                TotalPrice = 1.999999m,
                TotalVATAmount = 1.888888m
            });
            order.OrderDiscounts.Add(new OrderDiscountCarrier
            {
                DiscountAmount = 1.999999m,
                VATAmount = 1.888888m
            });

            sut.Create(order, null).ToList();
            var result = order.PaymentInfo.Single().Rows.Last();

            Assert.Equal(-1.999999m, result.TotalPrice);
            Assert.Equal(-1.888888m, result.TotalVatAmount);
        }

        [Fact]
        public void SetsIndex()
        {
            var paymentInfo = new PaymentInfoCarrier();
            var order = new OrderCarrier();
            order.PaymentInfo.Add(paymentInfo);
            order.OrderRows.Add(new OrderRowCarrier
            {
                TotalPrice = 1.999999m,
                TotalVATAmount = 1.888888m
            });
            order.Fees.Add(new FeeCarrier
            {
                TotalAmount = 1.999999m,
                TotalVATAmount = 1.888888m
            });
            order.Deliveries.Add(new DeliveryCarrier
            {
                // We assume the TotalPrice was rounded by OrderTotalCalculator
                TotalDeliveryCost = 1.999999m,
                TotalVATAmount = 1.888888m
            });
            order.OrderDiscounts.Add(new OrderDiscountCarrier
            {
                // We assume the TotalPrice was rounded by OrderTotalCalculator
                DiscountAmount = 1.999999m,
                VATAmount = 1.888888m
            });

            sut.Create(order, null).ToList();
            var result = order.PaymentInfo.Single().Rows;

            Assert.Equal(1, result.First().Index);
            Assert.Equal(2, result.Skip(1).First().Index);
            Assert.Equal(3, result.Skip(2).First().Index);
            Assert.Equal(4, result.Skip(3).First().Index);
        }

        [Fact]
        public void PreventDiscountHigherThanOrderTotal()
        {
            var paymentInfo = new PaymentInfoCarrier();
            var order = new OrderCarrier();
            order.PaymentInfo.Add(paymentInfo);
            order.OrderRows.Add(new OrderRowCarrier
            {
                TotalPrice = 10m,
                TotalVATAmount = 2.5m
            });
            order.Fees.Add(new FeeCarrier
            {
                TotalAmount = 10m,
                TotalVATAmount = 2.5m
            });
            order.Deliveries.Add(new DeliveryCarrier
            {
                TotalDeliveryCost = 10m,
                TotalVATAmount = 2.5m
            });
            order.OrderDiscounts.Add(new OrderDiscountCarrier
            {
                DiscountAmount = 10m,
                VATAmount = 2.5m,
                VATPercentage = 0.25m
            });
            order.OrderDiscounts.Add(new OrderDiscountCarrier
            {
                DiscountAmount = 30m,
                VATAmount = 7.5m,
                VATPercentage = 0.25m
            });
            order.OrderDiscounts.Add(new OrderDiscountCarrier
            {
                DiscountAmount = 10m,
                VATAmount = 2.5m,
                VATPercentage = 0.25m
            });

            sut.Create(order, null).ToList();
            var result = order.PaymentInfo.Single().Rows;

            Assert.Equal(12.5m, result.First().TotalAmountWithVAT);
            Assert.Equal(12.5m, result.Skip(1).First().TotalAmountWithVAT);
            Assert.Equal(12.5m, result.Skip(2).First().TotalAmountWithVAT);
            Assert.Equal(-12.5m, result.Skip(3).First().TotalAmountWithVAT);
            Assert.Equal(-25m, result.Skip(4).First().TotalAmountWithVAT);
            Assert.Equal(0, result.Skip(5).First().TotalAmountWithVAT);
        }
    }
}
