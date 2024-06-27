using System;
using System.Collections;
using System.Collections.Generic;
using ReadyPlayerMe.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ReadyPlayerMe.Samples.AvatarLoading
{
    public class MultipleQualityAvatarLoadingExample : MonoBehaviour
    {

        [SerializeField]
        private string avatarUrl = "https://models.readyplayer.me/638df75e5a7d322604bb3dcd.glb";
        [SerializeField]
        private Transform qualityContainerPrefab;
        [SerializeField]
        private AvatarConfigData[] avatarConfigs;

        private List<GameObject> avatarList;

        private void Start()
        {
            ApplicationData.Log();
            avatarList = new List<GameObject>();
            StartCoroutine(LoadAvatars());
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
            if (avatarList != null)
            {
                foreach (GameObject avatar in avatarList)
                {
                    Destroy(avatar);
                }
                avatarList.Clear();
                avatarList = null;
            }
        }

        private IEnumerator LoadAvatars()
        {
            var loading = false;

            foreach (AvatarConfigData config in avatarConfigs)
            {
                loading = true;
                var loader = new AvatarObjectLoader();
                loader.OnCompleted += (sender, args) =>
                {
                    loading = false;
                    AvatarAnimationHelper.SetupAnimator(args.Metadata, args.Avatar);
                    OnAvatarLoaded(args.Avatar, config);
                };
                loader.AvatarConfig = config.Config;
                loader.LoadAvatar(avatarUrl);

                yield return new WaitUntil(() => !loading);
            }
        }

        private void OnAvatarLoaded(GameObject avatar, AvatarConfigData data)
        {
            if (avatarList != null)
            {
                var quality = data.Config.name.Substring("Avatar Config".Length);
                Transform container = Instantiate(qualityContainerPrefab);
                container.name = quality;
                container.position = new Vector3(data.PosX, 0, 0);
                container.GetComponentInChildren<Text>().text = "<b>" + quality + "</b>\n" +
                                                                "MeshLoad: " + data.Config.Lod + "\n" +
                                                                "Texture: " + data.Config.TextureAtlas;
                avatar.name = "Avatar";
                avatar.transform.SetParent(container, false);
                avatarList.Add(container.gameObject);
            }
            else
            {
                Destroy(avatar);
            }
        }

        [Serializable]
        private struct AvatarConfigData
        {
            public AvatarConfig Config;
            public float PosX;

            // TODO Find a fix for ignoring warning
            // Had to add this constructor because of "Field is never assigned" warning
            public AvatarConfigData(AvatarConfig config, float posX)
            {
                Config = config;
                PosX = posX;
            }
        }
    }
}
