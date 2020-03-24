using Litium.Foundation.Modules.ECommerce.Carriers;
using Litium.Foundation.Modules.ECommerce.Plugins.Campaigns;
using Litium.Foundation.Modules.ECommerce.Plugins.Deliveries;
using Litium.Foundation.Modules.ECommerce.Plugins.Fees;
using Litium.Foundation.Modules.ECommerce.Plugins.Orders;
using Litium.Foundation.Modules.ECommerce.Plugins.Vat;
using Litium.Foundation.Security;
using NSubstitute;
using Xunit;

namespace Distancify.Litium.Rounding.ISO4217.Tests
{
    public class OrderCalculatorTests
    {
        // TODO: Make sure OrderCalculator sets the current order context in the thread
        // as calculations are made. This solved the issue with CalculateOrderRowTotal
        // methods not knowing what the order is

        [Fact]
        public void SetContext()
        {
            var order = new OrderCarrier();

            var deliveryCostCalculator = Substitute.For<IDeliveryCostCalculator>();
            deliveryCostCalculator.When(r => r.CalculateFromCarrier(order, Arg.Any<SecurityToken>()))
                .Do(ci => Assert.Same(order, CalculatorContext.GetCurrentOrderCarrier()));
            var feesCalculator = Substitute.For<IFeesCalculator>();
            feesCalculator.When(r => r.CalculateFromCarrier(order, Arg.Any<SecurityToken>()))
                .Do(ci => Assert.Same(order, CalculatorContext.GetCurrentOrderCarrier()));
            var orderTotalCalculator = Substitute.For<IOrderTotalCalculator>();
            orderTotalCalculator.When(r => r.CalculateFromCarrier(order, Arg.Any<SecurityToken>()))
                .Do(ci => Assert.Same(order, CalculatorContext.GetCurrentOrderCarrier()));
            var campaignCalculator = Substitute.For<ICampaignCalculator>();
            campaignCalculator.When(r => r.CalculateFromCarrier(order, Arg.Any<SecurityToken>()))
                .Do(ci => Assert.Same(order, CalculatorContext.GetCurrentOrderCarrier()));
            var vatCalculator = Substitute.For<IVatCalculator>();
            vatCalculator.When(r => r.CalculateFromCarrier(order, Arg.Any<SecurityToken>()))
                .Do(ci => Assert.Same(order, CalculatorContext.GetCurrentOrderCarrier()));
            var orderGrandTotalCalculator = Substitute.For<IOrderGrandTotalCalculator>();
            orderGrandTotalCalculator.When(r => r.Calculate(order, Arg.Any<SecurityToken>()))
                .Do(ci => Assert.Same(order, CalculatorContext.GetCurrentOrderCarrier()));

            var sut = new OrderCalculators.OrderCalculator(
                deliveryCostCalculator,
                feesCalculator,
                orderTotalCalculator,
                campaignCalculator,
                vatCalculator,
                orderGrandTotalCalculator);

            sut.Calculate(order, true, null);

            Assert.Null(CalculatorContext.GetCurrentOrderCarrier());
            deliveryCostCalculator.Received().CalculateFromCarrier(order, Arg.Any<SecurityToken>());
            feesCalculator.Received().CalculateFromCarrier(order, Arg.Any<SecurityToken>());
            orderTotalCalculator.Received().CalculateFromCarrier(order, Arg.Any<SecurityToken>());
            campaignCalculator.Received().CalculateFromCarrier(order, Arg.Any<SecurityToken>());
            vatCalculator.Received().CalculateFromCarrier(order, Arg.Any<SecurityToken>());
            orderGrandTotalCalculator.Received().Calculate(order, Arg.Any<SecurityToken>());
        }
    }
}
