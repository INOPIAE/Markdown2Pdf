<p align="center">
  <a href="https://www.nuget.org/packages/Markdown2Pdf" target="_blank">
    <img alt="Nuget" src="https://img.shields.io/nuget/v/Markdown2Pdf">
  </a>
  
  <a href="https://github.com/Flayms/Markdown2Pdf/actions/workflows/build-and-release.yml" target="_blank">
    <img src="https://github.com/Flayms/Markdown2Pdf/actions/workflows/build-and-release.yml/badge.svg?event=workflow_dispatch" alt="Build and Release" />
  </a>
</p>


# Markdown2Pdf
.NET library for converting Markdown to PDF. Uses [markdig](https://github.com/xoofx/markdig) for converting markdown to html and then [puppeteer-sharp](https://github.com/hardkoded/puppeteer-sharp) to convert that output to PDF. For a cross-platform console-application checkout [Markdown2Pdf.Console](https://github.com/Flayms/Markdown2Pdf.Console).

## Usage

```c#
var converter = new Markdown2PdfConverter();
converter.Convert("README.md");
```
