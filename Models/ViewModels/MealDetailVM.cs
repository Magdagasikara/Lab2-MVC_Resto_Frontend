using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Lab2_MVC_Resto_Frontend.Models.ViewModels
{
    public class MealDetailVM
    {
        public int Id { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Enter name of the dish")]
        public string Name { get; set; }
        [Required]
        [DataType(DataType.MultilineText)]
        [StringLength(200, ErrorMessage = "Enter name of the dish)")]
        public string Description { get; set; }
        [Required]
        [Range(25, 2000, ErrorMessage = "Enter a reasonable price in SEK (between 25 and 2000 kr)")]
        public decimal Price { get; set; }
        [Required]
        [DisplayName("Is available?")]
        public bool IsAvailable { get; set; }
        [DisplayName("Meal category")]
        public int? FK_MealCategoryId { get; set; }
        public string? MealCategoryName { get; set; }
    }
}
