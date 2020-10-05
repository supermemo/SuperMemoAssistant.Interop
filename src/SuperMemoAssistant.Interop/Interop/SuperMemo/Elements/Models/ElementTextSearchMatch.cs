#region License & Metadata

// The MIT License (MIT)
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

#endregion




namespace SuperMemoAssistant.Interop.Interop.SuperMemo.Elements.Models
{
  using SuperMemoAssistant.Interop.SuperMemo.Content.Components;
  using SuperMemoAssistant.Interop.SuperMemo.Elements.Types;
  using SuperMemoAssistant.Interop.SuperMemo.Registry.Members;

  /// <summary>
  /// Represents an element matching a certain text search query
  /// </summary>
  public class ElementTextSearchMatch
  {
    #region Constructors

    /// <summary>
    /// Creates a new instance
    /// </summary>
    /// <param name="element">The element matching the query</param>
    /// <param name="componentNo">The component id of the component that contains the text in the element</param>
    /// <param name="component">The component that contains the text in the element</param>
    /// <param name="textMember">The text member containing the text mapped to the component</param>
    /// <param name="matchScore">How well the text matched the search query (higher is better)</param>
    public ElementTextSearchMatch(
      IElement       element,
      int            componentNo,
      IComponentHtml component,
      IText          textMember,
      double         matchScore)
    {
      Element     = element;
      ComponentNo = componentNo;
      Component   = component;
      TextMember  = textMember;
      MatchScore  = matchScore;
    }

    #endregion




    #region Properties & Fields - Public

    /// <summary>
    /// The element matching the query
    /// </summary>
    public IElement       Element     { get; }

    /// <summary>
    /// The component id of the component that contains the text in the element
    /// </summary>
    public int            ComponentNo { get; }

    /// <summary>
    /// The component that contains the text in the element
    /// </summary>
    public IComponentHtml Component   { get; }

    /// <summary>
    /// The text member containing the text mapped to the component
    /// </summary>
    public IText          TextMember  { get; }

    /// <summary>
    /// How well the text matched the search query (higher is better)
    /// </summary>
    public double         MatchScore  { get; }

    #endregion
  }
}
