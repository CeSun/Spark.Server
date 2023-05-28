using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dispatcher
{
    public interface IPrivateSourceGenerator
    {
        void Execute(GeneratorExecutionContext context);

        void Initialize(GeneratorInitializationContext context);
    }
}
