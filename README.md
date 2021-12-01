# SuffixTreeSharp
[![Nuget](https://img.shields.io/nuget/v/SuffixTreeSharp)](https://www.nuget.org/packages/SuffixTreeSharp/)

<https://www.nuget.org/packages/SuffixTreeSharp/>

Generalized Suffix Tree in pure C#, with unit case.

Targetting .NET Standard 1.6

## Usage

```csharp
var tree = new GeneralizedSuffixTree();
tree.Put(0, "java");
tree.Put(1, "groovy");
tree.Put(2, "kotlin");

tree.Search("blabla"); // Returns a set of indices that were defined previously
```
