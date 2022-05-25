namespace net.openstack.Core.Domain.Mapping
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Represents an object that can convert between instances of two specific types.
    /// </summary>
    /// <remarks>
    /// This interface is similar to a <see cref="TypeConverter"/> which only supports
    /// conversions between exactly two concrete types.
    /// </remarks>
    /// <typeparam name="TFrom">The first type.</typeparam>
    /// <typeparam name="TTo">The second type.</typeparam>
    public interface IObjectMapper<TFrom, TTo>
    {
        /// <summary>
        /// Converts an instance of <typeparamref name="TFrom"/> to an instance of <typeparamref name="TTo"/>.
        /// </summary>
        /// <remarks>
        /// This method provides behavior similar to a strongly-typed implementation
        /// of <see cref="TypeConverter.ConvertTo(object, Type)"/>.
        /// </remarks>
        /// <param name="from">The instance to convert.</param>
        /// <returns>The converted instance.</returns>
        /// <exception cref="NotSupportedException">The conversion cannot be performed.</exception>
        TTo Map(TFrom from);

        /// <summary>
        /// Converts an instance of <typeparamref name="TTo"/> to an instance of <typeparamref name="TFrom"/>.
        /// </summary>
        /// <remarks>
        /// This method provides behavior similar to a strongly-typed implementation
        /// of <see cref="TypeConverter.ConvertFrom(object)"/>.
        /// </remarks>
        /// <param name="to">The instance to convert.</param>
        /// <returns>The converted instance.</returns>
        /// <exception cref="NotSupportedException">The conversion cannot be performed.</exception>
        TFrom Map(TTo to);
    }
}
