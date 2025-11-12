namespace SnapLabel.Interfaces;

public interface IDatabaseService<T> where T : BaseModel, IHasId, new() {

    Task<IEnumerable<T>> GetAllAsync();

    Task<T?> GetByIdAsync(Guid id);

    Task<bool> InsertAsync(T entity);

    Task<bool> UpdateAsync(T entity);

    Task<bool> DeleteAsync(Guid id);

    Task<bool> HasDataAsync();
}
