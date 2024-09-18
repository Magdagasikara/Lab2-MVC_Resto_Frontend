namespace Lab2_MVC_Resto_Frontend.Models.ViewModels
{
    public class MealDetailVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        public string? MealCategoryName { get; set; }
    }
}
