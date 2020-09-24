﻿#region License & Metadata

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
// 
// 
// Created On:   2020/03/29 00:21
// Modified On:  2020/04/07 04:46
// Modified By:  Alexis

#endregion




namespace SuperMemoAssistant.Interop.SuperMemo.UI.Element
{
  using System;
  using Content.Components;
  using Content.Controls;
  using Core;
  using Elements.Models;
  using Elements.Types;
  using Learning;
  using Registry.Members;

  /// <summary>SuperMemo Element Window service</summary>
  public interface IElementWdw : IWdw
  {
    /// <summary>Brings the element window forward and focus it</summary>
    /// <returns>Success of operation</returns>
    bool ActivateWindow();

    /// <summary>
    ///   The components controls loaded in the the element window. <see cref="IControl" /> are live instances of
    ///   <see cref="IComponent" />. <see cref="IComponent" /> are definitions of what kind of content your elements hold (e.g.
    ///   html component, image component, etc.) and how SuperMemo persist these definitions to disk.
    /// </summary>
    IControlGroup ControlGroup { get; }

    /// <summary>The currently displayed <see cref="IElement" />'s id</summary>
    int CurrentElementId { get; }

    /// <summary>The currently displayed <see cref="IElement" /></summary>
    IElement CurrentElement { get; }

    /// <summary>The current maximum children per branch limit (SM > Options > SuperMemo > Children per branch)</summary>
    short LimitChildrenCount { get; }

    /// <summary>The current <see cref="IConceptGroup" /> (i.e. an element which is also a Concept)</summary>
    int CurrentConceptGroupId { get; }

    /// <summary>The current root id (resolves to an <see cref="IElement" />)</summary>
    int CurrentRootId { get; set; }

    /// <summary>The current hook id (resolves to an <see cref="IElement" />)</summary>
    int CurrentHookId { get; set; }

    /// <summary>The current <see cref="IConcept" /> id</summary>
    int CurrentConceptId { get; }

    /// <summary>The active learning mode (e.g. standard, neural review)</summary>
    LearningMode CurrentLearningMode { get; }

    /// <summary>Changes the current <see cref="IConcept" /></summary>
    /// <param name="conceptId">The new concept's id</param>
    /// <returns>Success of operation</returns>
    bool SetCurrentConcept(int conceptId);

    /// <summary>
    ///   Requests element <paramref name="elementId" /> to be displayed in the <see cref="IElementWdw" />. Equivalent to using
    ///   Ctrl+G in SuperMemo.
    /// </summary>
    /// <param name="elementId">The new element id to display</param>
    /// <returns>Success of operation</returns>
    bool GoToElement(int elementId);

    /// <summary>Pastes your clipboard's content as a new element. Not recommended</summary>
    /// <returns>Success of operation</returns>
    [Obsolete("Not reliable, use the ElementRegistry.Add instead")]
    bool PasteArticle();

    /// <summary>
    ///   Assumes your clipboard is in the form of an element definition (e.g. Copy Element Info in SM) and uses it to create a
    ///   new element. Not recommended
    /// </summary>
    /// <returns>Success of operation</returns>
    [Obsolete("Not reliable, use the ElementRegistry.Add instead")]
    bool PasteElement();

    /// <summary>Creates a new, empty element of type <paramref name="elementType" /></summary>
    /// <returns>The new element's id, or -1 if the operation failed</returns>
    int AppendElement(ElementType elementType);

    /// <summary>
    ///   Assumes <paramref name="elementDesc" /> is in the form of an element definition (e.g. Copy Element Info in SM) and
    ///   uses it to create a new element. Not recommended
    /// </summary>
    /// <returns>Success of operation</returns>
    [Obsolete("Low level access, use the ElementRegistry.Add instead")]
    bool AddElementFromText(string elementDesc);

    /// <summary>Makes an Extract from the current selection. Equivalent to pressing Alt+X in SuperMemo</summary>
    /// <param name="elementType">The element type to create</param>
    /// <param name="memorize">Whether to create the element as memorized</param>
    /// <param name="askUserToScheduleInterval">Whether to show a popup asking the user to schedule his time</param>
    /// <returns>The element id</returns>
    int GenerateExtract(ElementType elementType,
                        bool        memorize                  = true,
                        bool        askUserToScheduleInterval = false);

    /// <summary>Creates a Cloze from the current selection. Equivalent to pressing Alt+Z in SuperMemo</summary>
    /// <param name="memorize">Whether to create the element as memorized</param>
    /// <param name="askUserToScheduleInterval">Whether to show a popup asking the user to schedule his time</param>
    /// <returns>The element id</returns>
    int GenerateCloze(bool memorize                  = true,
                      bool askUserToScheduleInterval = false);

    /// <summary>Deletes the currently displayed element</summary>
    /// <returns>Success of operation</returns>
    bool Delete();

    /// <summary>Removes the content from the current element and removes it from the learning queue.</summary>
    /// <returns>Success of operation</returns>
    bool Done();

    /// <summary>Raises an event when the element in the Element window changes</summary>
    event Action<SMDisplayedElementChangedEventArgs> OnElementChanged;

    /// <summary>
    ///   Displays the next element in the learning queue. This might not always work as intended depending on the current
    ///   <see cref="LearningMode" />
    /// </summary>
    /// <returns>Success of operation</returns>
    bool NextElementInLearningQueue();

    /// <summary>Changes the element state</summary>
    /// <param name="state">The new state</param>
    /// <returns>Success of operation</returns>
    bool SetElementState(ElementDisplayState state);

    /// <summary>Postpones the current element for <paramref name="interval" /> days</summary>
    /// <param name="interval">The new interval</param>
    /// <returns>Success of operation</returns>
    bool PostponeRepetition(int interval);

    /// <summary>
    ///   Executes a repetition on the current element and postpones it for <paramref name="interval" /> days. If
    ///   <paramref name="adjustPriority" /> is true, also changes the priority depending on the given interval
    /// </summary>
    /// <param name="interval">The new interval</param>
    /// <param name="adjustPriority">Whether to adjust the priority on the interval</param>
    /// <returns>Success of operation</returns>
    bool ForceRepetition(int interval, bool adjustPriority);

    /// <summary>
    ///   Executes a repetition on the current element and postpones it for <paramref name="interval" /> days ; then resumes
    ///   the learning process. If <paramref name="adjustPriority" /> is true, also changes the priority depending on the given
    ///   interval
    /// </summary>
    /// <param name="interval">The new interval</param>
    /// <param name="adjustPriority">Whether to adjust the priority on the interval</param>
    /// <returns>Success of operation</returns>
    bool ForceRepetitionAndResume(int interval, bool adjustPriority);

    /// <summary>
    ///   Uses the LockWindowUpdate function in the Win32 API to lock the UI. This is used by SM when you create a new element 
    ///   - SM locks the UI and switches to the new element in the background while the window is locked.
    /// </summary>
    /// <returns>Success of operation</returns>
    public bool EnterSMUpdateLock()

    /// <summary>
    ///   Uses the LockWindowUpdate function in the Win32 API to release the UI lock. This is used by SM when you create a new element 
    ///   - SM locks the UI and switches to the new element in the background while the window is locked.
    /// </summary>
    /// <returns>Success of operation</returns>
    public bool QuitSMUpdateLock()
  }
}
