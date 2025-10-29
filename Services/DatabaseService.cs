namespace SnapLabel.Services;
//public class DatabaseService {

//    // SemaphoreSlim is async-friendly and prevents race conditions
//    private readonly SemaphoreSlim _initSemaphore = new(1, 1);

//    // Flag to ensure initialization only happens once
//    private bool _isInitialized = false;

//    // SQLite connection instance
//    private SQLiteAsyncConnection? _database;

//    /// <summary>
//    /// Initializes the SQLite connection and ensures the Product table exists.
//    /// Uses SemaphoreSlim for thread-safe async initialization.
//    /// </summary>
//    private async Task InitializeAsync() {

//        if(_isInitialized) {
//            return;
//        }

//        await _initSemaphore.WaitAsync();
//        try {
//            if(_isInitialized) {
//                return;
//            }

//            Debug.WriteLine($"📂 SQLite DB Path: {DBConstants.DatabasePath}");

//            _database = new SQLiteAsyncConnection(
//                DBConstants.DatabasePath,
//                DBConstants.Flags);

//            await _database.CreateTableAsync<Product>();
//            _isInitialized = true;
//        } finally {
//            _initSemaphore.Release();
//        }
//    }

//    /// <summary>
//    /// Retrieves all products from the database.
//    /// </summary>
//    public async Task<List<Product>> GetItemsAsync() {

//        await InitializeAsync();

//        var items = await _database!.Table<Product>().ToListAsync();

//        return items;
//    }

//    /// <summary>
//    /// Attempts to add a product if it doesn't already exist.
//    /// Checks for uniqueness by ID, name, and image content.
//    /// </summary>
//    public async Task<long?> TryAddItemAsync(Product product) {

//        await InitializeAsync();

//        var existing = await _database!.Table<Product>()
//                  .Where(p => p.Name == product.Name)
//                  .FirstOrDefaultAsync();

//        if(existing is not null) {
//            // Duplicate found — do not insert
//            return null;
//        }


//        await _database.InsertAsync(product);

//        return product.Id;
//    }

//    /// <summary>
//    /// Deletes a product from the database.
//    /// </summary>
//    public async Task<int> DeleteItemAsync(Product product) {
//        await InitializeAsync();
//        return await _database!.DeleteAsync(product);
//    }

//    /// <summary>
//    /// Retrieves a product by its primary key ID.
//    /// </summary>
//    public async Task<Product?> GetItemByIdAsync(int id) {
//        await InitializeAsync();
//        return await _database!.FindAsync<Product>(id);
//    }

//    /// <summary>
//    /// Update product from the database, by passing it
//    /// </summary>
//    public async Task<int> UpdateItemAsync(Product product) {
//        await InitializeAsync();
//        return await _database!.UpdateAsync(product);
//    }

//}
