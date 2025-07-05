namespace BasedTechStore.Web.ViewModels.Cart
{
    public class CartVM
    {
        public Guid Id { get; set; }
        public bool IsEmpty => CartItems == null || CartItems.Count == 0;
        public decimal TotalPrice => CartItems?.Sum(i => i.TotalPrice) ?? 0;
        public int TotalItems => CartItems?.Sum(i => i.Quantity) ?? 0;

        public List<CartItemVM> CartItems { get; set; } = new List<CartItemVM>();
    }
}
