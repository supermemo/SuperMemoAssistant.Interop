using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Interop.SuperMemo.Content.Controls
{
  using Registry.Members;

  public interface IControlSound : IControl
  {
    ISound SoundMember { get; set; }

    int SoundMemberId { get; set; }
  }
}
