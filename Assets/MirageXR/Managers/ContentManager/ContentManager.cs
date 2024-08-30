using System;
using Cysharp.Threading.Tasks;

namespace MirageXR.NewDataModel
{
    public class ContentManager : IContentManager
    {
        public UniTask LoadContentAsync(Activity activity)
        {
            throw new NotImplementedException();
            foreach (var content in activity.Content)
            {
                //content.
            }
        }
    }
}