namespace SnapLabel.Interfaces;

public interface IDatabaseService<T> where T : IFirebaseEntity {

    Task<string> InsertAsync(T entity);

    Task<User?> GetCurrentUser(string email, string node = AppConstants.USERS_NODE);

    Task<T?> GetByIdAsync(string id, string node);

    Task<IEnumerable<T>> GetAllAsync(string node);

    Task UpdateAsync(T entity);

    Task DeleteAsync(string id);
}

