using Litium.Foundation.Modules.ECommerce.Carriers;
using Litium.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Distancify.Litium.Rounding.ISO4217.Tests
{
    public class PaymentInfoRowBuilderTests
    {
        private readonly Currency currency;

        public PaymentInfoRowBuilderTests()
        {
            currency = new Currency("IQD");
        }

        [Theory]
        [InlineData(1.999999, 1.888888, 1.999999, 1.888888)]
        [InlineData(-1.999999, -1.888888, 1.999999, 1.888888)]
        public void Build_OrderRowCarrier(decimal totalPrice, decimal totalVat, decimal expectedPrice, decimal expectedVat)
        {
            var row = new OrderRowCarrier
            {
                // We assume the TotalPrice was rounded by OrderTotalCalculator
                TotalPrice = totalPrice,
                TotalVATAmount = totalVat,
                VATPercentage = 0.19m
            };
            var paymentInfoID = Guid.NewGuid();

            var result = PaymentInfoRowBuilder.Build(row, paymentInfoID, 1);

            Assert.Equal(expectedPrice, result.TotalPrice);
            Assert.Equal(expectedVat, result.TotalVatAmount);
        }

        [Theory]
        [InlineData(1.999999, 1.888888, 1.999999, 1.888888)]
        [InlineData(-1.999999, -1.888888, 1.999999, 1.888888)]
        public void Build_DeliveryCarrier(decimal totalPrice, decimal totalVat, decimal expectedPrice, decimal expectedVat)
        {
            var delivery = new DeliveryCarrier
            {
                // We assume the TotalPrice was rounded by OrderTotalCalculator
                TotalDeliveryCost = totalPrice,
                TotalVATAmount = totalVat
            };
            var paymentInfoID = Guid.NewGuid();

            var result = PaymentInfoRowBuilder.Build(delivery, paymentInfoID, 1);

            Assert.Equal(expectedPrice, result.TotalPrice);
            Assert.Equal(expectedVat, result.TotalVatAmount);
        }

        [Theory]
        [InlineData(1.999999, 1.888888, 1.999999, 1.888888)]
        [InlineData(-1.999999, -1.888888, 1.999999, 1.888888)]
        public void Build_DeliveryCarrier_WithDescription(decimal totalPrice, decimal totalVat, decimal expectedPrice, decimal expectedVat)
        {
            var delivery = new DeliveryCarrier
            {
                // We assume the TotalPrice was rounded by OrderTotalCalculator
                TotalDeliveryCost = totalPrice,
                TotalVATAmount = totalVat
            };
            var paymentInfoID = Guid.NewGuid();

            var result = PaymentInfoRowBuilder.Build(delivery, "description", paymentInfoID, 1);

            Assert.Equal(expectedPrice, result.TotalPrice);
            Assert.Equal(expectedVat, result.TotalVatAmount);
            Assert.Equal("description", result.Description);
        }

        [Theory]
        [InlineData(1.999999, 1.888888, 1.999999, 1.888888)]
        [InlineData(-1.999999, -1.888888, 1.999999, 1.888888)]
        public void Build_FeeCarrier(decimal totalPrice, decimal totalVat, decimal expectedPrice, decimal expectedVat)
        {
            var fee = new FeeCarrier
            {
                // We assume the TotalPrice was rounded by OrderTotalCalculator
                TotalAmount = totalPrice,
                TotalVATAmount = totalVat
            };
            var paymentInfoID = Guid.NewGuid();

            var result = PaymentInfoRowBuilder.Build(fee, paymentInfoID, 1);

            Assert.Equal(expectedPrice, result.TotalPrice);
            Assert.Equal(expectedVat, result.TotalVatAmount);
        }

        [Theory]
        [InlineData(1.999999, 1.888888, -1.999999, -1.888888)]
        [InlineData(-1.999999, -1.888888, -1.999999, -1.888888)]
        public void Build_OrderDiscountCarrier(decimal totalPrice, decimal totalVat, decimal expectedPrice, decimal expectedVat)
        {
            var discount = new OrderDiscountCarrier
            {
                // We assume the TotalPrice was rounded by OrderTotalCalculator
                DiscountAmount = totalPrice,
                VATAmount = totalVat
            };
            var paymentInfoID = Guid.NewGuid();

            var result = PaymentInfoRowBuilder.Build(discount, paymentInfoID, 1);

            Assert.Equal(expectedPrice, result.TotalPrice);
            Assert.Equal(expectedVat, result.TotalVatAmount);
        }
    }
}
