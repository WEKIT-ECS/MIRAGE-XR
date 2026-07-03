namespace MirageXR
{
    public class AvatarLibraryManager : LibraryManagerBase
    {
        public override string LibraryFileName => "avatarLib.json";

        public override string DefaultContentId => AvatarLoader.DefaultAvatarUrl;

        protected override string GetId(string entry)
        {
            return AvatarLoadUtils.GetId(entry);
        }
    }
}
