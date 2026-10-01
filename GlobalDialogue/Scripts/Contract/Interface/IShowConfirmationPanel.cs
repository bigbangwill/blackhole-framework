using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

namespace Blackhole.GlobalDialogue.Contract
{
    public interface IShowConfirmationPanel
    {
        UniTask<ConfirmationResult> ShowYesOrNoPanel(YesOrNoPanelSetting setting);

        UniTask<ConfirmationResult> ShowConfirmPanel(ConfirmationPanelSetting setting);

    }
}