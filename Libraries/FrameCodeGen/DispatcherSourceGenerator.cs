using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameCodeGen
{
   class Debug
   {
        StreamWriter sw;
        public Debug()
        {
        }
        string info = "";
        public  void Log(string info)
        {
            this.info += info += "\n";
        }

        public void Output()
        {
            sw = new StreamWriter("d:/debug.txt");
            sw.Write(info);
            sw.Close();
        }
   }
    [Generator]
    public class DispatcherSourceGenerator : ISourceGenerator
    {
        Debug debug = new Debug();
        private GeneratorExecutionContext ctx;
        public void Execute(GeneratorExecutionContext context)
        {
            try
            {

                ctx = context;
                if (!(context.SyntaxContextReceiver is SyntaxReceiver receiver))
                    return;
                string source = "// 头文件" +
                                "\nusing Frame;\n";
                foreach (var obj in receiver.needDispatcherList)
                {
                    source += ProcessDispatcher(obj);
                }
                
                context.AddSource("dispatcher.cs", source);
                debug.Log(source);
            } catch (Exception e)
            {
                debug.Log(e.Message);
                debug.Log(e.StackTrace);
                debug.Log(e.ToString());
                throw e;
            } finally
            {

                debug.Output();
            }
            
        }
        class FunctionInfo
        {
           public List<(string type, string name)> ParamsList = new List<(string, string)>();
           public string FunName;
        }
        private string ProcessDispatcher(ClassDeclarationSyntax Obj)
        {
            var ObjInfo = ctx.Compilation.GetSemanticModel(Obj.SyntaxTree).GetDeclaredSymbol(Obj);
            if (ObjInfo == default)
                return "";
            string source = $"namespace {ObjInfo.ContainingNamespace}{{\n";
            source += "\tpublic partial class " + ObjInfo.Name + "\n\t{\n";

            FunctionInfo functionInfo = new FunctionInfo();
            foreach (var mem in Obj.Members)
            {
                if (!(mem is MethodDeclarationSyntax method))
                    continue;
                if (MethodHasAttribute(method, "Frame.DispatchMethodAttribute"))
                {
                    var baseInfo = ctx.Compilation.GetSemanticModel(method.SyntaxTree).GetDeclaredSymbol(method);
                    if (!(baseInfo is IMethodSymbol methodSymbol))
                        continue;
                    functionInfo.FunName = methodSymbol.Name;
                    foreach (var param in method.ParameterList.Parameters)
                    {
                        
                        var paramInfo = ctx.Compilation.GetSemanticModel(param.SyntaxTree).GetDeclaredSymbol(param);
                        if (paramInfo == null)
                            continue;
                        if (!(paramInfo is IParameterSymbol paramSymbol))
                            continue;
                        functionInfo.ParamsList.Add((paramSymbol.Type.ToDisplayString(), paramSymbol.Name));
                    }
                }
                else if (MethodHasAttribute(method, "Frame.ControllerAttribute"))
                {

                }
                else if (MethodHasAttribute(method, "Frame.FilterAttribute"))
                {

                }
            }
            source += "\t\tprivate partial  async Task " + functionInfo.FunName + "(";
            for (var i = 0;i < functionInfo.ParamsList.Count; i++)
            {
                if (i > 0)
                    source += ",";
                source += $"{functionInfo.ParamsList[i].type} {functionInfo.ParamsList[i].name}";
            }
            source += ")\n\t\t{\n";
            // 协议派发 switch-case
            // todo
            source += "\t\t}\n";
            source += "";
            source += "\t}\n}\n";
            return source;
        }

        private bool MethodHasAttribute(MethodDeclarationSyntax method, string attribute)
        {
            foreach (var attributeList in method.AttributeLists)
            {
                foreach (var att in attributeList.Attributes)
                {
                   
                    var info = ctx.Compilation.GetSemanticModel(att.SyntaxTree).GetSymbolInfo(att);
                    
                    if (info.Symbol.ContainingType.ToString() == attribute)
                    {
                        return true;
                    }
                }
            }
            return false;

        }
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }
    }

    class SyntaxReceiver : ISyntaxContextReceiver
    {
        public List<ClassDeclarationSyntax> needDispatcherList = new List<ClassDeclarationSyntax>();
        
        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is ClassDeclarationSyntax classDeclarationSyntax
                 && classDeclarationSyntax.AttributeLists.Count > 0)
                {
                    foreach(var attributes in classDeclarationSyntax.AttributeLists)
                    {
                        foreach(var attribute in attributes.Attributes)
                        {
                            var info = context.SemanticModel.GetSymbolInfo(attribute);
                            if (info.Symbol.ContainingType.ToString() == "Frame.DispatcherAttribute")
                            {
                                needDispatcherList.Add(classDeclarationSyntax);
                                return;
                            }
                        }
                    }
                }
         }
    }
}
