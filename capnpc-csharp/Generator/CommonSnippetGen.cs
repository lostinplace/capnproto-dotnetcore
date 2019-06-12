﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CapnpC.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static CapnpC.Generator.SyntaxHelpers;

namespace CapnpC.Generator
{
    class CommonSnippetGen
    {
        readonly GenNames _names;

        public CommonSnippetGen(GenNames names)
        {
            _names = names;
        }

        public EnumDeclarationSyntax MakeUnionSelectorEnum(TypeDefinition def)
        {
            var whichEnum = EnumDeclaration(_names.UnionDiscriminatorEnum.ToString())
                .AddModifiers(Public)
                .AddBaseListTypes(SimpleBaseType(Type<ushort>()));

            var discFields = def.Fields.Where(f => f.DiscValue.HasValue);

            foreach (var discField in discFields)
            {
                whichEnum = whichEnum.AddMembers(
                    EnumMemberDeclaration(_names.GetCodeIdentifier(discField).Identifier)
                        .WithEqualsValue(
                            EqualsValueClause(LiteralExpression(
                                SyntaxKind.NumericLiteralExpression,
                                Literal(discField.DiscValue.Value)))));
            }

            var ndecl = EnumMemberDeclaration(_names.UnionDiscriminatorUndefined.ToString()).WithEqualsValue(
                EqualsValueClause(
                    LiteralExpression(
                        SyntaxKind.NumericLiteralExpression,
                        Literal(Schema.Field.Reader.NoDiscriminant))));

            whichEnum = whichEnum.AddMembers(ndecl);

            return whichEnum;
        }

        public EnumDeclarationSyntax MakeEnum(TypeDefinition def)
        {
            var decl = EnumDeclaration(def.Name)
                .AddModifiers(Public)
                .AddBaseListTypes(SimpleBaseType(Type<ushort>()));

            foreach (var enumerant in def.Enumerants.OrderBy(e => e.CodeOrder))
            {
                var mdecl = EnumMemberDeclaration(enumerant.Literal);

                if (enumerant.Ordinal.HasValue)
                {
                    mdecl = mdecl.WithEqualsValue(
                        EqualsValueClause(
                            LiteralExpression(
                                SyntaxKind.NumericLiteralExpression,
                                Literal(enumerant.Ordinal.Value))));
                }

                decl = decl.AddMembers(mdecl);
            }

            return decl;
        }

        public static IEnumerable<SyntaxNodeOrToken> MakeCommaSeparatedList(IEnumerable<ExpressionSyntax> expressions)
        {
            bool first = true;

            foreach (var expr in expressions)
            {
                if (first)
                    first = false;
                else
                    yield return Token(SyntaxKind.CommaToken);

                yield return expr;
            }
        }

    }
}