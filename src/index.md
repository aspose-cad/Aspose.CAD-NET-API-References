---
_layout: landing
---

# Welcome to Aspose.CAD for .NET API version 24.8

Aspose.CAD for .NET is a cross-platform SDK library offering export, editing, conversion and other features for CAD and BIM drawings. This site provides quick access to essential resources. Visit the [API Documentation](/aspose.cad) for detailed information.

## Quick Start Guide

Get started with Aspose.CAD for .NET SDK in minutes. Follow the steps below to set up and run your first project. Detailed examples can be found in the [API Documentation](/aspose.cad).

### Installation

1. Install Aspose.CAD for .NET from NuGet:
    ```sh
    Install-Package Aspose.CAD
    ```

2. Add the library to your project:
    ```csharp
    using Aspose.CAD;
    ```

### Basic Usage

Convert a CAD file to PDF:
```csharp
string sourceFilePath = "example.dwg";
string outputFilePath = "output.pdf";

using (CadImage cadImage = (CadImage)Image.Load(sourceFilePath))
{
    PdfOptions pdfOptions = new PdfOptions();
    cadImage.Save(outputFilePath, pdfOptions);
}
```

version: f76d838cf6f0ebb081f2e8bc521969876370ebb0
