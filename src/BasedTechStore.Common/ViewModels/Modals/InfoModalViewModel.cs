namespace BasedTechStore.Common.ViewModels.Modals
{
    public class InfoModalViewModel
    {
        // Modal base properties
        public string Id { get; set; } = "Modal"; // default modal
        public string Title { get; set; } = "Попередження"; // default title
        public string Message { get; set; } = "Ви впевнені що хочете...?"; // default message
        public string SubMessage { get; set; } = "Цю дію не можна буде скасувати"; // default sub message

        // Buttons visibility
        public bool ShowCancelButton { get; set; } = true;
        public bool ShowConfirmButton { get; set; } = true; 
        public bool ShowSaveButton { get; set; } = false;

        // Buttons text
        public string CancelButtonText { get; set; } = "Скасувати"; // default cancel button text
        public string ConfirmButtonText { get; set; } = "Так"; // default confirm button text
        public string SaveButtonText { get; set; } = "Зберегти"; // default save button text

        // Modals type
        public bool IsDanger { get; set; } = false;
        public bool IsSuccess { get; set; } = false;
        public bool IsWarning { get; set; } = true; // default
        public bool IsInfo { get; set; } = false;

        public ModalSize Size { get; set; } = ModalSize.Default;

        // Modal behavior
        public bool StaticBackdrop { get; set; } = false;
        public string CustomCssClass { get; set; } = string.Empty; 
        public bool ShowCloseButton { get; set; } = true; 

        // Redirect params
        public string RedirectUrl { get; set; } = string.Empty;
        public bool RedirectAfterClose { get; set; } = false; 

        // Tracking params
        public bool TrackChanges { get; set; } = false;
        public string ChangeTrackingFormId { get; set; } = string.Empty;

        // Additional data
        public Dictionary<string, string> CustomData { get; set; } = new();

        public Dictionary<string, string> Translations { get; set; } = new();

        public string GetTypeClass()
        {
            if (IsDanger) return "modal-danger";
            if (IsSuccess) return "modal-success";
            if (IsWarning) return "modal-warning";
            return "modal-info";
        }
    }

    public enum ModalSize
    {
        Small,
        Default,
        Large,
        ExtraLarge,
        Fullscreen
    }
}
