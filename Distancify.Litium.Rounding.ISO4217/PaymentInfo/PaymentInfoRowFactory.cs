using Litium.Foundation.Modules.ECommerce.Carriers;
using Litium.Foundation.Modules.ECommerce.Plugins.Payments;
using Litium.Foundation.Security;
using Litium.Owin.InversionOfControl;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Distancify.Litium.Rounding.ISO4217.PaymentInfo
{
    /// <summary>
    /// Replacement of default IPaymentInfoRowFactory. Will add all rows, fees, delivery costs and discounts to the first payment info of the order.
    /// </summary>
    [Plugin("Default")]
    public class PaymentInfoRowFactory : IPaymentInfoRowFactory
    {
        private readonly IDeliveryMethodService deliveryMethodService;

        public PaymentInfoRowFactory(IDeliveryMethodService deliveryMethodService)
        {
            this.deliveryMethodService = deliveryMethodService;
        }

        public virtual IEnumerable<PaymentInfoCarrier> Create(OrderCarrier orderCarrier, SecurityToken token)
        {
            var pi = orderCarrier.PaymentInfo.Where(r => !r.CarrierState.IsMarkedForDeleting).FirstOrDefault();

            if (pi == null) return new PaymentInfoCarrier[0];

            foreach (var row in pi.Rows.Where(r => !r.CarrierState.IsMarkedForCreating))
            {
                row.CarrierState.IsMarkedForDeleting = true;
            }
            pi.Rows.RemoveAll(r => r.CarrierState.IsMarkedForCreating);

            int index = 1;
            pi.Rows.AddRange(orderCarrier.OrderRows.Select(r => PaymentInfoRowBuilder.Build(r, pi.ID, index++)));
            pi.Rows.AddRange(orderCarrier.Deliveries.Select(r => Build(r, pi.ID, index++, orderCarrier.ChannelID)));
            pi.Rows.AddRange(orderCarrier.Fees.Select(r => PaymentInfoRowBuilder.Build(r, pi.ID, index++)));

            var leftForDiscount = GetOrderTotal(orderCarrier);
            foreach (var discount in orderCarrier.OrderDiscounts.FindAll(item => !item.CarrierState.IsMarkedForDeleting))
            {
                var row = PaymentInfoRowBuilder.Build(discount, pi.ID, index++);

                decimal discountAmount = discount.DiscountAmount;
                if (discount.DiscountAmount > leftForDiscount)
                {
                    discountAmount = Math.Min(leftForDiscount, discount.DiscountAmount);
                    row.TotalPrice = row.TotalPriceWithoutRounding = Math.Abs(discountAmount) * -1;
                    row.TotalVatAmount = row.TotalVatAmountWithoutRounding = row.TotalPrice * discount.VATPercentage;
                }

                leftForDiscount = leftForDiscount + row.TotalPrice;

                pi.Rows.Add(row);
            }

            return new PaymentInfoCarrier[] { pi };
        }

        private PaymentInfoRowCarrier Build(DeliveryCarrier delivery, Guid paymentInfoId, int index, Guid channelId)
        {
            var description = deliveryMethodService.GetPaymentInfoDescription(delivery.DeliveryMethodID, channelId);
            return PaymentInfoRowBuilder.Build(delivery, description, paymentInfoId, index);
        }

        private decimal GetOrderTotal(OrderCarrier orderCarrier)
        {
            return
                orderCarrier.OrderRows.Where(r => !r.CarrierState.IsMarkedForDeleting).Sum(r => r.TotalPrice) +
                orderCarrier.Fees.Where(r => !r.CarrierState.IsMarkedForDeleting).Sum(r => r.TotalAmount) +
                orderCarrier.Deliveries.Where(r => !r.CarrierState.IsMarkedForDeleting).Sum(r => r.TotalDeliveryCost);
        }
    }
}
