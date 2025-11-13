namespace SnapLabel.Services;
public class DatabaseService<T>(Client _client, IShellService shell) : IDatabaseService<T> where T : BaseModel,
    IHasId, new() {

    public async Task<bool> DeleteAsync(Guid id) {

        try {
            await _client
                .From<T>()
                .Where(x => x.id == id)
                .Delete();

            return true;

        } catch(Exception ex) {

            await DisplayErrorAsync(ex);

            return false;
        }
    }



    public async Task<IEnumerable<T>> GetAllAsync() {

        try {

            var result = await _client.From<T>().Get();
            var objs = result.Models;

            return objs;


        } catch(Exception ex) {

            await DisplayErrorAsync(ex);

            return [];
        }
    }

    public async Task<T?> GetByIdAsync(Guid id) {

        try {

            var data = await _client
                .From<T>()
                .Where(x => x.id == id)
                .Get();

            return data.Models.FirstOrDefault();


        } catch(Exception ex) {

            await DisplayErrorAsync(ex);

            return null;
        }

    }

    public async Task<bool> HasDataAsync() {

        try {

            var response = await _client.From<T>().Limit(1).Get();

            var data = response.Models.Count != 0;

            return data;

        } catch(Exception ex) {

            await DisplayErrorAsync(ex);
            return false;
        }
    }

    public async Task<bool> InsertAsync(T entity) {

        try {
            var response = await _client
                .From<T>()
                .Insert(entity, new QueryOptions {
                    Returning = QueryOptions.ReturnType.Representation
                });

            return response.Models.Count != 0;
        } catch(Exception ex) {
            await DisplayErrorAsync(ex);
            return false;
        }

    }

    public async Task<bool> UpdateAsync(T entity) {

        try {
            var response = await _client
                .From<T>()
                .Update(entity);

            return response.Models.Count != 0;

        } catch(Exception ex) {

            await DisplayErrorAsync(ex);

            return false;
        }
    }

    private async Task DisplayErrorAsync(Exception ex) {
        await shell.DisplayAlertAsync("Error", ex.Message, "OK");
    }
}
