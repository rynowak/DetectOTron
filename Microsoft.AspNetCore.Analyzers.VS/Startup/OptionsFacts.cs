﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis;

namespace Microsoft.AspNetCore.Analyzers
{
    internal static class OptionsFacts
    {
        public static bool IsEndpointRoutingExplicitlyDisabled(OptionsAnalysis analysis)
        {
            for (var i = 0; i < analysis.Options.Length; i++)
            {
                var item = analysis.Options[i];
                if (string.Equals(item.OptionsType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat), "Microsoft.AspNetCore.Mvc.MvcOptions") &&
                    string.Equals(item.Property.Name, "EnableEndpointRouting", StringComparison.Ordinal))
                {
                    return item.ConstantValue as bool? == false;
                }
            }

            return false;
        }
    }
}
