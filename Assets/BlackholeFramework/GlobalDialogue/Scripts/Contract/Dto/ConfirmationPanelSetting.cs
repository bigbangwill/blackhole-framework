namespace Blackhole.GlobalDialogue.Contract
{
    public struct ConfirmationPanelSetting 
    {
        public readonly string Description;
        public readonly string ButtonDescription;

        public ConfirmationPanelSetting(string description, string buttonDescription)
        {
            Description = description;
            ButtonDescription = buttonDescription;
        }
    }
    public struct YesOrNoPanelSetting
    {
        public readonly string Description;
        public readonly string NoButtonDescription;
        public readonly string YesButtonDescription;

        public YesOrNoPanelSetting(string description, string yesButtonDescription, string noButtonDescription)
        {
            Description = description;
            YesButtonDescription = yesButtonDescription;
            NoButtonDescription= noButtonDescription;
        }
    }
}