namespace SnapLabel.Interfaces;

public interface IDatabaseService {

    Task<Product?> TryAddProductAsync(Product product);

    Task<List<Product>> GetAllProductsAsync();

    Task<Product?> GetProductByIdAsync(long id);

    Task<Product?> GetProductByNameAsync(string name);

    Task<bool> UpdateProductAsync(Product product);

    Task<bool> DeleteProductAsync(long id);
}