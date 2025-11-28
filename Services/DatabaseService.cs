using Firebase.Database.Query;

namespace SnapLabel.Services;

public class DatabaseService<T>(FirebaseClient _client) : IDatabaseService<T> where T : IFirebaseEntity {

    private readonly string _collectionName = $"{typeof(T).Name}s";

    public async Task<string> InsertAsync(T entity) {

        var result = await _client
        .Child(_collectionName)
        .PostAsync(entity);

        entity.Id = result.Key;

        await UpdateAsync(entity);

        return result.Key;
    }

    public async Task<T?> GetByIdAsync(string id, string node) {

        if(string.IsNullOrEmpty(id))
            throw new InvalidOperationException("Please provide id");

        var items = await _client
            .Child(node)
            .OrderByKey()
            .OnceAsync<T>();

        return items.Select(item => item.Object).FirstOrDefault();
    }

    public async Task<IEnumerable<T>> GetAllAsync(string node) {

        var items = await _client
            .Child(node)
            .OrderByKey()
            .OnceAsync<T>();

        return items.Select(item => item.Object);
    }

    public async Task UpdateAsync(T entity) {

        if(string.IsNullOrEmpty(entity.Id))
            throw new InvalidOperationException("Entity must have an Id to update.");

        await _client
            .Child(_collectionName)
            .Child(entity.Id)
            .PutAsync(entity);

    }

    public Task DeleteAsync(string id) {
        throw new NotImplementedException();
    }

}
