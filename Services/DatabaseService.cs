namespace SnapLabel.Services;
public class DatabaseService<T> : IDatabaseService<T> where T : BaseModel, new() {
    private readonly IShellService _shellService;

    public DatabaseService(IShellService shellService) {
        _shellService = shellService;
    }

    public async Task<bool> HasDataAsync() {
        // Your logic here
        return false;
    }

    public async Task<long> TryAddAsync(T entity) {
        // Your logic here
        return 0;
    }

    public async Task<List<T>> GetAllAsync() {
        // Your logic here
        return new List<T>();
    }

    public async Task<T?> GetByIdAsync(long id) {
        // Your logic here
        return null;
    }

    public async Task<T?> GetByNameAsync(string name) {
        // Optional logic if T has a Name property
        return null;
    }

    public async Task UpdateAsync(T entity) {
        // Your logic here
    }

    public async Task DeleteAsync(long id) {
        // Your logic here
    }
}
