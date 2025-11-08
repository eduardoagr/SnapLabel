namespace SnapLabel.Interfaces;

public interface IDatabaseService<T> where T : BaseModel, new() {

    Task<bool> HasDataAsync();

    Task<long> TryAddAsync(T entity);

    Task<List<T>> GetAllAsync();

    Task<T?> GetByIdAsync(long id);

    Task<T?> GetByNameAsync(string name); // Optional: only if T has Name

    Task UpdateAsync(T entity);

    Task DeleteAsync(long id);


}
