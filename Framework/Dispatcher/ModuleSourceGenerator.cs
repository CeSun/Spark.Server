using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Dispatcher
{
    public class ModuleSourceGenerator : IPrivateSourceGenerator
    {
        private GeneratorExecutionContext context;
        public void Execute(GeneratorExecutionContext context)
        {
            this.context = context;
            if (context.SyntaxReceiver is ModuleSyntaxReceiver Receiver)
            {
                List<ModuleClassInfo> Modules = new List<ModuleClassInfo>();
                List<ModuleClassInfo> LifeCycleModules = new List<ModuleClassInfo>();
                foreach(var Class in  Receiver.Classes)
                {
                    foreach(var AttributeList in Class.AttributeLists)
                    {
                        foreach (var Attribute in AttributeList.Attributes)
                        {
                            if (HasAttribute(Attribute, "Frame.Attributes.ModuleAttribute"))
                            {
                                var className = GetModuleClassName(Class);
                                var args = GetAttributeParams(Attribute);

                                var ModuleClass = new ModuleClassInfo
                                {
                                    FullName = className,
                                    FieldName = args[0],
                                    NeedApplication = false
                                };

                                var property = GetPropertyNameByType(Class, "ServerApplication");
                                if (property != null)
                                {
                                    ModuleClass.NeedApplication = true;
                                    ModuleClass.ApplicationMemberName = property;
                                }
                                var methodList = ProcessMethod(Class);
                                ModuleClass.RouteMethods = methodList;
                                Modules.Add(ModuleClass);
                                if (HasImplInterface(Class, "Frame.Interfaces.ILifeCycle"))
                                {
                                    LifeCycleModules.Add(ModuleClass);
                                }
                                goto end;
                            }
                        }
                    }
                end:
                    continue;

                }
                var source = $@"using Frame;
using Frame.NetDrivers;
using GameServer.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frame;
public partial class ServerApplication : Application
{{

    public ServerApplication() : base()
    {{
"; 
                foreach (var module in Modules)
                {
                    source +=
@$"         {module.FieldName} = new {module.FullName}() {{{module.ApplicationMemberName} = this}};";
                }
                source += 
@"
    }";
                foreach (var module in Modules)
                {
                    source += @$"
    public {module.FullName} {module.FieldName} {{get; private set;}}
";
                }

                source +=
@$"
    public override void OnStart()
    {{
";

                foreach(var module in LifeCycleModules)
                {
                    source += $@"
        {module.FieldName}.OnStart();
";
                }
 source +=
$@"
    }}

    public override void OnStop()
    {{
";

                foreach(var module in LifeCycleModules)
                {
                    source += $@"
        {module.FieldName}.OnStop();
";
                }
                source += $@"
    }}";
                source += "}";
                context.AddSource("ServerApplicationModule.g.cs", source);
            }



        }
        public bool HasAttribute(AttributeSyntax attribute, string attributeName)
        {
            var SemanticMode = context.Compilation.GetSemanticModel(attribute.SyntaxTree);
            var typeSymbol = SemanticMode.GetSymbolInfo(attribute).Symbol;
            var str = typeSymbol.ToString();
            var name = typeSymbol.ContainingSymbol.ToString();
            
            if (name == attributeName)
            {
                return true;
            }
            return false;

        }


        public bool HasImplInterface(ClassDeclarationSyntax classDeclaration, string interfaceName)
        {
            var SemanticMode = context.Compilation.GetSemanticModel(classDeclaration.SyntaxTree);
            var typeSymbol = SemanticMode.GetDeclaredSymbol(classDeclaration);
            var InterfaceType = context.Compilation.GetTypeByMetadataName(interfaceName);

            foreach (var type in classDeclaration.BaseList.Types)
            {
                if (SemanticMode.GetTypeInfo(type.Type).Type.ToDisplayString() == interfaceName)
                {
                    return true;
                }
            }
            return false;
        }
        public string GetPropertyNameByType(ClassDeclarationSyntax classDeclaration, string TypeName)
        {
            var SemanticMode = context.Compilation.GetSemanticModel(classDeclaration.SyntaxTree);
            var typeSymbol = SemanticMode.GetDeclaredSymbol(classDeclaration);
            var appType = SemanticMode.Compilation.GetTypeByMetadataName(TypeName);
            foreach(var  Member in typeSymbol.GetMembers().OfType<IPropertySymbol>())
            {
                if (Member.SetMethod == null || Member.SetMethod.MethodKind != MethodKind.PropertySet) {
                    continue;
                }
                var str = Member.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                if (str.IndexOf(TypeName) >= 0 ) {
                    return Member.Name;
                }

            }
            return null;
        }

        public string GetModuleClassName(ClassDeclarationSyntax classDeclaration)
        {
            var SemanticMode = context.Compilation.GetSemanticModel(classDeclaration.SyntaxTree);
            var typeSymbol = SemanticMode.GetDeclaredSymbol(classDeclaration);

            return typeSymbol.ToDisplayString();
        }

        public List<string> GetAttributeParams (AttributeSyntax attribute)
        {
            var SemanticMode = context.Compilation.GetSemanticModel(attribute.SyntaxTree);
            var List = new List<string>();
            foreach(var args in attribute.ArgumentList.Arguments)
            {
                ;
                var arg = SemanticMode.GetConstantValue(args.Expression);
                if (arg.HasValue)
                {
                    List.Add(arg.Value.ToString());
                }
            }

            return List;
        }


        public List<RouteMethodInfo> ProcessMethod(ClassDeclarationSyntax classDeclaration)
        {
            List<RouteMethodInfo> list = new List<RouteMethodInfo>();
            foreach(var method in classDeclaration.Members.OfType<MethodDeclarationSyntax>())
            {
                foreach(var attlist in method.AttributeLists)
                {
                    foreach(var att in attlist.Attributes)
                    {
                        var b = HasAttribute(att, "Frame.Attributes.RouteAttribute");
                    }
                }
            }
            return list;
        }
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new ModuleSyntaxReceiver());
        }


    }


    public class ModuleSyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> Classes = new List<ClassDeclarationSyntax>();
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax classSyntax)
            {
                Classes.Add(classSyntax);
            }
        }
    }


    public class ModuleClassInfo
    {
        public string FullName { get; set; }
        public string FieldName { get; set; }
        public bool NeedApplication { get; set; }

        public string ApplicationMemberName { get; set; }

        public List<RouteMethodInfo> RouteMethods { get; set; }
    }

    public class RouteMethodInfo
    {
        public string Name { get; set; }

        public string ReqMsgId { get; set; }

        public string RspMsgId { get; set; }

        public string ReqType { get; set; }


    }
}
