using System;
using System.Collections;
using System.Collections.Generic;

namespace Common.Libs
{
    public static class TypeCoreExtensions
    {
        public static IList CreateElementTypeAsGenericList(this Type elementType)
        {
            return (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(new Type[] { elementType }));
        }
    }
}
