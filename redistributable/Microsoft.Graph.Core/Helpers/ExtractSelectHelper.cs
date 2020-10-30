// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    
    /// <summary>
    /// Helper class to extract $select or $expand parameters from strongly-typed expressions.
    /// </summary>
    public static class ExpressionExtractHelper
    {
        /// <summary>
        /// Extract referenced members of the type T from the given expression as a list of strings
        /// </summary>
        /// <param name="expression">The expression to search</param>
        /// <param name="error">Message about what's wrong with the expression if return value is null</param>
        /// <returns>A comma-separated list of strings or null</returns>
        public static string ExtractMembers<T>(Expression<Func<T, object>> expression, out string error)
        {
            error = null;
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            // Search s => s.Foo
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression != null)
            {
                return ProcessSimpleMemberExpression<T>(memberExpression, ref error);
            }

            // Search s => s.BarFromBaseType 
            // Property base type expressions introduce an intermediate conversion operator.
            var convertExpression = expression.Body as UnaryExpression;
            if (convertExpression?.NodeType == ExpressionType.Convert)
            {
                memberExpression = convertExpression.Operand as MemberExpression;
                if (memberExpression != null)
                {
                    return ProcessSimpleMemberExpression<T>(memberExpression, ref error);
                }
            }

            // Search s => new { [Foo = ]s.Foo, [bar = ]s.Bar }
            // We'd prefer not to support the variant with named anonymous type members, but the expression trees don't differentiate, 
            // between implicit and explicit naming, so there's no way to throw an error.
            var newExpression = expression.Body as NewExpression;
            if (newExpression != null)
            {
                if (newExpression.Arguments == null || newExpression.Arguments.Count == 0)
                {
                    error = "Lambda expression must provide initializer for new anonymous type.";
                    return null;
                }
                if (newExpression.Arguments.Any(a =>
                {
                    var memberArgument = a as MemberExpression;
                    return memberArgument == null ||
                           !(memberArgument.Expression is ParameterExpression) ||
                           !memberArgument.Member.DeclaringType.GetTypeInfo().IsAssignableFrom(typeof(T).GetTypeInfo());
                }))
                {
                    error = $"Anonymous type in lambda expression may only be initialized with direct members of type {typeof(T).Name}";
                    return null;
                }

                // Search only for direct members of the lambda's parameter
                // Should already be validated above, but doesn't hurt to be sure.
                var members = from m in newExpression.Arguments.OfType<MemberExpression>()
                    where m.Expression is ParameterExpression && m.Member.DeclaringType.GetTypeInfo().IsAssignableFrom(typeof(T).GetTypeInfo())
                    select GetMemberWireName(m.Member);
                return string.Join(",", members);
            }
            error = "Unrecognized lambda expression.";
            return null;
        }

        private static string ProcessSimpleMemberExpression<T>(MemberExpression memberExpression, ref string error)
        {
            if (!memberExpression.Member.DeclaringType.GetTypeInfo().IsAssignableFrom(typeof (T).GetTypeInfo()))
            {
                error = $"Anonymous type in lambda expression may only be initialized with direct members of type {typeof (T).Name}";
                return null;
            }
            return GetMemberWireName(memberExpression.Member);
        }

        private static string GetMemberWireName(MemberInfo member)
        {
            var jsonProperty = member.GetCustomAttribute<Newtonsoft.Json.JsonPropertyAttribute>();
            if (jsonProperty != null && !string.IsNullOrWhiteSpace(jsonProperty.PropertyName))
            {
                return jsonProperty.PropertyName;
            }
            return StringHelper.ConvertIdentifierToLowerCamelCase(member.Name);
        }
    }
}