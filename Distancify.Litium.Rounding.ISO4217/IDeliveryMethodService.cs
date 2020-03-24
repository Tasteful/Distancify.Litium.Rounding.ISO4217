using Litium.Runtime.DependencyInjection;
using System;

namespace Distancify.Litium.Rounding.ISO4217
{
    public interface IDeliveryMethodService
    {
        string GetPaymentInfoDescription(Guid deliveryMethodId, Guid channelId);
    }
}
