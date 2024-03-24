﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Markdig.Helpers;

namespace Markdown2Pdf.Options;

/// <inheritdoc cref="TableOfContents(bool, int)"/>
public class TableOfContents {

  private readonly struct Link(string title, string linkAddress, int Depth) {
    public string Title { get; } = title;
    public string LinkAddress { get; } = linkAddress;
    public int Depth { get; } = Depth;

    public override readonly string ToString() => $"<a href=\"{this.LinkAddress}\">{this.Title}</a>";
  }

  private readonly int _maxDepthLevel;
  private readonly bool _isOrdered;

  private const string _IDENTIFIER = "<!--TOC-->";
  private static readonly Regex _headerReg = new("^(?<hashes>#{1,6}) +(?<title>[^\r\n]*)",
    RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.ExplicitCapture);
  private static readonly Regex _htmlElementReg = new("<[^>]*>[^>]*</[^>]*>|<[^>]*/>", RegexOptions.Compiled);
  private static readonly Regex _emojiReg = new(":(\\w+):", RegexOptions.Compiled);

  /// <summary>
  /// Inserts a Table of Contents into the PDF, generated from all headers. 
  /// The TOC will be inserted into all <c>&lt;!--TOC--&gt;</c> comments within the markdown document. 
  /// </summary>
  /// <param name="isOrdered">
  /// If <see langword="true"/>, will generate an Ordered List, otherwise an Unordered List.
  /// </param>
  /// <param name="maxDepthLevel">
  /// The maximum level of heading depth to include in the TOC 
  /// (e.g. <c>3</c> will include headings up to <c>&lt;h3&gt;</c>).
  /// </param>
  public TableOfContents(bool isOrdered = true, int maxDepthLevel = 3) {
    if (maxDepthLevel is < 1 or > 6)
      throw new ArgumentOutOfRangeException();

    this._isOrdered = isOrdered;
    this._maxDepthLevel = maxDepthLevel;
  }

  private IEnumerable<Link> _CreateLinks(string markdownContent) {
    var matches = _headerReg.Matches(markdownContent);
    var links = new List<Link>(matches.Count);

    foreach (Match match in matches) {
      var depth = match.Groups["hashes"].Value.Length - 1;

      if (depth > this._maxDepthLevel)
        continue;

      // build link
      var title = match.Groups["title"].Value;
      title = _htmlElementReg.Replace(title, string.Empty);
      title = _emojiReg.Replace(title, string.Empty).Trim();

      var linkAddress = LinkHelper.Urilize(title, true);
      linkAddress = "#" + linkAddress.ToLower();

      links.Add(new Link(title, linkAddress, depth));
    }

    return links;
  }

  internal string ToHtml(string markdownContent) {
    var links = _CreateLinks(markdownContent);
    var tocBuilder = new StringBuilder();
    var lastDepth = -1; // start at -1 to open the list on first element
    var openList = this._isOrdered ? "<ol>" : "<ul>";
    var closeList = this._isOrdered ? "</ol>" : "</ul>";

    foreach (var link in links) {

      switch (link.Depth) {
        case var depth when depth > lastDepth: // nested element
          tocBuilder.AppendLine();
          tocBuilder.AppendLine(openList);

          ++lastDepth;
          break;

        case var depth when depth == lastDepth: // same height
          // close previous element
          tocBuilder.AppendLine("</li>");
          break;

        default: // depth < lastDepth
          // determine difference
          var difference = lastDepth - link.Depth;

          // close previous elements
          for (var i = 0; i < difference; ++i) {
            tocBuilder.AppendLine("</li>");
            tocBuilder.AppendLine(closeList);
          }

          tocBuilder.AppendLine("</li>");
          break;
      }

      lastDepth = link.Depth;
      tocBuilder.Append($"<li>{link}");
    }

    tocBuilder.AppendLine("</li>");
    tocBuilder.AppendLine(closeList);

    return tocBuilder.ToString();
  }

  internal void InsertInto(ref string htmlContent, string tocHtml)
    => htmlContent = htmlContent.Replace(_IDENTIFIER, tocHtml);

}
