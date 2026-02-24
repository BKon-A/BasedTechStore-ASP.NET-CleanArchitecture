namespace BasedTechStore.Application.Common.Interfaces.Authorization
{
    public interface IAuthorizationPolicy<TResourse>
    {
        Task<bool> IsAuthorizedAsync(string userId, TResourse resourse, string action);
    }
}
