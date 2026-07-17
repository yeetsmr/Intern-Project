using MVVM_WPF.Model.Enums;

namespace MVVM_WPF.Resources
{
    public static class MessageProvider
    {
        private static readonly Dictionary<CustomMessages, string> _messages = new Dictionary<CustomMessages, string>
        {
            { CustomMessages.LoginEmptyCredentials, "Username or password can not be empty." },
            { CustomMessages.LoginInvalidCredentials, "Invalid username or password." },

            { CustomMessages.TaskNameEmpty, "Task name can not be null!" },
            { CustomMessages.TaskAddedSuccess, "Task added successfully!" },

            { CustomMessages.TaskUpdateNotSelected, "Please select a valid task to update." },
            { CustomMessages.TaskUpdatedSuccess, "Task updated successfully!" },

            { CustomMessages.TaskDeleteNotSelected, "Please select a task to delete." },
            { CustomMessages.TaskDeleteConfirm, "Are you sure you want to delete this task?" },
            { CustomMessages.TaskDeletedSuccess, "Task deleted!" },

            { CustomMessages.ApiGenericError, "API Error: {0}" },
            { CustomMessages.ConnectionError, "An error occured: {0}" }
        };

        public static string GetString(CustomMessages messageKey, params object[] args)
        {
            if (_messages.TryGetValue(messageKey, out string text))
            {
                if (args != null && args.Length > 0)
                {
                    return string.Format(text, args);
                }
                return text;
            }
            return "Unknown Message Key!";
        }
    }
}