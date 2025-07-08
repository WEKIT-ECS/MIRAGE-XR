using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ReadyPlayerMe.Core;
using UnityEngine;

namespace ReadyPlayerMe.AvatarCreator
{
    /// <summary>
    /// This class is responsible for fetching avatar template data,
    /// including icons, and creating buttons for each template.
    /// It serves as a self-contained UI element for avatar template selection.
    /// It extends the functionality of SelectionElement to handle AvatarTemplate-specific interactions.
    /// </summary>
    public class TemplateSelectionElement : SelectionElement
    {
        private const string TAG = nameof(TemplateSelectionElement);
        [SerializeField] private TemplateVersions templateVersions = TemplateVersions.V2;
        private List<AvatarTemplateData> avatarTemplates;
        private AvatarTemplateFetcher avatarTemplateFetcher;
        private CancellationTokenSource cancellationTokenSource;
        
        private const string TEMPLATE_V2_USAGE_TYPE = "onboarding";
        private const string TEMPLATE_V1_USAGE_TYPE = "randomize";

        private void Awake()
        {
            cancellationTokenSource = new CancellationTokenSource();
            avatarTemplateFetcher = new AvatarTemplateFetcher(cancellationTokenSource.Token);
        }

        private void OnDestroy()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }


        /// <summary>
        /// Asynchronously loads avatar template data and creates button elements for each template.
        /// Each button is created with an icon fetched from the template's image URL.
        /// </summary>
        public async void LoadAndCreateButtons()
        {
            await LoadAndCreateButtons(OutfitGender.None);
        }
        
        /// <summary>
        /// Asynchronously loads avatar template data and creates button elements for each template based on the gender.
        /// Each button is created with an icon fetched from the template's image URL.
        /// <param name="gender">Gender for which the templates are loaded for</param>
        /// </summary>
        public async Task LoadAndCreateButtons(OutfitGender gender)
        {
            await TaskExtensions.HandleCancellation(LoadTemplateData(), () => CreateButtons(gender));
        }

        private void CreateButtons(OutfitGender gender)
        {
            var filteredTemplates = avatarTemplates!.Where(template => HasCorrectTemplateVersion(template) && HasCorrectGender(template, gender)).ToList();
            
            CreateButtons(filteredTemplates!.ToArray(), async (button, asset) =>
            {
                var webRequestDispatcher = new WebRequestDispatcher();
                var url = $"{asset.ImageUrl}";
                var texture = await TaskExtensions.HandleCancellation(webRequestDispatcher.DownloadTexture(url, cancellationTokenSource.Token));
                if (texture == null)
                {
                    return;
                }
                button.SetIcon(texture);
            });
        }
        
        private bool HasCorrectTemplateVersion(AvatarTemplateData template)
        {
            switch (templateVersions)
            {
                case TemplateVersions.V2:
                    return template.UsageType.Contains(TEMPLATE_V2_USAGE_TYPE);
                case TemplateVersions.V1:
                    return template.UsageType.Contains(TEMPLATE_V1_USAGE_TYPE);
                case TemplateVersions.All:
                default:
                    return true;
            }
        }
        
        private bool HasCorrectGender(AvatarTemplateData template, OutfitGender gender)
        {
            if (gender == OutfitGender.None || template.Gender == OutfitGender.None)
            {
                return true;
            }
            return gender == template.Gender;
        }

        /// <summary>
        /// Loads avatar template data asynchronously.
        /// This method fetches data but does not handle icon rendering.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation of loading avatar template data.</returns>
        public async Task LoadTemplateData()
        {
            avatarTemplates = await avatarTemplateFetcher.GetTemplates();
            if (avatarTemplates == null || avatarTemplates.Count == 0)
            {
                SDKLogger.LogWarning(TAG, "No templates found");
            }
        }
    }
}
