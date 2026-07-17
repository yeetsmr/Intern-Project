namespace MVVM_WPF.Model.Enums
{ 
    public enum FilterCompositionLogicalOperator { And, Or }

    public enum FilterOperator
    {
        IsLessThan,
        IsLessThanOrEqualTo,
        IsEqualTo,
        IsNotEqualTo,
        IsGreaterThanOrEqualTo,
        IsGreaterThan,
        StartsWith,
        EndsWith,
        Contains,
        DoesNotContain,
        IsContainedIn,
        IsNotContainedIn,
        IsNull,
        IsNotNull,
        IsEmpty,
        IsNotEmpty
    }


    public enum TaskPriority
    {
        Low = 0,
        Mid = 1,
        High = 2
    }

    public enum TaskCategory
    {
        Work = 0,
        Personal = 1,
        Shopping = 2,
        Health = 3,
        Finance = 4,
        Education = 5,
        Travel = 6,
        Entertainment = 7,
        Other = 8
    }
    public enum CustomMessages
    {
        LoginEmptyCredentials,
        LoginInvalidCredentials,
        TaskNameEmpty,
        TaskAddedSuccess,
        TaskUpdateNotSelected,
        TaskUpdatedSuccess,
        TaskDeleteNotSelected,
        TaskDeleteConfirm,
        TaskDeletedSuccess,
        ApiGenericError,
        ConnectionError
    }
}