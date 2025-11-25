using Firebase.Database;
using Firebase.Database.Query;

namespace SnapLabel.Services;

public class DatabaseService<T>(FirebaseClient _client) : IDatabaseService<T> where T : IFirebaseEntity {

    private readonly string _collectionName = $"{typeof(T).Name}s";

    public async Task<string> InsertAsync(T entity) {

        var result = await _client
        .Child(_collectionName)
        .PostAsync(entity);

        entity.Id = result.Key;
        return entity.Id;
    }

    public Task<T?> GetByIdAsync(string id) {
        throw new NotImplementedException();
    }


    public async Task<IEnumerable<T>> GetAllAsync<T>(string node) {

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
            .Child(entity.Id)   // use the generated key
            .PutAsync(entity);  // overwrite with updated object

    }

    public Task DeleteAsync(string id) {
        throw new NotImplementedException();
    }
}
