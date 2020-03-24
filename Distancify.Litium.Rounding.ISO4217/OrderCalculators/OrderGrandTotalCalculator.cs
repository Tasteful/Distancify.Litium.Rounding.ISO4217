using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Litium.Foundation.Modules.ECommerce.Carriers;
using Litium.Foundation.Security;

namespace Distancify.Litium.Rounding.ISO4217.OrderCalculators
{
    public class OrderGrandTotalCalculator : global::Litium.Foundation.Modules.ECommerce.Plugins.Orders.OrderGrandTotalCalculator
    {
        public override void Calculate(OrderCarrier orderCarrier, SecurityToken securityToken)
        {
            base.Calculate(orderCarrier, securityToken);

            var vat = GetAverageVatPercentage(orderCarrier);
            if (vat != null)
            {
                orderCarrier.OverallVatPercentage = (decimal)vat;
            }
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
