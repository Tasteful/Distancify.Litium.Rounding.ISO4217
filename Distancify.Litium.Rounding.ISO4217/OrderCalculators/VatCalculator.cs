using System;
using System.Linq;
using Litium.Foundation.Modules.ECommerce.Carriers;
using Litium.Foundation.Modules.ECommerce.Plugins.Vat;
using Litium.Foundation.Security;
using Litium.Globalization;
using Litium.Owin.InversionOfControl;

namespace Distancify.Litium.Rounding.ISO4217.OrderCalculators
{
    [Plugin("Default")]
    public class VatCalculator : IVatCalculator
    {
        private readonly CurrencyService currencyService;

        public VatCalculator(CurrencyService currencyService)
        {
            this.currencyService = currencyService;
        }

        public virtual void CalculateFromCarrier(OrderCarrier orderCarrier, SecurityToken token)
        {
            CalculateTotalOrderRowsVat(orderCarrier);
            CalculateTotalDeliveryVat(orderCarrier);
            CalculateTotalFeeVat(orderCarrier);
            CalculateTotalDiscount(orderCarrier);

            orderCarrier.TotalVAT =
                orderCarrier.TotalOrderRowVAT +
                orderCarrier.TotalFeeVAT +
                orderCarrier.TotalDeliveryCostVAT -
                orderCarrier.TotalDiscountVAT;
        }

        public virtual void CalculateTotalOrderRowsVat(OrderCarrier orderCarrier)
        {
            var totalOrderRowVAT = 0m;
            using (CalculatorContext.Use(orderCarrier))
            {
                foreach (var row in orderCarrier.OrderRows.Where(r => !r.CarrierState.IsMarkedForDeleting))
                {
                    CalculateOrderRowVat(orderCarrier, row);
                    totalOrderRowVAT += row.TotalVATAmount;
                }
            }
            orderCarrier.TotalOrderRowVAT = totalOrderRowVAT;
        }

        public virtual void CalculateOrderRowVat(OrderRowCarrier orderRowCarrier)
        {
            CalculateOrderRowVat(null, orderRowCarrier);
        }

        public virtual void CalculateOrderRowVat(OrderCarrier order, OrderRowCarrier orderRowCarrier)
        {
            var currency = GetCurrency(order);

            var price = orderRowCarrier.CampaignID != Guid.Empty
                ? orderRowCarrier.UnitCampaignPrice
                : orderRowCarrier.UnitListPrice;

            price -= orderRowCarrier.DiscountAmount / orderRowCarrier.Quantity;

            orderRowCarrier.TotalVATAmount = Math.Round(price * orderRowCarrier.VATPercentage, currency.GetDecimals()) * orderRowCarrier.Quantity;
        }

        public virtual void CalculateTotalFeeVat(OrderCarrier orderCarrier)
        {
            Currency currency = GetCurrency(orderCarrier);
            decimal? vatPercentage = GetAverageVatPercentage(orderCarrier);

            decimal orderTotalFee = 0;
            decimal orderTotalFeeVat = 0;
            foreach (var fee in orderCarrier.Fees.Where(r => !r.CarrierState.IsMarkedForDeleting))
            {
                if (vatPercentage != null)
                {
                    fee.VATPercentage = (decimal)vatPercentage;
                }
                if (fee.KeepAmountWithVATConstant)
                {
                    var totalAmount = fee.AmountWithVAT / (1 + fee.VATPercentage) - fee.DiscountAmount;
                    fee.TotalAmount = Math.Round(totalAmount, currency.GetDecimals());
                    fee.TotalVATAmount = Math.Round(fee.TotalAmount * fee.VATPercentage, currency.GetDecimals());
                }
                else
                {
                    fee.TotalAmount = Math.Round(fee.Amount - fee.DiscountAmount, currency.GetDecimals());
                    fee.TotalVATAmount = Math.Round(fee.TotalAmount * fee.VATPercentage, currency.GetDecimals());
                }
                orderTotalFee += fee.TotalAmount;
                orderTotalFeeVat += fee.TotalVATAmount;
            }
            orderCarrier.TotalFee = orderTotalFee;
            orderCarrier.TotalFeeVAT = orderTotalFeeVat;
        }

