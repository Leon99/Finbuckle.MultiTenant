﻿// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Reflection;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;

internal static class TypeExtensions
{
    public static bool ImplementsOrInheritsUnboundGeneric(this Type source, Type unboundGeneric)
    {
        if (unboundGeneric.IsInterface)
        {
            return source.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == unboundGeneric);
        }

        Type? toCheck = source;

        if (unboundGeneric != toCheck)
        {
            while (toCheck is not null && toCheck != typeof(object))
            {
                var current = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;

                if (unboundGeneric == current)
                {
                    return true;
                }

                toCheck = toCheck.BaseType;
            }
        }

        return false;
    }

    internal static bool HasMultiTenantAttribute(this Type type)
    {
        return type.GetCustomAttribute<MultiTenantAttribute>() is not null;
    }
}