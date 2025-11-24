namespace SnapLabel.Interfaces;

public interface IDatabaseService<T> where T : IFirebaseEntity {

    Task<string> InsertAsync(T entity);

    Task<T?> GetByIdAsync(string id);

    Task<IEnumerable<T>> GetAllAsync();

    Task UpdateAsync(T entity);

    Task DeleteAsync(string id);
}

