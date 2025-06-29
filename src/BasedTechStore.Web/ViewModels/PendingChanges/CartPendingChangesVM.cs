using BasedTechStore.Application.DTOs.Cart;

namespace BasedTechStore.Web.ViewModels.PendingChanges
{
    public class CartPendingChangesVM
    {
        public CartPendingChangesVM()
        {
            CreatedItems = new List<CartItemDto>();
            UpdatedItems = new List<CartItemDto>();
            DeletedItems = new List<CartItemDto>();
        }

        public Guid CartId { get; set; }
        public List<CartItemDto> CreatedItems { get; set; }
        public List<CartItemDto> UpdatedItems { get; set; }
        public List<CartItemDto> DeletedItems { get; set; }
    }
}
