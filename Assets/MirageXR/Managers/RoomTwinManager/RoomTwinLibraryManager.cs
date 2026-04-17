using UnityEngine;

namespace MirageXR
{
    public class RoomTwinLibraryManager : LibraryManagerBase
    {
        public override string LibraryFileName => "roomTwinLib.json";

        public override string DefaultContentId => "";

        protected override string GetId(string entry)
        {
            return entry;
        }
    }
}
