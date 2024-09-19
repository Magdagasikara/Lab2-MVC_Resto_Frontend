using System.ComponentModel.DataAnnotations;

namespace Lab2_MVC_Resto_Frontend.Models.ViewModels
{
    public class MealVM
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }

    }
}
