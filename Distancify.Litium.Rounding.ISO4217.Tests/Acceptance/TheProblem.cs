using System;
using Litium;
using Litium.Foundation;
using Litium.Foundation.Modules.ECommerce.Carriers;
using Litium.Globalization;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Distancify.Litium.Rounding.ISO4217.Tests.Utils;
using Litium.Foundation.Modules.ECommerce.Plugins.Campaigns;
using Litium.Foundation.Modules.ECommerce.Plugins.Campaigns.Actions;
using Litium.Foundation.Modules.ECommerce.Plugins.Orders;
using Xunit;

namespace Distancify.Litium.Rounding.ISO4217.Tests.Acceptance
{
    public class TheProblem : LitiumApplicationTestBase
    {
        [Fact]
        public void VerifyDefaultLitiumImplementationIsStillBroken()
        {
            // The gist of the problem is that when Litium is calculating paymeninfo rows, each row is rounded to two decimals.
            // This is just an assumption on Litium's part that all currencies (and therefor all payment providers) use two decimals.
            //
            // The problem is that when you round each row to two decimals, you end up with a different total than you would if you
            // added all sums and then rounded. This causes payment info total to sometimes not match the orders grand total.
            // Litium solves this by adding an adjustment fee to payment info. This however, causes other issues such as when
            // you export the order rows to a third system that doesn't do this exact adjustment. they will come to a conclusion that
            // the grand total is something else.

            var order = new OrderCarrier
            {
                GrandTotal = 857.6700m,
                TotalOrderRow = 686.1360m,
                TotalOrderRowVAT = 171.5340m,
                TotalVAT = 171.5340m,
                OrderRows = new List<OrderRowCarrier>
                {
                    new OrderRowCarrier
                    {
                        Quantity = 1,
                        TotalPrice = 129.4440m,
                        TotalVATAmount = 32.3610m,
                        UnitListPrice = 129.44400000m,
                        VATPercentage = 0.2500m
                    },
                    new OrderRowCarrier
                    {
                        Quantity = 1,
                        TotalPrice = 154.2840m,
                        TotalVATAmount = 38.5710m,
                        UnitListPrice = 154.28400000m,
                        VATPercentage = 0.2500m
                    },
                    new OrderRowCarrier
                    {
                        Quantity = 1,
                        TotalPrice = 192.9240m,
                        TotalVATAmount = 48.2310m,
                        UnitListPrice = 192.92400000m,
                        VATPercentage = 0.2500m
                    },
                    new OrderRowCarrier
                    {
                        Quantity = 1,
                        TotalPrice = 43.8840m,
                        TotalVATAmount = 10.9710m,
                        UnitListPrice = 43.88400000m,
                        VATPercentage = 0.2500m
                    },
                    new OrderRowCarrier
                    {
                        Quantity = 2,
                        TotalPrice = 165.6000m,
                        TotalVATAmount = 41.4000m,
                        UnitListPrice = 82.80000000m,
                        VATPercentage = 0.2500m
                    },
                },
                PaymentInfo = new List<PaymentInfoCarrier>
                {
                    new PaymentInfoCarrier()
                }
            };

            
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
    }
}
