using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models;
using System.ComponentModel.DataAnnotations;
public class SuperBox
{
    public string Id { get; set; }
    [ForeignKey("Order")]
    public int? OrderId { get; set; }
    [Required(ErrorMessage = "Capacity is required.")]
    [Range(1, 1000, ErrorMessage = "Capacity must be between 1 and 1000.")]
    public int Capacity { get; set; }

    [Required(ErrorMessage = "Street Name is required.")]
    public string StreetName { get; set; }

    [Required(ErrorMessage = "Street Number is required.")]
    [Range(1, 9999, ErrorMessage = "Street number must be a valid number.")]
    public int StreetNumber { get; set; }

    [Required(ErrorMessage = "City is required.")]
    public string City { get; set; }

    [Required(ErrorMessage = "Zip code is required.")]
    [RegularExpression(@"^\d{5}$", ErrorMessage = "Zip code must be 5 digits.")]
    public int ZipCode { get; set; }
    public string DisplayAddress => $"{StreetName} {StreetNumber}, {City}, {ZipCode}";

    public SuperBox()
    {
        Id = Guid.NewGuid().ToString();
    }
    public SuperBox(Order order, int capacity, string streetName, int streetNumber, string city, int zipCode)
    {
        Id = Guid.NewGuid().ToString();
        Capacity = capacity;
        StreetName = streetName;
        StreetNumber = streetNumber;
        City = city;
        ZipCode = zipCode;
    }
}