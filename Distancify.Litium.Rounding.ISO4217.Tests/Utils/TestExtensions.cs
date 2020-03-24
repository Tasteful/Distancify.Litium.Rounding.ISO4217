using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Distancify.Litium.Rounding.ISO4217.Tests.Utils
{
    public static class TestExtensions
    {
        public static void SetInternalProperty<TSource, TProperty, TValue>(this TSource source, Expression<Func<TSource, TProperty>> property, TValue value)
        {
            var p = GetPropertyInfo(property);

            p.SetValue(source, value);
        }

        public static PropertyInfo GetPropertyInfo<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertyLambda)
        {
            MemberExpression member = propertyLambda.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException($"Expression '{propertyLambda}' refers to a method, not a property.");

            PropertyInfo propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException($"Expression '{propertyLambda}' refers to a field, not a property.");

            return propInfo;
        }
    }
}
