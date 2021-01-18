﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AsyncFixer
{
    public static class Helpers
    {
        public static bool IsTaskCreationMethod(this IMethodSymbol symbol)
        {
            return symbol.ToString().Contains("System.Threading.Tasks.Task.Start")
                   || symbol.ToString().Contains("System.Threading.Tasks.Task.Run")
                   || symbol.ToString().Contains("System.Threading.Tasks.TaskFactory.StartNew")
                   || symbol.ToString().Contains("System.Threading.Tasks.TaskEx.RunEx")
                   || symbol.ToString().Contains("System.Threading.Tasks.TaskEx.Run")
                   || symbol.ToString().Contains("StartNewTask")
                   || symbol.ToString().Contains("StartNewTaskWithoutExceptionHandling");
        }

        public static bool IsTask(this ITypeSymbol type)
        {
            return type.ContainingNamespace.ToDisplayString() == "System.Threading.Tasks" &&
                type.Name == "Task";
        }

        public static bool ReturnTask(this IMethodSymbol symbol)
        {
            return !symbol.ReturnsVoid && symbol.ReturnType.IsTask();
        }

        public static bool IsAsync(this MethodDeclarationSyntax method)
        {
            return method.Modifiers.ToString().Contains("async");
        }

        public static bool IsTestMethod(this MethodDeclarationSyntax method)
        {
            return method.AttributeLists.Any(a => a.Attributes.ToString().Contains("TestMethod"));
        }

        public static bool HasEventArgsParameter(this MethodDeclarationSyntax method)
        {
            return method.ParameterList != null &&
                   method.ParameterList.Parameters.Any(param => param.Type.ToString().EndsWith("EventArgs", StringComparison.OrdinalIgnoreCase));
        }

        public static bool HasObjectStateParameter(this MethodDeclarationSyntax method)
        {
            // If method in this form async void Xyz(object state) { ..}, ignore it!
            return method.ParameterList != null && method.ParameterList.Parameters.Count == 1 &&
                   method.ParameterList.Parameters.First().Type.ToString() == "object";
        }

        public static bool ReturnsVoid(this MethodDeclarationSyntax method)
        {
            return method.ReturnType.ToString() == "void";
        }

        public static bool IsImplicitTypeCasting(this TypeInfo typeInfo)
        {
            return !SymbolEqualityComparer.Default.Equals(typeInfo.Type, typeInfo.ConvertedType);
        }

        public static T FirstAncestorOrSelfUnderGivenNode<T>(this SyntaxNode node, SyntaxNode parent)
            where T : SyntaxNode
        {
            var current = node;

            while (current != null && current != parent)
            {
                var temp = current as T;
                if (temp != null)
                {
                    return temp;
                }

                current = current.Parent;
            }

            return null;
        }

        /// <summary>
        /// Replace all old nodes in the given pairs with their corresponding new nodes.
        /// </summary>
        /// <typeparam name="T">
        /// Subtype of SyntaxNode that supports the
        /// replacement of descendent nodes.
        /// </typeparam>
        /// <param name="node">
        /// The SyntaxNode or subtype to operate on.
        /// </param>
        /// <param name="syntaxReplacementPairs">
        /// The SyntaxNodeReplacementPair
        /// instances that each contain both the old node that is to be
        /// replaced, and the new node that will replace the old node.
        /// </param>
        /// <returns>
        /// The SyntaxNode that contains all the replacmeents.
        /// </returns>
        public static T ReplaceAll<T>(this T node, params SyntaxReplacementPair[] syntaxReplacementPairs)
            where T : SyntaxNode
        {
            return node.ReplaceNodes(
                syntaxReplacementPairs.Select(pair => pair.OldNode),
                (oldNode, newNode) => syntaxReplacementPairs.First(pair => pair.OldNode == oldNode).NewNode
                );
        }

        /// <summary>
        /// Replace all old nodes in the given pairs with their corresponding new nodes.
        /// </summary>
        /// <typeparam name="T">
        /// Subtype of SyntaxNode that supports the
        /// replacement of descendent nodes.
        /// </typeparam>
        /// <param name="node">
        /// The SyntaxNode or subtype to operate on.
        /// </param>
        /// <param name="replacementPairs">
        /// The SyntaxNodeReplacementPair
        /// instances that each contain both the old node that is to be
        /// replaced, and the new node that will replace the old node.
        /// </param>
        /// <returns>
        /// The SyntaxNode that contains all the replacmeents.
        /// </returns>
        public static T ReplaceAll<T>(this T node, IEnumerable<SyntaxReplacementPair> replacementPairs)
            where T : SyntaxNode
        {
            return node.ReplaceNodes(
                replacementPairs.Select(pair => pair.OldNode),
                (oldNode, newNode) => replacementPairs.First(pair => pair.OldNode == oldNode).NewNode
                );
        }
    }

    /// <summary>
    /// Pair of old and new SyntaxNodes for ReplaceAll.
    /// </summary>
    public sealed class SyntaxReplacementPair
    {
        /// <summary>
        /// The node that will replace the old node.
        /// </summary>
        public readonly SyntaxNode NewNode;

        /// <summary>
        /// The node that must be replaced.
        /// </summary>
        public readonly SyntaxNode OldNode;

        public SyntaxReplacementPair(SyntaxNode oldNode, SyntaxNode newNode)
        {
            if (oldNode == null) throw new ArgumentNullException(nameof(oldNode));
            if (newNode == null) throw new ArgumentNullException(nameof(newNode));

            OldNode = oldNode;
            NewNode = newNode;
        }
    }
}
