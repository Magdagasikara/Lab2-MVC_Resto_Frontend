namespace Lab2_MVC_Resto_Frontend.Models.ViewModels
{
    public class MealCategoryWithMealsVM
    {
        public string Name { get; set; }
        public ICollection<MealVM>? Meals { get; set; }
    }
}
