using Blackhole.GlobalDialogue.Contract;
using Cysharp.Threading.Tasks;

namespace Blackhole.GlobalDialogue.Application
{
    public interface IConfirmatinOutput
    {
        UniTask<ConfirmationResult> ShowYesOrNoPanel(YesOrNoPanelSetting setting);
        UniTask<ConfirmationResult> ShowConfirmPanel(ConfirmationPanelSetting setting);
    }
}
