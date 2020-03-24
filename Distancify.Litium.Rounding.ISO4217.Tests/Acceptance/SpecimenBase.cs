using Distancify.Litium.Rounding.ISO4217.Tests.Utils;
using Litium;
using Litium.Foundation;
using Litium.Foundation.Modules.ECommerce;
using Litium.Foundation.Modules.ECommerce.Carriers;
using Litium.Foundation.Modules.ECommerce.Plugins.Orders;
using Litium.Foundation.Modules.ECommerce.Plugins.Payments;
using Litium.Globalization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Distancify.Litium.Rounding.ISO4217.Tests.Acceptance
{
    public abstract class SpecimenBase : LitiumApplicationTestBase
    {
        protected readonly Guid CurrencySystemId;
        protected readonly Guid DeliveryMethodId = Guid.Parse("1CC1572A-E33D-4070-B0B9-26A9CC698900");

        public SpecimenBase()
        {
            CurrencySystemId = EnsureCurrency();
            EnsureDeliveryMethod();
        }

        protected abstract OrderCarrier Order();

        [Fact]
        public void LegacyImplementationIsInvalid()
        {
            var order = Order();

            var languageService = IoC.Resolve<LanguageService>();
            using (Solution.Instance.SystemToken.Use())
            {
                if (languageService.Get(CultureInfo.CurrentUICulture) == null)
                    languageService.Create(new Language(CultureInfo.CurrentUICulture));
            }

            var sut = new global::Litium.Foundation.Modules.ECommerce.Plugins.Payments.PaymentInfoRowFactory(languageService);

            var result = sut.Create(order, null);

            var rounding = result.Single().Rows
                .Where(r => r.ReferenceType == global::Litium.Foundation.Modules.ECommerce.Payments.PaymentInfoRowType.RoundingOffAdjustment)
                .Select(r => r.TotalAmountWithVAT)
                .Single();
            Assert.NotEqual(0, rounding);
        }

        [Fact]
        public void NewImplementationIsCorrect()
        {
            var order = Order();

            IoC.Resolve<IOrderCalculator>().Calculate(order, true, Solution.Instance.SystemToken);

            var sut = IoC.Resolve<IPaymentInfoCalculator>();

            sut.CalculateFromCarrier(order, null);

            AssertGrandTotalAndPaymentInfoMatches(order);

            Assert.Empty(order.PaymentInfo.Single().Rows
                .Where(r => r.ReferenceType == global::Litium.Foundation.Modules.ECommerce.Payments.PaymentInfoRowType.RoundingOffAdjustment)
                .Select(r => r.TotalAmountWithVAT));
        }

        protected abstract string CurrencyId { get; }

        protected abstract ICollection<DeliveryMethodCostCarrier> DeliveryMethodCosts { get; }

        private void AssertGrandTotalAndPaymentInfoMatches(OrderCarrier orderCarrier)
        {
            Assert.Equal(orderCarrier.GrandTotal, orderCarrier.PaymentInfo.Where(r => !r.CarrierState.IsMarkedForDeleting).Sum(r => r.TotalAmountWithVAT));
        }

        private Guid EnsureCurrency()
        {
            var currencyService = IoC.Resolve<CurrencyService>();
            var currency = currencyService.Get(CurrencyId);
            if (currency == null)
            {
                using (Solution.Instance.SystemToken.Use())
                {
                    currency = new Currency(CurrencyId);
                    currency.SystemId = Guid.NewGuid();
                    currencyService.Create(currency);
                }
            }

            return currency.SystemId;
        }

        private void EnsureDeliveryMethod()
        {
            if (ModuleECommerce.Instance.DeliveryMethods.Get(DeliveryMethodId, Solution.Instance.SystemToken) == null)
            {
                ModuleECommerce.Instance.DeliveryMethods.Create(new DeliveryMethodCarrier
                {
                    ID = DeliveryMethodId,
                    Name = "DeliveryMethod",
                    Costs = DeliveryMethodCosts
                }, Solution.Instance.SystemToken);
            }
        }
    }
}
