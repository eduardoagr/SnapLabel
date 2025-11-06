namespace SnapLabel.Services;

public class DatabaseService(Supabase.Client client, IShellService shellService) : IDatabaseService {

    public async Task DeleteProductAsync(long id) {

        await client.From<Product>().Where(p => p.Id == id).Delete();
    }

    public async Task<List<Product>> GetAllProductsAsync() {

        var response = await client.From<Product>().Get();

        return response.Models;
    }

    public async Task<Product?> GetProductByIdAsync(long id) {

        var response = await client.From<Product>().Where(p => p.Id == id).Get();

        return response.Models.FirstOrDefault();
    }

    public async Task<Product?> GetProductByNameAsync(string name) {

        var response = await client.From<Product>().Where(p => p.Name == name).Get();

        return response.Models.FirstOrDefault();
    }

    public async Task<bool> HasProductDataAsync() {
        var response = await client.From<Product>().Get();
        return response.Models.Count != 0;
    }

    public async Task<bool> IsSupabaseReachableAsync() {
        try {
            var response = await client.From<Product>().Get();
            return true;
        } catch {
            return false;
        }
    }

    public async Task<long> TryAddProductAsync(Product product) {

        //product.NormalizeValues();

        //var existingProduct = await GetProductByNameAsync(product.Name!);
        //if(existingProduct != null) {
        //    await shellService.DisplayAlertAsync("Duplicate Product", "A product with this name already exists.", "OK");
        //    return 0;
        //}

        // Insert test product
        var cc = new Product {
            Name = "Test Product",
            Price = "9.99",
            Location = "Warehouse A",
            ImageBytes = [1, 2, 3, 4]
        };

        await client.From<Product>().Insert(cc);

        // Fetch back
        var result = await client.From<Product>().Select("*").Get();
        var fetched = result.Models.First();

        Console.WriteLine($"ImageBytes: {BitConverter.ToString(fetched.ImageBytes)}");

        await client.From<Product>().Insert(product);

        // 2️⃣ Immediately get the latest inserted product
        var inserted = (await client
            .From<Product>()
            .Order("id", Supabase.Postgrest.Constants.Ordering.Descending)
            .Limit(1)
            .Get())
            .Models
            .FirstOrDefault();

        if(inserted == null)
            return 0;

        // Now update its ImagePath using the ID
        product.Id = inserted.Id;


        await client
            .From<Product>()
            .Where(p => p.Id == inserted.Id)
            .Update(inserted);

        return inserted.Id;
    }



    public async Task UpdateProductAsync(Product product) {

        product.NormalizeValues();

        var conflict = await GetProductByNameAsync(product.Name!);

        if(conflict != null && conflict.Id != product.Id) {

            await shellService.DisplayAlertAsync("Duplicate Product", "A product with this name already exists.", "OK");

            return;
        }

        var response = await client
            .From<Product>()
            .Where(p => p.Id == product.Id)
            .Update(product);
    }
}