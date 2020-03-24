using System;
using System.Threading;
using Litium.Foundation.Modules.ECommerce.Carriers;

namespace Distancify.Litium.Rounding.ISO4217
{
    /// <summary>
    /// <para>
    ///     If you're calling any calculator directly, outside of <see cref="global::Litium.Foundation.Modules.ECommerce.Plugins.Orders.IOrderCalculator"/>,
    ///     you must set the order carrier you're calculating in the context. This is needed as not
    ///     all calculators takes an <see cref="OrderCarrier"/> as argument.
    /// </para>
    /// <code>
    /// using (CalculatorContext.Use(orderCarrier) { ... }
    /// </code>
    /// </summary>
    public static class CalculatorContext
    {
        private const string slotKey = "OrderCarrier_09EC6EA2-5847-4336-826B-614896AC1A1A";

        public static OrderCarrierWrapper Use(OrderCarrier order)
        {
            return new OrderCarrierWrapper(order);
        }

        public static OrderCarrier GetCurrentOrderCarrier()
        {
            return Thread.GetData(Thread.GetNamedDataSlot(slotKey)) as OrderCarrier;
        }

        public sealed class OrderCarrierWrapper : IDisposable
        {
            private readonly OrderCarrier previous;

            public OrderCarrierWrapper(OrderCarrier order)
            {
                previous = Thread.GetData(Thread.GetNamedDataSlot(slotKey)) as OrderCarrier;

                Thread.SetData(
                    Thread.GetNamedDataSlot(slotKey),
                    order);
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                Thread.SetData(
                    Thread.GetNamedDataSlot(slotKey),
                    previous);
            }
        }
    }
}
