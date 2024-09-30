Source Generator Guide
======================

This guide explains how the source generator works, focusing on the general mechanism. It's intended for you to easily revisit the key concepts if you want to modify or extend the source generator in the future.

1\. What is a Source Generator?
-------------------------------

A **source generator** is a feature in C# that allows you to inspect your existing code during compilation and **generate new C# code** based on that inspection. The generated code is compiled alongside your manually written code, which helps automate repetitive tasks like property generation, validation, or even entire methods.

2\. Key Concepts of Source Generators
-------------------------------------

### 2.1. **Source Generator Structure**

A source generator is a class that implements the `ISourceGenerator` interface. This interface provides two methods:

-   **`Initialize`**: This method is called when the source generator is first initialized. It's typically used to register callbacks or perform setup actions.

-   **`Execute`**: This is the core of the generator. It's called every time the code is compiled, and it contains the logic to analyze the syntax tree and generate new code.

Here's an example:

```
[Generator]
public class PropertyGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // Initialization logic (if needed)
    }

    public void Execute(GeneratorExecutionContext context)
    {
        // Code generation logic
    }
}
```

### 2.2. **How the Generator Works**

In the `Execute` method, the source generator inspects your existing code by traversing the **syntax trees** of your C# files.

#### 2.2.1 **Syntax Trees** and **Semantic Model**

-   **Syntax Trees**: Each C# file is parsed into a syntax tree, which is a hierarchical representation of the code. The source generator can inspect these trees to find certain elements like classes, properties, and attributes.

    Example of finding class declarations in a syntax tree:


```
  foreach (var syntaxTree in context.Compilation.SyntaxTrees)
    {
        var root = syntaxTree.GetRoot();
        var classDeclarations = root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>(); // Find class declarations
    }`
```

-   **Semantic Model**: To gain a deeper understanding of the code (such as retrieving types, methods, and symbols), we use the **semantic model**. The semantic model provides additional information about the syntax nodes.

    Example of retrieving the semantic model:


```
var semanticModel = context.Compilation.GetSemanticModel(syntaxTree);
```

The **semantic model** allows you to resolve syntax nodes to **symbols**, which represent the actual code elements (like classes, properties, methods) in the compilation.


#### 2.2.2 Class Declarations

A **class declaration** refers to how classes are identified in the syntax tree. Using Roslyn, we search for `ClassDeclarationSyntax` nodes, which represent all classes in the current code base.

```
var classDeclarations = root.DescendantNodes()
    .OfType<ClassDeclarationSyntax>()
    .Where(c => c.AttributeLists.Count > 0); // Find class declarations with attributes
```

Here, we retrieve all class declarations that have attributes applied to them, as they are of interest for code generation.


#### 2.2.3 **What is `classSymbol`?**

The `classSymbol` represents the **symbol** of the class that we're inspecting. A **symbol** is an abstraction in Roslyn that represents a type (like a class, struct, or interface), method, or property.


```
var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;
```

-   The `classSymbol` is an `INamedTypeSymbol`, which specifically represents a **class**, **interface**, **struct**, or **enum** symbol.

By analyzing this `classSymbol`, we can access details about the class, such as its name, properties, methods, and even its base types.

For example, if you want to generate code based on properties defined in this class, you can extract them from the `classSymbol` like this:


```
var properties = classSymbol.GetMembers()
    .OfType<IPropertySymbol>()
    .Select(p => p.Name);  // Get property names from the class symbol
```

The `classSymbol` essentially acts as a bridge between the syntax (the raw text/code) and the semantic meaning of the class (its properties, methods, etc.).

#### 2.2.4 **Class and Attribute Detection**

You can also inspect **attributes** applied to the class, and use them to determine whether or not to generate code.

Example of checking for a specific attribute on a class:


```
var classAttributes = classSymbol.GetAttributes()
    .Where(ad => ad.AttributeClass?.ToDisplayString() == "YourNamespace.AutoGeneratePropertiesAttribute");
```

### 2.3. **Code Generation**

Once the generator identifies where code should be added (for example, based on class attributes), it proceeds to **generate new code**.

#### Property Generation

If the generator needs to generate properties, it could use a method like this:


```cs
public string GenerateProperties(List<string> properties)
{
    var sb = new StringBuilder();

    foreach (var property in properties)
    {
        sb.AppendLine($@"
    public {property.Type} {property.Name}
    {{
        get => _{property.Name};
        set => _{property.Name} = value;
    }}
    private {property.Type} _{property.Name};");
    }

    return sb.ToString();
}
```

This code takes a list of properties (which might be gathered from attributes) and generates property code for each of them.

### 2.4. **Adding the Generated Code**

Once the generator has created the necessary C# code, it can add it to the compilation using `context.AddSource`. This method creates a new `.cs` file with the generated code:


```
context.AddSource($"{classSymbol.Name}_Generated.cs", generatedCode);
```

The generated file becomes part of the final compiled project.

3\. Validations and Diagnostics
-------------------------------

A key feature of source generators is the ability to **validate generated code** and report **diagnostics** (errors or warnings) if something is wrong.

For example, you can validate generated properties:

```
public List<string> GetValidationForProperty(string propertyType, string propertyName)
{
    var validations = new List<string>();

    if (propertyType == "int")
    {
        validations.Add($"if (value < 0) throw new ArgumentOutOfRangeException(nameof({propertyName}), \"{propertyName} must be non-negative.\");");
    }

    return validations;
}
```

You can also report diagnostics if there's an issue (e.g., if a property already exists in the class):


```
context.ReportDiagnostic(Diagnostic.Create(
    new DiagnosticDescriptor("BGA001", "Property Defined", $"Property '{propertyName}' is already defined.", "SourceGenerator", DiagnosticSeverity.Error, true),
    classDeclaration.GetLocation()));
```

4\. Behind the Scenes: Compilation Process
------------------------------------------

Here's what happens when you build your project with a source generator:

1.  **Source Generator Initialization**: The generator is initialized (via `Initialize`).
2.  **Code Inspection**: The generator inspects your existing code (via `Execute`), analyzing the syntax trees and semantic models.
3.  **Code Generation**: Based on the analysis, the generator produces new source code and injects it into the project.
4.  **Compilation**: The generated code is compiled along with the rest of the project, as if it were part of the original source.

### Example Flow:

-   You create a class `MyClass` with the attribute `[AutoGenerateProperties]`.
-   The source generator detects the attribute, generates the specified properties, and injects them into a partial class.
-   The project compiles the generated code and your manual code together.

5\. Best Practices and Tips
---------------------------

-   **Avoid Overcomplicating**: Keep the generator focused on one task. In this case, generating properties and validation methods.
-   **Testing**: Use unit tests to validate the generated code. You can use snapshot tests or compare the generated code to expected outputs.
-   **Debugging**: Debugging source generators can be tricky. You can use logging (`context.ReportDiagnostic`) or run the generator in a separate project for better visibility.