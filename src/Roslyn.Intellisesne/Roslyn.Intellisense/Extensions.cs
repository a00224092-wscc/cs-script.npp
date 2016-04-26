using Intellisense.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace RoslynIntellisense
{
    public static class Extensions
    {
        public static bool HasText(this string text)
        {
            return !string.IsNullOrEmpty(text);
        }

        public static T To<T>(this object obj)
        {
            return (T) obj;
        }

        public static CompletionType ToCompletionType(this SymbolKind kind)
        {
            switch (kind)
            {
                case SymbolKind.Method: return CompletionType.method;
                case SymbolKind.Event: return CompletionType._event;
                case SymbolKind.Field: return CompletionType.field;
                case SymbolKind.Property: return CompletionType.property;
                case SymbolKind.Namespace: return CompletionType._namespace;
                case SymbolKind.Assembly: return CompletionType._namespace;
                default:
                    return CompletionType.unresolved;
            }
        }

        public static string ToDecoratedName(this ITypeSymbol type)
        {
            string result = type.ToDisplayString();

            if (!result.Contains('.'))
                return result;
            //if (clrAliaces.ContainsKey(result))
            //    return clrAliaces[result];

            string nmspace = type.GetNamespace();
            if (!string.IsNullOrEmpty(nmspace))
                nmspace = "{" + nmspace + "}.";
            return (nmspace + type.Name);
        }

        public static string GetNamespace(this ISymbol type)
        {
            List<string> parts = new List<string>();
            var nm = type.ContainingNamespace;

            while (nm != null && nm.Name.HasText())
            {
                parts.Add(nm.Name);
                nm = nm.ContainingNamespace;
            }

            parts.Reverse();
            return string.Join(".", parts.ToArray());
        }

        public static string GetFullName(this ISymbol type)
        {
            List<string> parts = new List<string>();
            var nm = type.ContainingNamespace;

            while (nm != null && nm.Name.HasText())
            {
                parts.Add(nm.Name);
                nm = nm.ContainingNamespace;
            }

            parts.Reverse();
            parts.Add(type.Name);
            return string.Join(".", parts.ToArray());
        }

        public static string ToDisplayKind(this ISymbol symbol)
        {
            if (symbol is INamedTypeSymbol)
                return (symbol as INamedTypeSymbol).TypeKind.ToString();

            if (symbol is IMethodSymbol)
            {
                switch ((symbol as IMethodSymbol).MethodKind)
                {
                    case MethodKind.LambdaMethod:
                        return "Lambda";
                    case MethodKind.Constructor:
                        return "Cosntructor";
                    case MethodKind.Destructor:
                        return "Destructor";
                    case MethodKind.ReducedExtension:
                        return "Extension Method";
                    case MethodKind.StaticConstructor:
                        return "Static Constructor";
                    case MethodKind.UserDefinedOperator:
                    case MethodKind.BuiltinOperator:
                        return "Operator";
                    case MethodKind.DeclareMethod:
                    default:
                        return "Method";
                }
            }

            return symbol.Kind.ToString();
        }

        public static ICompletionData ToCompletionData(this ISymbol symbol)
        {
            if (symbol.Kind == SymbolKind.Event ||
                symbol.Kind == SymbolKind.Property ||
                symbol.Kind == SymbolKind.Field ||
                symbol.Kind == SymbolKind.Method)
            {
                // Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE.PEMethodSymbol is internal so we cannot explore it
                var invokeParams = new List<string>();
                string invokeReturn = null;
                try
                {
                    if (symbol.Kind == SymbolKind.Method && symbol is IMethodSymbol)
                    {
                        var method = (IMethodSymbol) symbol;
                        foreach (var item in method.Parameters)
                        {
                            invokeParams.Add(item.Type.ToDecoratedName() + " " + item.Name);
                        }
                        invokeReturn = method.ReturnType.ToDecoratedName();
                    }
                    else if (symbol.Kind == SymbolKind.Event && symbol is IEventSymbol)
                    {
                        var method = symbol.To<IEventSymbol>()
                                           .Type.To<INamedTypeSymbol>()
                                           .DelegateInvokeMethod.To<IMethodSymbol>();

                        foreach (var item in method.Parameters)
                        {
                            invokeParams.Add(item.Type.ToDecoratedName() + " " + item.Name);
                        }
                        invokeReturn = method.ReturnType.ToDecoratedName();
                    }
                }
                catch { }

                return new EntityCompletionData
                {
                    DisplayText = symbol.Name,
                    CompletionText = symbol.Name,
                    CompletionType = symbol.Kind.ToCompletionType(),
                    RawData = symbol,
                    InvokeParameters = invokeParams,
                    InvokeParametersSet = true,
                    InvokeReturn = invokeReturn
                };
            }
            else
            {
                return new CompletionData
                {
                    DisplayText = symbol.Name,
                    CompletionText = symbol.Name,
                    CompletionType = symbol.Kind.ToCompletionType(),
                    RawData = symbol
                };
            }
        }

        public static int GetWordStartOf(this string text, int offset)
        {
            if (text[offset] != '.') //we may be at the partially complete word
                for (int i = offset - 1; i >= 0; i--)
                    if (Autocompleter.Delimiters.Contains(text[i]))
                        return i + 1;
            return offset;
        }

        public static object GetProp(this object obj, string name)
        {
            var property = obj.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (property == null)
                throw new Exception("ReflectionExtensions: cannot find property " + name);
            return property.GetValue(obj, null);
        }

        public static string[] GetUsingNamespace(this SyntaxNode syntax, string code)
        {
            var t = syntax.SyntaxTree.Options.DocumentationMode;


            var namespaces = syntax.DescendantNodes()
                               .OfType<UsingDirectiveSyntax>()
                               .Select(x => x.GetText().ToString()
                                             .Replace("using", "")
                                             .Replace(";", "")
                                             .Trim())
                               .ToArray();
            return namespaces;
        }

        //public static string GetXmlDocFor(this SyntaxNode syntax, string docId)
        //{
        //    if (docId.StartsWith("T"))
        //    {
        //        var docContent = syntax.DescendantNodes()
        //                               .Where(x => x is ClassDeclarationSyntax || x is InterfaceDeclarationSyntax || x is StructDeclarationSyntax || x is DelegateDeclarationSyntax)
        //                               .FirstOrDefault()
        //                               .GetXmlDocString();
        //        return docContent;
        //    }
        //    else
        //    {
        //        var parts = docId.Split('.');
        //        //var member = parts.
        //        var type = string.Join(".", parts.Take(parts.Length - 1).ToArray());
        //        var typeNode = syntax.DescendantNodes()
        //                             .OfType<TypeDeclarationSyntax>()
        //                             .FirstOrDefault();


        //        // string xmlText = DocumentationCommentCompiler.GetDocumentationCommentXml(symbol, expandIncludes, default(CancellationToken));
        //        //T:F:M:P
        //        //var typeNode = syntax.DescendantNodes()
        //        //    .Where(x=>x.IsKind(SyntaxKind.t))

        //        var docContent = syntax.DescendantNodes()
        //                               //.OfType<ClassDeclaration SingleLineDocumentationCommentTrivia>()
        //                               //.Where(x=>x.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
        //                               //.Select(x => x.GetText().ToString()
        //                               //              .Replace("using", "")
        //                               //              .Replace(";", "")
        //                               //              .Trim())
        //                               .ToArray();

        //        var ttt = docContent.FirstOrDefault(x=> x is INamespaceOrTypeSymbol) as INamespaceOrTypeSymbol;
        //        //var r = ttt.GetFullMetadataName();
        //        //%
        //        var tt22t = docContent[3].ToString();
        //        return docContent[3].GetXmlDocString();
        //    }
        //}

        //public static string GetFullName(this TypeDeclarationSyntax symbol)
        //{
        //    return null;
        //    var sb = new StringBuilder(symbol.ToFullString());

        //    var last = s;
        //    s = s.ContainingSymbol;
        //    while (!IsRootNamespace(s))
        //    {
        //        if (s is ITypeSymbol && last is ITypeSymbol)
        //        {
        //            sb.Insert(0, '+');
        //        }
        //        else
        //        {
        //            sb.Insert(0, '.');
        //        }
        //        sb.Insert(0, s.MetadataName);
        //        s = s.ContainingSymbol;
        //    }

        //    return sb.ToString();
        //}

        //private static bool IsRootNamespace(ISymbol s)
        //{
        //    return s is INamespaceSymbol && ((INamespaceSymbol) s).IsGlobalNamespace;
        //}

        //static string GetTypeName(string docId)
        //{
        //    switch (docId[0])
        //    {
        //        case 'M':
        //        case 'E':
        //        case 'F':
        //        case 'P':
        //            {
        //                var parts = docId.Split('.');

        //                return string.Join(".", parts.Take(parts.Length-1).ToArray());
        //            }
        //        case 'T': return docId;
        //    }
        //    return "";
        //}

        //static string GetXmlDocString(this SyntaxNode node)
        //{
        //    if (node == null)
        //        return "";

        //    var xmlTrivia = node.GetLeadingTrivia()
        //        .Select(i => i.GetStructure())
        //        .OfType<DocumentationCommentTriviaSyntax>()
        //        .FirstOrDefault();

        //    if (xmlTrivia != null)
        //        return xmlTrivia.ToString().Replace("///", "");

        //    return "";
        //}

        public static bool HasExtension(this string file, string extension)
        {
            return string.Compare(Path.GetExtension(file), extension, true) == 0;
        }

        public static string ShrinkNamespaces(this string statement, params string[] knownNamespaces)
        {
            //statement format: "{<namespace_name>}.type"
            string result = statement;

            foreach (var item in knownNamespaces.Select(x => "{" + x + "}."))
                result = result.Replace(item, "");

            return result.Replace("{", "")
                         .Replace("}", "");
        }
    }

    internal static class XmlDocExtensions
    {
        public static string Shrink(this string text)
        {
            //should be replaced with RegEx (eventually)
            string retval = text.Replace(" \r\n", " ").Replace("\r\n ", " ").Replace("\r\n", " ").Replace("\t", " ");
            //Sandcastle has problem processing <para/> with the content having line breaks. This leads to the
            //multiple joined spaces. The following is a simplistic solution for this.

            string newRetval;
            while (true)
            {
                newRetval = retval.Replace("  ", " ");
                if (newRetval.Length != retval.Length)
                    retval = newRetval;
                else
                    return newRetval;
            }
        }

        public static string GetCrefAttribute(this XmlTextReader reader)
        {
            try
            {
                string typeName = reader.GetAttribute("cref");
                if (typeName != null)
                {
                    if (typeName.StartsWith("T:") || typeName.StartsWith("F:") || typeName.StartsWith("M:"))
                        typeName = typeName.Substring(2);
                }
                else
                {
                    return reader.GetAttribute(0);
                }
                return typeName;
            }
            catch
            {
                return "";
            }
        }

        //XmlTextReader "crawling style" reader fits better the purpose than a "read it all at once" XDocument
        public static string XmlToPlainText(this string xmlDoc, bool isReflectionDocument = false, bool ignoreExceptionsInfo = false)
        {
            //var root.XElement.Parse("<root>" + entity.Documentation.Xml.Text + "</root>");

            var sections = new List<string>();

            var b = new StringBuilder();
            try
            {
                using (var reader = new XmlTextReader(new StringReader("<root>" + xmlDoc + "</root>")))
                {
                    string lastElementName = null;
                    bool exceptionsStarted = false;
                    reader.XmlResolver = null;
                    while (reader.Read())
                    {
                        var nodeType = reader.NodeType;
                        switch (nodeType)
                        {
                            case XmlNodeType.Text:
                                if (lastElementName == "summary")
                                {
                                    b.Insert(0, reader.Value.Shrink());
                                }
                                else
                                {
                                    if (exceptionsStarted)
                                        b.Append("  ");

                                    if (lastElementName == "code")
                                        b.Append(reader.Value); //need to preserve all formatting (line breaks and indents)
                                    else
                                    {
                                        //if (reflectionDocument)
                                        //    b.Append(reader.Value.NormalizeLines()); //need to preserve line breaks but not indents
                                        //else
                                        b.Append(reader.Value.Shrink());
                                    }
                                }
                                break;

                            case XmlNodeType.Element:
                                {
                                    bool silentElement = false;

                                    switch (reader.Name)
                                    {
                                        case "filterpriority":
                                            reader.Skip();
                                            break;

                                        case "root":
                                        case "summary":
                                        case "c":
                                            silentElement = true;
                                            break;

                                        case "paramref":
                                            silentElement = true;
                                            b.Append(reader.GetAttribute("name"));
                                            break;

                                        case "param":
                                            silentElement = true;
                                            b.AppendLine();
                                            b.Append(reader.GetAttribute("name") + ": ");
                                            break;

                                        case "para":
                                            silentElement = true;
                                            b.AppendLine();
                                            break;

                                        case "remarks":
                                            b.AppendLine();
                                            b.Append("Remarks: ");
                                            break;

                                        case "returns":
                                            silentElement = true;
                                            b.AppendLine();
                                            b.Append("Returns: ");
                                            break;

                                        case "exception":
                                            {
                                                if (!exceptionsStarted)
                                                {
                                                    b.AppendLine();
                                                    sections.Add(b.ToString().Trim());
                                                    b.Length = 0;
                                                    if (!ignoreExceptionsInfo)
                                                        b.AppendLine("Exceptions: ");
                                                }
                                                exceptionsStarted = true;

                                                if (!ignoreExceptionsInfo && !reader.IsEmptyElement)
                                                {
                                                    bool printExInfo = false;
                                                    if (printExInfo)
                                                    {
                                                        b.Append("  " + reader.GetCrefAttribute() + ": ");
                                                    }
                                                    else
                                                    {
                                                        b.Append("  " + reader.GetCrefAttribute());
                                                        reader.Skip();
                                                    }
                                                }
                                                break;
                                            }
                                        case "see":
                                            silentElement = true;
                                            if (reader.IsEmptyElement)
                                            {
                                                b.Append(reader.GetCrefAttribute());
                                            }
                                            else
                                            {
                                                reader.MoveToContent();
                                                if (reader.HasValue)
                                                {
                                                    b.Append(reader.Value);
                                                }
                                                else
                                                {
                                                    b.Append(reader.GetCrefAttribute());
                                                }
                                            }
                                            break;
                                    }

                                    if (!silentElement)
                                        b.AppendLine();

                                    lastElementName = reader.Name;
                                    break;
                                }
                            case XmlNodeType.EndElement:
                                {
                                    if (reader.Name == "summary")
                                    {
                                        b.AppendLine();
                                        sections.Add(b.ToString().Trim());
                                        b.Length = 0;
                                    }
                                    else if (reader.Name == "returns")
                                    {
                                        b.AppendLine();
                                        sections.Add(b.ToString().Trim());
                                        b.Length = 0;
                                    }
                                    break;
                                }
                        }
                    }
                }

                sections.Add(b.ToString().Trim());

                string sectionSeparator = (isReflectionDocument ? "\r\n--------------------------\r\n" : "\r\n\r\n");
                return string.Join(sectionSeparator, sections.Where(x => !string.IsNullOrEmpty(x)).ToArray());
            }
            catch (XmlException)
            {
                return xmlDoc;
            }
        }
    }

    public static class CompletionStringExtensions
    {
        public static bool CanComplete(this string completion, string partial)
        {
            return completion.StartsWith(partial) || completion.IsSubsequenceMatch(partial);
        }

        public static bool IsCamelCaseMatch(this string completion, string partialWord)
        {
            return new string(completion.Where(c => char.IsLetter(c) && char.IsUpper(c)).ToArray()).StartsWith(partialWord, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsSubsequenceMatch(this string completion, string partial)
        {
            if (!partial.HasText())
                return true;

            if (char.ToUpperInvariant(completion[0]) != char.ToUpperInvariant(partial[0]))
                return false;
            else
                return string.Compare(new string(completion.ToUpper().Intersect(partial.ToUpper()).ToArray()),
                                      partial,
                                      StringComparison.OrdinalIgnoreCase) == 0;
        }
    }
}