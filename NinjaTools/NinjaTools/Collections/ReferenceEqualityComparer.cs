﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace NinjaTools.Collections
{
    /// <summary>
    /// An equality comparer that compares objects for reference equality.
    /// </summary>
    /// <typeparam name="T">The type of objects to compare.</typeparam>
    [SuppressMessage("dot42", "StaticFieldInGenericType")]
    [SuppressMessage("dot42", "StaticConstructorUsesGenericParameter")]
    public sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
    {
        #region Predefined
        private static readonly ReferenceEqualityComparer<T> instance
            = new ReferenceEqualityComparer<T>();
        /// <summary>
        /// Gets the default instance of the
        /// <see cref="ReferenceEqualityComparer{T}"/> class.
        /// </summary>
        /// <value>A <see cref="ReferenceEqualityComparer{T}"/> instance.</value>
        public static ReferenceEqualityComparer<T> Instance
        {
            get { return instance; }
        }
        #endregion

        /// <inheritdoc />
        public bool Equals(T left, T right)
        {
            return ReferenceEquals(left, right);
        }

        /// <inheritdoc />
        public int GetHashCode(T value)
        {
            return RuntimeHelpers.GetHashCode(value);
        }
    }
}