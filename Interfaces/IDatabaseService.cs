namespace SnapLabel.Interfaces;

public interface IDatabaseService<T> where T : IFirebaseEntity {

    Task<string> InsertAsync(T entity);

    Task<T?> GetByIdAsync(string id);

    Task<IEnumerable<T>> GetAllAsync<T>(string node);

    Task UpdateAsync(T entity);

    Task DeleteAsync(string id);
}

