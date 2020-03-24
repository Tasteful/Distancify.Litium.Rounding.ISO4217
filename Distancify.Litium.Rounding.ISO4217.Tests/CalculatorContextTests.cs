using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Litium.Foundation.Modules.ECommerce.Carriers;
using Xunit;

namespace Distancify.Litium.Rounding.ISO4217.Tests
{
    public class CalculatorContextTests
    {
        [Fact]
        public void IsSet()
        {
            var order = new OrderCarrier();

            using (CalculatorContext.Use(order))
            {
                Assert.Same(order, CalculatorContext.GetCurrentOrderCarrier());
            }

            Assert.Null(CalculatorContext.GetCurrentOrderCarrier());
        }

        [Fact]
        public void Nested()
        {
            var order = new OrderCarrier();
            var order2 = new OrderCarrier();

            using (CalculatorContext.Use(order))
            {
                using (CalculatorContext.Use(order2))
                {
                    Assert.Same(order2, CalculatorContext.GetCurrentOrderCarrier());
                }

                Assert.Same(order, CalculatorContext.GetCurrentOrderCarrier());
            }

            Assert.Null(CalculatorContext.GetCurrentOrderCarrier());
        }
    }
}
