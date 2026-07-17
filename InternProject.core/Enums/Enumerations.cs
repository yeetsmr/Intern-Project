
namespace InternProject.Core.Enums
{
    public enum Priority
    {
        Low,
        Mid,
        High
    }

    public enum Category
    {
        Work,
        Personal,
        Shopping,
        Health,
        Finance,
        Education,
        Travel,
        Entertainment,
        Other
    }
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

}
