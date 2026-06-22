using InternProject.Core;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InternProject.DataAccess
{
    public class TaskRepository
    {
        private readonly IMongoCollection<task> _taskCollection;

        public TaskRepository(IMongoDatabase database)
        {

            _taskCollection = database.GetCollection<task>("dto");
        }

        public async Task<List<task>> GetFilteredTasksAsync(FilterDto filterDto)
        {

            var combinedFilter = FilterBuilder<task>.Build(filterDto);

            int skipAmount = (filterDto.PageNumber - 1) * filterDto.PageSize;

            return await _taskCollection.Find(combinedFilter)
                                        .Skip(skipAmount)
                                        .Limit(filterDto.PageSize)
                                        .ToListAsync();
        }
    }
}