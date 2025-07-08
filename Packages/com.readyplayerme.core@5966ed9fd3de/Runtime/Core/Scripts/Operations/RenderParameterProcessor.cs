namespace ReadyPlayerMe.Core
{
    public static class RenderParameterProcessor
    {
        public static string GetRenderUrl(AvatarContext avatarContext)
        {
            return $"{avatarContext.AvatarUri.ImageUrl}?{avatarContext.RenderSettings.GetParametersAsString()}";
        }
    }
}
