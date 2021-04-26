# RoslynExamples

* `ExampleAnalyzerTemplate`

  Just simple analyzer example.

* `ExampleAnalyzer0000`

  Checks symbol naming rule.

* `ExampleAnalyzer0001`

  Checks symbol naming rule.

* `ExampleAnalyzer0002`

  Checks compilation naming rule.

* `ExampleCodeFixProvider`

  Provides code actions for symbols naming fixing.

* `ExampleCodeRefactoringProvider`

  Provides code actions for symbols naming refactoring.

* `ExampleSourceGenerator`

  Generates `ToString` method for each partial class. Method `ToString` returns string with type and it's members names.

* `DependenciesAnalyzer`

  Searches references on namespaces, types, members, misc.

# RoslynExamples.Tests

* `Tests_00_CodeAnalysis`

  Contains tests for: diagnostic analysis, source generation, dependencies analysis.

* `Tests_01_Workspaces`

  Contains tests for: fixing, refactoring.