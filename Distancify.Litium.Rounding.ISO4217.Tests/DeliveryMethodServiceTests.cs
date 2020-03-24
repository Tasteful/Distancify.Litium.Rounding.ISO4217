using Distancify.Litium.Rounding.ISO4217.Tests.Utils;
using Litium;
using Litium.FieldFramework;
using Litium.Foundation;
using Litium.Foundation.Modules.ECommerce;
using Litium.Foundation.Modules.ECommerce.Carriers;
using Litium.Globalization;
using System;
using Xunit;

namespace Distancify.Litium.Rounding.ISO4217.Tests
{
    public class DeliveryMethodServiceTests : LitiumApplicationTestBase
    {
        [Fact]
        public void GetDisplayNameBasedOnChannelsLanguage()
        {
            var methodId = Guid.NewGuid();

            using (Solution.Instance.SystemToken.Use())
            {
                var language = IoC.Resolve<LanguageService>().Get("en-US");

                var method = ModuleECommerce.Instance.DeliveryMethods.Get("Standard", Solution.Instance.SystemToken)?.GetAsCarrier();
                if (method == null)
                {
                    method = new DeliveryMethodCarrier();
                    method.ID = methodId;
                    method.Name = "Standard";
                    method.Translations.Add(new DeliveryMethodTranslationCarrier
                    {
                        LanguageID = language.SystemId,
                        DisplayName = "DisplayName"
                    });

                    ModuleECommerce.Instance.DeliveryMethods.Create(method, Solution.Instance.SystemToken);
                }

                var channelFieldTemplate = IoC.Resolve<FieldTemplateService>().Get<ChannelFieldTemplate>("Default");
                if (channelFieldTemplate == null)
                {
                    channelFieldTemplate = new ChannelFieldTemplate("Default");
                    IoC.Resolve<FieldTemplateService>().Create(channelFieldTemplate);
                }

                var channel = IoC.Resolve<ChannelService>().Get("Default");
                if (channel == null)
                {
                    channel = new Channel(channelFieldTemplate.SystemId);
                    channel.Id = "Default";
                    channel.WebsiteLanguageSystemId = language.SystemId;
                    IoC.Resolve<ChannelService>().Create(channel);
                }

                var sut = IoC.Resolve<IDeliveryMethodService>();
                var result = sut.GetPaymentInfoDescription(methodId, channel.SystemId);

                Assert.Equal("DisplayName", result);
            }
        }
    }
}
