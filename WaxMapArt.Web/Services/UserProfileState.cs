namespace WaxMapArt.Web.Services;

public class UserProfileState
{
    public string? AvatarUrl { get; private set; }

    public event Action? OnAvatarChanged;

    public void SetAvatar(string newUrl)
    {
        AvatarUrl = newUrl;
        OnAvatarChanged?.Invoke();
    }
}
