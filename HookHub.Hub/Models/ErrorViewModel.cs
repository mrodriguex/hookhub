namespace HookHub.Hub.Models
{
    /// <summary>
    /// Model for displaying error information in error views.
    /// Contains request ID for debugging purposes.
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// Gets or sets the request ID associated with the error.
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
        /// Gets a value indicating whether the request ID should be shown.
        /// Returns true if RequestId is not null or empty.
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}