using Litium.Foundation.Modules.ECommerce.Carriers;
using Litium.Foundation.Modules.ECommerce.Plugins.Campaigns;
using Litium.Foundation.Security;
using Litium.Owin.InversionOfControl;

namespace Distancify.Litium.Rounding.ISO4217.Tests.Utils
{
    /// <summary>
    /// This funky class is here because the default CampaignCalculator in Litium does not have a Plugin attribute,
    /// so it causes conflicts with NSubstitute when tests are run using vstest.console.exe (as in build servers)
    /// as vstest.console.exe does not fully isolate each test. All tests runs sequentially in the same process.
    /// </summary>
    [Plugin("Default")]
    public class DefaultCampaignCalculator : CampaignCalculator
    {
        public DefaultCampaignCalculator(ICampaignProcessor campaignProcessor, ICampaignHandler campaignHandler) : base(campaignProcessor, campaignHandler)
        {
        }

        public override void CalculateFromCarrier(OrderCarrier orderCarrier, SecurityToken securityToken)
        {
            base.CalculateFromCarrier(orderCarrier, securityToken);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override void HandleOrderConfirmation(OrderCarrier carrier, SecurityToken securityToken)
        {
            base.HandleOrderConfirmation(carrier, securityToken);
        }

        public override void RemoveCampaigns(OrderCarrier orderCarrier, SecurityToken securityToken)
        {
            base.RemoveCampaigns(orderCarrier, securityToken);
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
