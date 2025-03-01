﻿namespace CapnpC.Generator
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using CapnpC.Model;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using static SyntaxHelpers;

    class CodeGenerator
    {
        readonly SchemaModel _model;
        readonly GenNames _names;
        readonly CommonSnippetGen _commonGen;
        readonly DomainClassSnippetGen _domClassGen;
        readonly ReaderSnippetGen _readerGen;
        readonly WriterSnippetGen _writerGen;
        readonly InterfaceSnippetGen _interfaceGen;

        public CodeGenerator(SchemaModel model, GeneratorOptions options)
        {
            _model = model;
            _names = new GenNames(options);
            _commonGen = new CommonSnippetGen(_names);
            _domClassGen = new DomainClassSnippetGen(_names);
            _readerGen = new ReaderSnippetGen(_names);
            _writerGen = new WriterSnippetGen(_names);
            _interfaceGen = new InterfaceSnippetGen(_names);
        }

        IEnumerable<MemberDeclarationSyntax> TransformEnum(TypeDefinition def)
        {
            yield return _commonGen.MakeEnum(def);
        }

        IEnumerable<TypeParameterSyntax> MakeTypeParameters(TypeDefinition def)
        {
            foreach (string name in def.GenericParameters)
            {
                yield return TypeParameter(_names.GetGenericTypeParameter(name).Identifier);
            }
        }

        IEnumerable<TypeParameterConstraintClauseSyntax> MakeTypeParameterConstraints(TypeDefinition def)
        {
            foreach (string name in def.GenericParameters)
            {
                yield return TypeParameterConstraintClause(
                    _names.GetGenericTypeParameter(name).IdentifierName)
                        .AddConstraints(ClassOrStructConstraint(SyntaxKind.ClassConstraint));
            }
        }

        IEnumerable<MemberDeclarationSyntax> TransformStruct(TypeDefinition def)
        {
            var topDecl = ClassDeclaration(_names.MakeTypeName(def).Identifier)                
                .AddModifiers(Public)
                .AddBaseListTypes(SimpleBaseType(Type<Capnp.ICapnpSerializable>()));

            if (def.GenericParameters.Count > 0)
            {
                topDecl = topDecl
                    .AddTypeParameterListParameters(MakeTypeParameters(def).ToArray())
                    .AddConstraintClauses(MakeTypeParameterConstraints(def).ToArray());
            }

            if (def.UnionInfo != null)
            {
                topDecl = topDecl.AddMembers(_commonGen.MakeUnionSelectorEnum(def));
            }

            topDecl = topDecl.AddMembers(_domClassGen.MakeDomainClassMembers(def));
            topDecl = topDecl.AddMembers(_readerGen.MakeReaderStruct(def));
            topDecl = topDecl.AddMembers(_writerGen.MakeWriterStruct(def));

            foreach (var nestedGroup in def.NestedGroups)
            {
                topDecl = topDecl.AddMembers(Transform(nestedGroup).ToArray());
            }

            foreach (var nestedDef in def.NestedTypes)
            {
                topDecl = topDecl.AddMembers(Transform(nestedDef).ToArray());
            }

            yield return topDecl;
        }

        IEnumerable<MemberDeclarationSyntax> TransformInterface(TypeDefinition def)
        {
            yield return _interfaceGen.MakeInterface(def);
            yield return _interfaceGen.MakeProxy(def);
            yield return _interfaceGen.MakeSkeleton(def);

            if (_interfaceGen.RequiresPipeliningSupport(def))
            {
                yield return _interfaceGen.MakePipeliningSupport(def);
            }

            if (def.NestedTypes.Count > 0)
            {
                var ns = ClassDeclaration(
                    _names.MakeTypeName(def, NameUsage.Namespace).ToString())
                    .AddModifiers(Public, Static);

                if (def.GenericParameters.Count > 0)
                {
                    ns = ns
                        .AddTypeParameterListParameters(MakeTypeParameters(def).ToArray())
                        .AddConstraintClauses(MakeTypeParameterConstraints(def).ToArray());
                }

                foreach (var nestedDef in def.NestedTypes)
                {
                    ns = ns.AddMembers(Transform(nestedDef).ToArray());
                }

                yield return ns;
            }
        }

        IEnumerable<MemberDeclarationSyntax> Transform(TypeDefinition def)
        {
            switch (def.Tag)
            {
                case TypeTag.Enum:
                    return TransformEnum(def);

                case TypeTag.Group:
                case TypeTag.Struct:
                    return TransformStruct(def);

                case TypeTag.Interface:
                    return TransformInterface(def);

                default:
                    throw new NotSupportedException($"Cannot declare type of kind {def.Tag} here");
            }
        }

        string Transform(GenFile file)
        {
            if (file.Namespace != null)
            {
                _names.TopNamespace = IdentifierName(MakeCamel(file.Namespace[0]));

                foreach (string name in file.Namespace.Skip(1))
                {
                    var temp = IdentifierName(MakeCamel(name));
                    _names.TopNamespace = QualifiedName(_names.TopNamespace, temp);
                }
            }

            var ns = NamespaceDeclaration(_names.TopNamespace);

            foreach (var def in file.NestedTypes)
            {
                ns = ns.AddMembers(Transform(def).ToArray());
            }

            var cu = CompilationUnit().AddUsings(
                UsingDirective(ParseName("Capnp")),
                UsingDirective(ParseName("Capnp.Rpc")),
                UsingDirective(ParseName("System")),
                UsingDirective(ParseName("System.Collections.Generic")),
                UsingDirective(ParseName("System.Threading")),
                UsingDirective(ParseName("System.Threading.Tasks")));

            cu = cu.AddMembers(ns);

            return cu.NormalizeWhitespace().ToFullString();
        }

        public void Generate()
        {
            foreach (var file in _model.FilesToGenerate)
            {
                string content = Transform(file);
                string path = Path.ChangeExtension(file.Name, ".cs");
                File.WriteAllText(path, content);
            }
        }
    }
}
