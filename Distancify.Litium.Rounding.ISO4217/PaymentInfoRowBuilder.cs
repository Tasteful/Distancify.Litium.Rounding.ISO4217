using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Litium.Foundation.Modules.ECommerce.Carriers;
using Litium.Globalization;

namespace Distancify.Litium.Rounding.ISO4217
{
    /// <summary>
    /// Use this service to create PaymentInfoRowCarrier instead of it's constructor, as the constructors puts erroneous roundings to sums.
    /// </summary>
    public static class PaymentInfoRowBuilder
    {
        public static PaymentInfoRowCarrier Build(OrderRowCarrier row, Guid paymentInfoID, int index)
        {
            var result = new PaymentInfoRowCarrier(row, paymentInfoID, index);
            result.TotalPrice = Math.Abs(row.TotalPrice);
            result.TotalVatAmount = Math.Abs(row.TotalVATAmount);
            return result;
        }

        public static PaymentInfoRowCarrier Build(DeliveryCarrier delivery, Guid paymentInfoID, int index)
        {
            return Build(delivery, null, paymentInfoID, index);
        }

        public static PaymentInfoRowCarrier Build(DeliveryCarrier delivery, string description, Guid paymentInfoID, int index)
        {
            var result = new PaymentInfoRowCarrier(delivery, description, paymentInfoID, index);
            result.TotalPrice = result.TotalPriceWithoutRounding = Math.Abs(delivery.TotalDeliveryCost);
            result.TotalVatAmount = result.TotalVatAmountWithoutRounding = Math.Abs(delivery.TotalVATAmount);
            return result;
        }

        public static PaymentInfoRowCarrier Build(FeeCarrier fee, Guid paymentInfoID, int index)
        {
            var result = new PaymentInfoRowCarrier(fee, paymentInfoID, index);
            result.TotalPrice = result.TotalPriceWithoutRounding = Math.Abs(fee.TotalAmount);
            result.TotalVatAmount = result.TotalVatAmountWithoutRounding = Math.Abs(fee.TotalVATAmount);
            return result;
        }

        public static PaymentInfoRowCarrier Build(OrderDiscountCarrier discount, Guid paymentInfoID, int index)
        {
            var result = new PaymentInfoRowCarrier(discount, paymentInfoID, index);
            result.TotalPrice = result.TotalPriceWithoutRounding = Math.Abs(discount.DiscountAmount) * -1;
            result.TotalVatAmount = result.TotalVatAmountWithoutRounding = Math.Abs(discount.VATAmount) * -1;
            return result;
        }
    }
}
