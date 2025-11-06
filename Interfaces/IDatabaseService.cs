namespace SnapLabel.Interfaces;

public interface IDatabaseService {

    // Ping the database to check connectivity
    Task<bool> IsSupabaseReachableAsync();

    Task<bool> HasProductDataAsync();

    Task<long> TryAddProductAsync(Product product);

    Task<List<Product>> GetAllProductsAsync();

    Task<Product?> GetProductByIdAsync(long id);

    Task<Product?> GetProductByNameAsync(string name);

    Task UpdateProductAsync(Product product);

    Task DeleteProductAsync(long id);
}
