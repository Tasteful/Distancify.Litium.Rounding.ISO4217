using Litium.Foundation.Modules.ECommerce;
using Litium.Foundation.Security;
using Litium.Globalization;
using Litium.Runtime.DependencyInjection;
using System;

namespace Distancify.Litium.Rounding.ISO4217.PaymentInfo
{
    [Service(FallbackService = true, ServiceType = typeof(IDeliveryMethodService))]
    public class DeliveryMethodService : IDeliveryMethodService
    {
        private readonly ChannelService channelService;
        private readonly ModuleECommerce moduleECommerce;

        public DeliveryMethodService(
            ChannelService channelService,
            ModuleECommerce moduleECommerce)
        {
            this.channelService = channelService;
            this.moduleECommerce = moduleECommerce;
        }

        public virtual string GetPaymentInfoDescription(Guid deliveryMethodId, Guid channelId)
        {
            var method = moduleECommerce.DeliveryMethods.Get(deliveryMethodId, SecurityToken.CurrentSecurityToken);
            if (method == null) return null;

            var channel = channelService.Get(channelId);
            if (channel == null || channel.WebsiteLanguageSystemId == null) return null;

            return method.GetDisplayName((Guid)channel.WebsiteLanguageSystemId);
        }
    }
}
