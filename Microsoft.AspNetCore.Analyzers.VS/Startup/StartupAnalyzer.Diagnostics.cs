// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.AspNetCore.Analyzers
{
    public partial class StartupAnalzyer : DiagnosticAnalyzer
    {
        internal static class Diagnostics
        {
            public static readonly ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics;

            static Diagnostics()
            {
                SupportedDiagnostics = ImmutableArray.Create<DiagnosticDescriptor>(new[]
                {
                    MiddlewareMissingRequiredServices,
                    MiddlewareInvalidOrder,
                    UnsupportedUseMvcWithEndpointRouting,
                });
            }

            internal readonly static DiagnosticDescriptor MiddlewareMissingRequiredServices = new DiagnosticDescriptor(
                "ASPC0001",
                "Middleware missing required services.",
                "Blah blah",
                "Usage",
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true,
                helpLinkUri: null);

            internal readonly static DiagnosticDescriptor MiddlewareInvalidOrder = new DiagnosticDescriptor(
                "ASPC0000",
                "Middleware in invalid order.",
                "Blah blah",
                "Usage",
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true,
                helpLinkUri: null);

            internal readonly static DiagnosticDescriptor UnsupportedUseMvcWithEndpointRouting = new DiagnosticDescriptor(
                "MVC1005",
                "Cannot use UseMvc with Endpoint Routing.",
                "Using '{0}' to configure MVC is not supported while using Endpoint Routing. To continue using '{0}', please set 'MvcOptions.EnableEndpointRounting = false' inside '{1}'.",
                "Usage",
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true,
                helpLinkUri: "https://aka.ms/YJggeFn");
        }
    }
}
