# Introduction 
This project adds support for proper rounding based on order currency (ISO-4217) in Litium by replacing
most of the default calculators.

It's not an add-on per se, but can rather be seen as a patch.

## A note about order's total VAT

This package changes how the default rounding behaves in Litium. While the default Litium implementation
rounds the order totals, this package ensures that numbers round up accurately on rows. This means that
the order's total VAT can seem off, because if you take the order total and multiplies with the VAT
percentage, you might not end up with exactly the total VAT on the order. This is because currencies are
inherently inaccurate since they use so few decimals. But by rounding on row level instead of order totals,
we make sure we don't run into any issues with payment options and avoid nasty rounding rows in the order's
PaymentInfo.

# Install

```
Install-Package Distancify.Litium.Rounding.ISO4217
```

# Using

This is a drop-in patch. After install, simply run Litium as normal.

## Custom calculators

This comes with new implementations for the following Litium plugins. If you are already overriding these
plugins, you need to take extra care.

- IOrderCalculator
- IOrderTotalCalculator
- IVatCalculator
- IOrderGrandTotalCalculator
- IPaymentInfoRowFactory

## Note about PaymentInfoRowCarrier.ctor()

If you're constructing PaymentInfoRowCarriers in your project, you should instead switch to construct them
using PaymentInfoRowBuilder. This is because PaymentInfoRowCarrier's default constructors doesn't do
rounding properly (i.e it will round to two decimals no matter what).

# Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct, and the process for
submitting pull requests to us.

# Versioning

We use [SemVer](http://semver.org/) for versioning.

# License

This project is licensed under the LGPL v3 License - see the [LICENSE](LICENSE) file for details