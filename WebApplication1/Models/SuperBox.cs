using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models;
using System.ComponentModel.DataAnnotations;
public class SuperBox
{
    public string Id { get; set; } 
    
    [ForeignKey("Order")]
    public int? OrderId { get; set; }
    [Required(ErrorMessage = "Capacity is required.")]
    public int Capacity { get; set; }

    [Required(ErrorMessage = "Street Name is required.")]
    public string StreetName { get; set; }

    [Required(ErrorMessage = "Street Number is required.")]
    public int StreetNumber { get; set; }

    [Required(ErrorMessage = "City is required.")]
    public string City { get; set; }

    [Required(ErrorMessage = "Zip Code is required.")]
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