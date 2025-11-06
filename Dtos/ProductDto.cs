namespace SnapLabel.Dtos;
public class ProductDto {

    public long Id { get; set; }
    public string? Name { get; set; }
    public string? Price { get; set; }
    public string? Location { get; set; }
    public byte[]? ImageBytes { get; set; }
    public string? GeneratedDate { get; set; }

}