        public virtual void CalculateTotalDeliveryVat(OrderCarrier orderCarrier)
        {
            Currency currency = GetCurrency(orderCarrier);
            decimal? vatPercentage = GetAverageVatPercentage(orderCarrier);

            decimal orderTotalDelivery = 0;
            decimal orderTotalDeliveryVat = 0;
            foreach (var delivery in orderCarrier.Deliveries.Where(r => !r.CarrierState.IsMarkedForDeleting))
            {
                if (vatPercentage != null)
                {
                    delivery.VATPercentage = (decimal)vatPercentage;
                }
                if (delivery.KeepDeliveryCostWithVatConstant)
                {
                    var totalDeliveryCost = delivery.DeliveryCostWithVAT / (1 + delivery.VATPercentage) - delivery.DiscountAmount;
                    delivery.TotalDeliveryCost = Math.Round(totalDeliveryCost, currency.GetDecimals());
                    delivery.TotalVATAmount = Math.Round(delivery.TotalDeliveryCost * delivery.VATPercentage, currency.GetDecimals());
                }
                else
                {
                    delivery.TotalDeliveryCost = Math.Round(delivery.DeliveryCost - delivery.DiscountAmount, currency.GetDecimals());
                    delivery.TotalVATAmount = Math.Round(delivery.TotalDeliveryCost * delivery.VATPercentage, currency.GetDecimals());
                }
                orderTotalDelivery += delivery.TotalDeliveryCost;
                orderTotalDeliveryVat += delivery.TotalVATAmount;
            }
            orderCarrier.TotalDeliveryCost = orderTotalDelivery;
            orderCarrier.TotalDeliveryCostVAT = orderTotalDeliveryVat;
        }

        public virtual void CalculateTotalDiscount(OrderCarrier orderCarrier)
        {
            Currency currency = GetCurrency(orderCarrier);
            decimal? vatPercentage = GetAverageVatPercentage(orderCarrier);

            decimal orderTotalDiscount = 0;
            decimal orderTotalDiscountVat = 0;
            foreach (var discount in orderCarrier.OrderDiscounts.Where(r => !r.CarrierState.IsMarkedForDeleting))
            {
                if (vatPercentage != null)
                {
                    discount.VATPercentage = (decimal)vatPercentage;
                }
                if (discount.DiscountAmountWithVAT > 0)
                {
                    discount.DiscountAmountWithVAT = Math.Round(discount.DiscountAmountWithVAT, currency.GetDecimals());
                    discount.DiscountAmount = Math.Round(discount.DiscountAmountWithVAT / (1 + discount.VATPercentage), currency.GetDecimals());
                }
                else
                {
                    discount.DiscountAmount = Math.Round(discount.DiscountAmount, currency.GetDecimals());
                    discount.DiscountAmountWithVAT = Math.Round(discount.DiscountAmount * (1 + discount.VATPercentage), currency.GetDecimals());
                }
                discount.VATAmount = discount.DiscountAmountWithVAT - discount.DiscountAmount;
                orderTotalDiscount += discount.DiscountAmount;
                orderTotalDiscountVat += discount.VATAmount;
            }
            orderCarrier.TotalDiscount = orderTotalDiscount;
            orderCarrier.TotalDiscountVAT = orderTotalDiscountVat;
        }

        private Currency GetCurrency(OrderCarrier orderCarrier)
        {
            if (orderCarrier == null)
            {
                orderCarrier = CalculatorContext.GetCurrentOrderCarrier();
            }
            Currency currency = null;
            if (orderCarrier != null)
            {
                currency = currencyService.Get(orderCarrier.CurrencyID);
            }
            if (currency == null)
            {
                currency = new Currency(string.Empty);
            }
            return currency;
        }

        private decimal? GetAverageVatPercentage(OrderCarrier orderCarrier)
        {
            decimal? vatPercentage = null;
            var rows = orderCarrier.OrderRows.Where(r => !r.CarrierState.IsMarkedForDeleting).ToList();
            if (rows.Count > 0)
            {
                var totalPrice = rows.Sum(r => r.TotalPrice);
                var totalVat = rows.Sum(r => r.TotalPrice * r.VATPercentage);
                if (totalPrice != 0)
                    vatPercentage = totalVat / totalPrice;
            }
            return vatPercentage;
        }
    }
}
