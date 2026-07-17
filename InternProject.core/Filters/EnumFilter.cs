
namespace InternProject.Core.Filters
{
    public class EnumFilter<T> where T : struct, Enum
    {
        public List<T> SelectedValues { get; set; } = new List<T>();
    }
}
