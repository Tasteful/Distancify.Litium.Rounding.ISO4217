using System;
using Litium.Foundation.Modules.ECommerce.Carriers;
using Litium.Foundation.Modules.ECommerce.Plugins.Orders;
using Litium.Foundation.Security;
using Litium.Globalization;
using Litium.Owin.InversionOfControl;

namespace Distancify.Litium.Rounding.ISO4217.OrderCalculators
{
    [Plugin("Default")]
    public class OrderTotalCalculator : IOrderTotalCalculator
    {
        private readonly CurrencyService currencyService;

        public OrderTotalCalculator(
            CurrencyService currencyService)
        {
            this.currencyService = currencyService;
        }

        public virtual void CalculateFromCarrier(OrderCarrier orderCarrier, SecurityToken token)
        {
            var currency = currencyService.Get(orderCarrier.CurrencyID);

            var total = 0m;
            foreach (var r in orderCarrier.OrderRows)
            {
                CalculateOrderRowTotal(r, currency);

                total += r.TotalPrice;
            }

            orderCarrier.TotalOrderRow = total;
        }

        public virtual void CalculateOrderRowTotal(OrderRowCarrier orderRowCarrier)
        {
            var order = CalculatorContext.GetCurrentOrderCarrier();
            Currency currency = null;
            if (order != null)
            {
                currency = currencyService.Get(order.CurrencyID);
            }

            if (currency == null)
            {
                currency = new Currency(string.Empty);
            }

            CalculateOrderRowTotal(orderRowCarrier, currency);
        }

        protected virtual void CalculateOrderRowTotal(OrderRowCarrier orderRowCarrier, Currency currency)
        {
            var price = orderRowCarrier.CampaignID != Guid.Empty
                ? orderRowCarrier.UnitCampaignPrice
                : orderRowCarrier.UnitListPrice;

            var unitDiscount = GetUnitDiscountAmount(currency, price, orderRowCarrier.DiscountPercentage);
            orderRowCarrier.DiscountAmount = unitDiscount * orderRowCarrier.Quantity;
            orderRowCarrier.TotalPrice = Math.Round(price - unitDiscount, currency.GetDecimals()) * orderRowCarrier.Quantity;
        }

        /// <summary>
        /// Calculates the discount amount for a single unit. Override this to apply custom discount rounding.
        /// </summary>
        /// <param name="currency">Order's currency</param>
        /// <param name="unitPrice">Row's unit listing or campaign price depending on whether row has a campaign</param>
        /// <param name="quantity">Row quantity</param>
        /// <param name="discountPercentage">Discount percentage (not that this is actual percent, not a factor). So 25% is passed in as "25", not "0.25m"</param>
        /// <returns></returns>
        protected virtual decimal GetUnitDiscountAmount(Currency currency, decimal unitPrice, decimal discountPercentage)
        {
            return unitPrice * (discountPercentage * 0.01m);
        }
    }
}
