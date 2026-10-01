using Blackhole.GlobalDialogue.Contract;
using Blackhole.SharedKernel.Contract;
using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;

namespace Blackhole.GlobalDialogue.Application
{
    public class ConfirmationPanelService : IShowConfirmationPanel
    {
        private readonly IScopedHandlerRegistery<IConfirmatinOutput> _confirmOutputHandler;

        public ConfirmationPanelService(IScopedHandlerRegistery<IConfirmatinOutput> confirmOutputHandler)
        {
            _confirmOutputHandler = confirmOutputHandler ?? throw new ArgumentNullException(nameof(confirmOutputHandler));
        }

        public UniTask<ConfirmationResult> ShowYesOrNoPanel(YesOrNoPanelSetting setting)
        {
            if (_confirmOutputHandler.TryGet(out var output))
            {
                return output.ShowYesOrNoPanel(setting);
            }

            return UniTask.FromResult(ConfirmationResult.NoRespons);
        }

        public UniTask<ConfirmationResult> ShowConfirmPanel(ConfirmationPanelSetting setting)
        {
            if (_confirmOutputHandler.TryGet(out var output))
            {
                return output.ShowConfirmPanel(setting);
            }

            return UniTask.FromResult(ConfirmationResult.NoRespons);
        }
    }
}