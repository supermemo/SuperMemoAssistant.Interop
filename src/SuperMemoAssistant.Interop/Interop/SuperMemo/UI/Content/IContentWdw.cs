using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Interop.SuperMemo.UI.Content
{
  public interface IContentWdw : IWdw
  {
    /// <summary>
    /// Moves an Element of it's respective id into a target Concept ID
    /// </summary>
    /// <param name="elementId">The ID of the element to move.</param>
    /// <param name="conceptId">The target concpet the element is moved to.</param>
    /// <returns></returns>
    public bool MoveElementToConcept(int elementId, int conceptId);

  }
}
