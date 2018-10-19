﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace docgen
{
  static class MethodDeclarationExtensions
  {
    public static bool IsStatic(this MethodDeclarationSyntax method)
    {
      foreach (var modifier in method.Modifiers)
      {
        if (modifier.Text == "static")
          return true;
      }
      return false;
    }

    public static bool IsStatic(this PropertyDeclarationSyntax method)
    {
      foreach (var modifier in method.Modifiers)
      {
        if (modifier.Text == "static")
          return true;
      }
      return false;
    }

    public static bool IsPublic(this ConstructorDeclarationSyntax c)
    {
      foreach (var modifier in c.Modifiers)
      {
        if (modifier.Text == "public")
          return true;
      }
      return false;
    }

    public static bool IsPublic(this MethodDeclarationSyntax method)
    {
      foreach (var modifier in method.Modifiers)
      {
        if (modifier.Text == "public")
          return true;
      }
      return false;
    }

    public static bool IsPublic(this PropertyDeclarationSyntax p)
    {
      foreach (var modifier in p.Modifiers)
      {
        if (modifier.Text == "public")
          return true;
      }
      return false;
    }
  }

  class RhinoCommonClass
  {
    public static Dictionary<string, RhinoCommonClass> AllClasses { get; private set; }

    public static void BuildClassDictionary(string sourcePath)
    {
      AllClasses = new Dictionary<string, RhinoCommonClass>();
      var options = new Microsoft.CodeAnalysis.CSharp.CSharpParseOptions().WithDocumentationMode(Microsoft.CodeAnalysis.DocumentationMode.Parse);
      foreach (var file in AllSourceFiles(sourcePath))
      {
        if (System.IO.Path.GetFileName(file).StartsWith("auto", StringComparison.OrdinalIgnoreCase))
          continue;
        string text = System.IO.File.ReadAllText(file);
        if (!text.Contains("Geometry"))
          continue;

        Console.WriteLine($"parse: {file}");
        var tree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(text, options);
        SourceFileWalker sfw = new SourceFileWalker();
        sfw.Construct(tree.GetRoot());
      }

    }

    public static RhinoCommonClass Get(string className)
    {
      className = className.ToLowerInvariant();
      RhinoCommonClass cb;
      if (AllClasses.TryGetValue(className, out cb))
        return cb;
      cb = new RhinoCommonClass(className);
      AllClasses[className] = cb;
      return cb;
    }

    static IEnumerable<string> AllSourceFiles(string sourcePath)
    {
      foreach (string file in System.IO.Directory.EnumerateFiles(sourcePath, "*.cs", System.IO.SearchOption.AllDirectories))
      {
        if (file.Contains("\\obj\\"))
          continue;
        yield return file;
      }
    }

    private RhinoCommonClass(string rhinocommonClassName)
    {
      FullClassName = rhinocommonClassName;
    }

    public string FullClassName { get; set; }
    public string ClassName
    {
      get
      {
        string s = FullClassName;
        int index = s.LastIndexOf('.');
        return s.Substring(index + 1);
      }
    }

    public void AddClassComment(DocumentationCommentTriviaSyntax docComment)
    {
      DocComment = docComment;
    }
    public DocumentationCommentTriviaSyntax DocComment { get; set; }
    public List<Tuple<ConstructorDeclarationSyntax, DocumentationCommentTriviaSyntax>> Constructors { get; } = new List<Tuple<ConstructorDeclarationSyntax, DocumentationCommentTriviaSyntax>>();
    public List<Tuple<MethodDeclarationSyntax, DocumentationCommentTriviaSyntax>> Methods { get; } = new List<Tuple<MethodDeclarationSyntax, DocumentationCommentTriviaSyntax>>();
    public List<Tuple<PropertyDeclarationSyntax, DocumentationCommentTriviaSyntax>> Properties { get; } = new List<Tuple<PropertyDeclarationSyntax, DocumentationCommentTriviaSyntax>>();
  }


  class SourceFileWalker : Microsoft.CodeAnalysis.CSharp.CSharpSyntaxWalker
  {
    readonly List<Microsoft.CodeAnalysis.Text.TextSpan> _rhinoSdkSpans = new List<Microsoft.CodeAnalysis.Text.TextSpan>();
    string _visitingClass = null;
    int _activeSpanStart;

    public SourceFileWalker() : base(Microsoft.CodeAnalysis.SyntaxWalkerDepth.StructuredTrivia)
    {
    }

    bool _buildingSpans = true;
    public void Construct(Microsoft.CodeAnalysis.SyntaxNode node)
    {
      _buildingSpans = true;
      Visit(node);
      _buildingSpans = false;
      Visit(node);
    }

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
      string className = node.Identifier.ToString();
      _visitingClass = className;
      var docComment = node.GetLeadingTrivia().Select(i => i.GetStructure()).OfType<DocumentationCommentTriviaSyntax>().FirstOrDefault();
      RhinoCommonClass.Get(className).AddClassComment(docComment);
      base.VisitClassDeclaration(node);
    }

    public override void VisitStructDeclaration(StructDeclarationSyntax node)
    {
      NamespaceDeclarationSyntax ns = node.Parent as NamespaceDeclarationSyntax;
      string className = node.Identifier.ToString();
      _visitingClass = className;
      base.VisitStructDeclaration(node);
    }

    public override void VisitIfDirectiveTrivia(IfDirectiveTriviaSyntax node)
    {
      _activeSpanStart = node.SpanStart;
      base.VisitIfDirectiveTrivia(node);
    }
    public override void VisitEndIfDirectiveTrivia(EndIfDirectiveTriviaSyntax node)
    {
      if (_buildingSpans)
      {
        var span = new Microsoft.CodeAnalysis.Text.TextSpan(_activeSpanStart, node.Span.End - _activeSpanStart);
        _rhinoSdkSpans.Add(span);
      }
      base.VisitEndIfDirectiveTrivia(node);
    }

    bool InSpans(Microsoft.CodeAnalysis.Text.TextSpan span)
    {
      for (int i = 0; i < _rhinoSdkSpans.Count; i++)
      {
        if (_rhinoSdkSpans[i].IntersectsWith(span))
          return true;
      }
      return false;
    }

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
      if (!_buildingSpans)
      {
        bool isPublic = node.IsPublic();
        bool isStatic = node.IsStatic();

        if (isPublic)
        {
          // skip methods with ref parameters of multiple out parameters for now
          int refCount = 0;
          int outCount = 0;
          foreach (var parameter in node.ParameterList.Parameters)
          {
            foreach (var modifier in parameter.Modifiers)
            {
              if (modifier.Text == "ref")
                refCount++;
              if (modifier.Text == "out")
                outCount++;
            }
          }

          if (refCount == 0 && outCount < 2)
          {
            var docComment = node.GetLeadingTrivia().Select(i => i.GetStructure()).OfType<DocumentationCommentTriviaSyntax>().FirstOrDefault();
            RhinoCommonClass.Get(_visitingClass).Methods.Add(new Tuple<MethodDeclarationSyntax, DocumentationCommentTriviaSyntax>(node, docComment));
          }
        }
      }
      base.VisitMethodDeclaration(node);
    }

    public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
      if (!_buildingSpans)
      {
        bool isPublic = node.IsPublic();
        bool isStatic = node.IsStatic();

        if (isPublic)
        {
          var docComment = node.GetLeadingTrivia().Select(i => i.GetStructure()).OfType<DocumentationCommentTriviaSyntax>().FirstOrDefault();
          RhinoCommonClass.Get(_visitingClass).Properties.Add(new Tuple<PropertyDeclarationSyntax, DocumentationCommentTriviaSyntax>(node, docComment));
        }
      }
      base.VisitPropertyDeclaration(node);
    }

    public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
      if (!_buildingSpans)
      {
        bool isPublic = node.IsPublic();

        if (isPublic)
        {
          var docComment = node.GetLeadingTrivia().Select(i => i.GetStructure()).OfType<DocumentationCommentTriviaSyntax>().FirstOrDefault();
          RhinoCommonClass.Get(_visitingClass).Constructors.Add(new Tuple<ConstructorDeclarationSyntax, DocumentationCommentTriviaSyntax>(node, docComment));
        }
      }
      base.VisitConstructorDeclaration(node);
    }
  }
}
