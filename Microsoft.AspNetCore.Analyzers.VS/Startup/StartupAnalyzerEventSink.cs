// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis;

namespace Microsoft.AspNetCore.Analyzers
{
    public static class StartupAnalyzerEventSink
    {
        public static event EventHandler<IMethodSymbol> ConfigureServicesMethodFound;

        internal static void OnConfigureServicesMethodFound(IMethodSymbol method)
        {
            ConfigureServicesMethodFound?.Invoke(null, method);
        }

        public static event EventHandler<ServicesAnalysis> ServicesAnalysisCompleted;

        internal static void OnServicesAnalysisCompleted(ServicesAnalysis analysis)
        {
            ServicesAnalysisCompleted?.Invoke(null, analysis);
        }

        public static event EventHandler<OptionsAnalysis> OptionsAnalysisCompleted;

        internal static void OnOptionsAnalysisCompleted(OptionsAnalysis analysis)
        {
            OptionsAnalysisCompleted?.Invoke(null, analysis);
        }

        public static event EventHandler<IMethodSymbol> ConfigureMethodFound;

        internal static void OnConfigureMethodFound(IMethodSymbol method)
        {
            ConfigureMethodFound?.Invoke(null, method);
        }

        public static event EventHandler<MiddlewareAnalysis> MiddlewareAnalysisCompleted;

        internal static void OnMiddlewareAnalysisCompleted(MiddlewareAnalysis analysis)
        {
            MiddlewareAnalysisCompleted?.Invoke(null, analysis);
        }
    }
}
