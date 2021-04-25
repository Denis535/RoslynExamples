# RoslynExamples

* ExampleAnalyzerTemplate
Just analyzer example.

* ExampleAnalyzer0000
Analyzer checks symbol naming rule.

* ExampleAnalyzer0001
Analyzer checks symbol naming rule.

* ExampleAnalyzer0002
Analyzer checks compilation naming rule.

* ExampleCodeFixProvider
Provides code fix actions for: `ExampleAnalyzer0000` and `ExampleAnalyzer0001`. 

* ExampleCodeRefactoringProvider
Provides code refactoring actions for symbols names refactoring.

* ExampleSourceGenerator
Generates `ToString` method for each partial class. Method `ToString` returns string with type and it's members names.

# RoslynExamples.Tests

* Tests_00_CodeAnalysis
Contains tests for: diagnostic analysis, source generation, dependencies analysis.

* Tests_01_Workspaces
Contains tests for: fixing, refactoring.