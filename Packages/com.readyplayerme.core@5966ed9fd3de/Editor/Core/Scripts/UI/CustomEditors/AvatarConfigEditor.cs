using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ReadyPlayerMe.Core.Editor
{
    [CustomEditor(typeof(AvatarConfig))]
    public class AvatarConfigEditor : UnityEditor.Editor
    {
        private const string DIALOG_TITLE = "Read Player Me";
        private const string DIALOG_MESSAGE = "Do you want to install {0} Unity Package: {1} ?";
        private const string DIALOG_OK = "Ok";
        private const string DIALOG_CANCEL = "Cancel";
        private const string ADD_MORPH_TARGET = "Add Morph Target";
        private const string DELETE_MORPH_TARGET = "Delete Morph Target";
        private const string REMOVE_BUTTON_TEXT = "Remove";
        private const string MESH_OPT_PACKAGE_NAME = "com.unity.meshopt.decompress";

        [SerializeField] private VisualTreeAsset visualTreeAsset;

        private AvatarConfig avatarConfigTarget;
        private List<Label> morphTargetLabels;

        private Dictionary<VisualElement, string> morphTargetsParentVisualElement;

        private VisualElement selectedMorphTargets;

        private SerializedProperty useDracoCompressionField;
        private bool previousDracoCompressionValue;

        private SerializedProperty useMeshOptCompressionField;
        private bool previousMeshOptCompressionValue;

        private VisualElement root;
        private Action textureChannelChanged;

        private SerializedProperty shaderPropertyMappingList;

        public override VisualElement CreateInspectorGUI()
        {
            root = new VisualElement();
            visualTreeAsset.CloneTree(root);
            avatarConfigTarget = (AvatarConfig) target;
            shaderPropertyMappingList = serializedObject.FindProperty("ShaderPropertyMapping");
            SetupLod();
            SetupPose();
            SetupTextureAtlas();
            SetupTextureSizeLimit();
            SetupUseHands();
            SetupCompressionPackages();
            SetupTextureChannel();
            SetupMorphTargets();
            SetupShader();
            return root;
        }

        private void SetupLod()
        {
            var lod = root.Q<EnumField>("Lod");
            lod.SetValueWithoutNotify(avatarConfigTarget.Lod);
            lod.RegisterValueChangedCallback(x =>
                {
                    avatarConfigTarget.Lod = (Lod) x.newValue;
                    Save();
                }
            );
        }

        private void SetupPose()
        {
            var pose = root.Q<EnumField>("Pose");
            pose.SetValueWithoutNotify(avatarConfigTarget.Pose);
            pose.RegisterValueChangedCallback(x =>
                {
                    avatarConfigTarget.Pose = (Pose) x.newValue;
                    Save();
                }
            );
        }

        private void Save()
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(serializedObject.targetObject);
        }

        private void SetupTextureAtlas()
        {
            var textureAtlas = root.Q<EnumField>("TextureAtlas");
            textureAtlas.SetValueWithoutNotify(avatarConfigTarget.TextureAtlas);
            textureAtlas.RegisterValueChangedCallback(x =>
                {
                    avatarConfigTarget.TextureAtlas = (TextureAtlas) x.newValue;
                    Save();
                }
            );
        }

        private void SetupTextureSizeLimit()
        {
            var textureSizeLimit = root.Q<SliderInt>("TextureSizeLimit");
            textureSizeLimit.SetValueWithoutNotify(avatarConfigTarget.TextureSizeLimit);
            textureSizeLimit.RegisterValueChangedCallback(x =>
                {
                    avatarConfigTarget.TextureSizeLimit = x.newValue;
                    Save();
                }
            );
        }

        private void SetupUseHands()
        {
            var useHands = root.Q<Toggle>("UseHands");
            useHands.SetValueWithoutNotify(avatarConfigTarget.UseHands);
            useHands.RegisterValueChangedCallback(x =>
                {
                    avatarConfigTarget.UseHands = x.newValue;
                    Save();
                }
            );
        }

        private void SetupCompressionPackages()
        {
            var useDracoCompression = root.Q<Toggle>("UseDracoCompression");
            var useMeshOptCompression = root.Q<Toggle>("UseMeshOptCompression");
            var optimizationPackages = root.Q<Foldout>("OptimizationPackages");
            optimizationPackages.RegisterValueChangedCallback(x =>
            {
                useDracoCompression.SetValueWithoutNotify(avatarConfigTarget.UseDracoCompression);
                useMeshOptCompression.SetValueWithoutNotify(avatarConfigTarget.UseMeshOptCompression);
            });

            useDracoCompression.RegisterValueChangedCallback(x =>
                {
                    if (avatarConfigTarget.UseDracoCompression == x.newValue) return;
                    avatarConfigTarget.UseDracoCompression = x.newValue;
                    if (!ModuleInstaller.IsModuleInstalled(ModuleList.DracoCompression.name))
                    {
                        if (EditorUtility.DisplayDialog(
                                DIALOG_TITLE,
                                string.Format(DIALOG_MESSAGE, "Draco compression", ModuleList.DracoCompression.name),
                                DIALOG_OK,
                                DIALOG_CANCEL))
                        {
                            ModuleInstaller.AddModuleRequest(ModuleList.DracoCompression.Identifier);
                        }
                        else
                        {
                            avatarConfigTarget.UseDracoCompression = false;
                        }
                    }
                    useDracoCompression.SetValueWithoutNotify(avatarConfigTarget.UseDracoCompression);
                    if (avatarConfigTarget.UseDracoCompression && avatarConfigTarget.UseMeshOptCompression)
                    {
                        Debug.LogWarning("Draco compression is not compatible with Mesh Optimization compression. Mesh Optimization compression will be disabled.");
                        avatarConfigTarget.UseMeshOptCompression = false;
                        useMeshOptCompression.SetValueWithoutNotify(false);
                    }
                    Save();
                }
            );

            useMeshOptCompression.RegisterValueChangedCallback(x =>
                {
                    if (avatarConfigTarget.UseMeshOptCompression == x.newValue) return;
                    avatarConfigTarget.UseMeshOptCompression = x.newValue;
                    if (!PackageManagerHelper.IsPackageInstalled(MESH_OPT_PACKAGE_NAME))
                    {
                        if (EditorUtility.DisplayDialog(
                                DIALOG_TITLE,
                                string.Format(DIALOG_MESSAGE, "Mesh opt compression", MESH_OPT_PACKAGE_NAME),
                                DIALOG_OK,
                                DIALOG_CANCEL))
                        {
                            PackageManagerHelper.AddPackage(MESH_OPT_PACKAGE_NAME);
                        }
                        else
                        {
                            avatarConfigTarget.UseMeshOptCompression = false;
                        }
                    }
                    useMeshOptCompression.SetValueWithoutNotify(avatarConfigTarget.UseMeshOptCompression);
                    if (avatarConfigTarget.UseMeshOptCompression && avatarConfigTarget.UseDracoCompression)
                    {
                        Debug.LogWarning("Mesh Optimization compression is not compatible with Draco compression. Draco compression will be disabled.");
                        avatarConfigTarget.UseDracoCompression = false;
                        useDracoCompression.SetValueWithoutNotify(false);
                    }
                    Save();
                }
            );
        }

        private void SetupTextureChannel()
        {
            var items = new List<string>();
            foreach (TextureChannel textureChannel in Enum.GetValues(typeof(TextureChannel)))
            {
                items.Add(textureChannel.ToString());
            }

            VisualElement MakeItem()
            {
                var toggle = new Toggle();
                toggle.style.alignItems = Align.Center;
                return toggle;
            }

            void BindItem(VisualElement e, int i)
            {
                var toggle = (Toggle) e;
                toggle.label = items[i];
                if (avatarConfigTarget.TextureChannel.Contains((TextureChannel) i))
                {
                    toggle.SetValueWithoutNotify(true);
                }

                toggle.RegisterValueChangedCallback(x =>
                {
                    if (x.newValue)
                    {
                        var textureChannels = avatarConfigTarget.TextureChannel.ToList();
                        textureChannels.Add((TextureChannel) i);
                        avatarConfigTarget.TextureChannel = textureChannels.ToArray();
                        textureChannelChanged?.Invoke();
                    }
                    else
                    {
                        var textureChannels = avatarConfigTarget.TextureChannel.ToList();
                        textureChannels.Remove((TextureChannel) i);
                        avatarConfigTarget.TextureChannel = textureChannels.ToArray();
                        textureChannelChanged?.Invoke();
                    }
                    Save();
                });
            }

            var listView = root.Q<ListView>();
            listView.style.height = 30 * Enum.GetValues(typeof(TextureChannel)).Length + 2;
            listView.makeItem = MakeItem;
            listView.bindItem = BindItem;
            listView.itemsSource = items;
            listView.selectionType = SelectionType.Multiple;

            listView.onItemsChosen += Debug.Log;
            listView.onSelectionChange += Debug.Log;
        }

        private void SetupShader()
        {
            var shader = root.Q<ObjectField>("ShaderOverride");
            shader.SetValueWithoutNotify(avatarConfigTarget.Shader);

            shader.RegisterValueChangedCallback(x =>
                {
                    avatarConfigTarget.Shader = (Shader) x.newValue;
                    Save();
                    SetupShader();
                }
            );
            var shaderPropertiesContainer = root.Q<VisualElement>("ShaderProperties");
            shaderPropertiesContainer.Clear();
            shaderPropertiesContainer.style.display = DisplayStyle.Flex;
            shaderPropertiesContainer.style.flexDirection = FlexDirection.Column;
            if (avatarConfigTarget.Shader == null)
            {
                shaderPropertiesContainer.style.display = DisplayStyle.None;
            }
            else
            {
                shaderPropertiesContainer.style.display = DisplayStyle.Flex;
                shaderPropertiesContainer.style.marginTop = 10;
                shaderPropertiesContainer.style.left = 10;
                shaderPropertiesContainer.style.right = 10;
                var titleRowContainer = new VisualElement();
                titleRowContainer.style.flexDirection = FlexDirection.Row;
                titleRowContainer.style.marginBottom = 7;
                titleRowContainer.style.marginTop = 7;
                titleRowContainer.style.left = 10;
                titleRowContainer.style.right = 10;
                var sourceTitleField = new Label("Source Property")
                {
                    style =
                    {
                        width = 200, unityFontStyleAndWeight = FontStyle.Bold, // Make the text bold
                        unityTextAlign = TextAnchor.MiddleLeft
                    }
                };
                titleRowContainer.Add(sourceTitleField);
                var targetTitleField = new Label("Target Property")
                {
                    style = { width = 200, marginRight = 10, flexGrow = 0.8f, unityFontStyleAndWeight = FontStyle.Bold, }
                };
                titleRowContainer.Add(targetTitleField);
                var typeTitleField = new Label("Type")
                {
                    style = { width = 70, alignSelf = Align.FlexEnd, unityFontStyleAndWeight = FontStyle.Bold, }
                };
                titleRowContainer.Add(typeTitleField);
                shaderPropertiesContainer.Add(titleRowContainer);
                for (int i = 0; i < shaderPropertyMappingList.arraySize; i++)
                {
                    SerializedProperty mapping = shaderPropertyMappingList.GetArrayElementAtIndex(i);

                    var propertyContainer = new VisualElement();
                    propertyContainer.style.flexDirection = FlexDirection.Column;
                    //propertyContainer.style.marginBottom = 10;

                    var horizontalContainer = new VisualElement();
                    horizontalContainer.style.flexDirection = FlexDirection.Row;
                    horizontalContainer.style.marginBottom = 7;
                    horizontalContainer.style.marginTop = 7;
                    horizontalContainer.style.left = 10;
                    horizontalContainer.style.right = 10;
                    // Alternating background colors
                    propertyContainer.style.backgroundColor = i % 2 == 0 ? new StyleColor(new Color(0.25f, 0.25f, 0.25f)) : new StyleColor(new Color(0.3f, 0.3f, 0.3f));

                    var sourcePropertyField = new Label(mapping.FindPropertyRelative("SourceProperty").stringValue)
                    {
                        style =
                        {
                            width = 200, unityFontStyleAndWeight = FontStyle.Bold, // Make the text bold
                            unityTextAlign = TextAnchor.MiddleLeft
                        }
                    };
                    horizontalContainer.Add(sourcePropertyField);

                    var targetPropertyField = new TextField
                    {
                        value = mapping.FindPropertyRelative("TargetProperty").stringValue,
                        //style = { flexGrow = 1, marginTop = 5 }
                        style = { width = 200, marginRight = 10, flexGrow = 0.8f }
                    };
                    targetPropertyField.RegisterValueChangedCallback(evt =>
                    {
                        mapping.FindPropertyRelative("TargetProperty").stringValue = evt.newValue;
                        Save();
                    });
                    horizontalContainer.Add(targetPropertyField);

                    var propertyTypeField = new EnumField((ShaderPropertyType) mapping.FindPropertyRelative("Type").enumValueIndex)
                    {
                        style = { width = 70, alignSelf = Align.FlexEnd }
                    };
                    propertyTypeField.RegisterValueChangedCallback(evt =>
                    {
                        mapping.FindPropertyRelative("Type").enumValueIndex = (int) (ShaderPropertyType) evt.newValue;
                        Save();
                    });
                    horizontalContainer.Add(propertyTypeField);
                    propertyContainer.Add(horizontalContainer);
                    shaderPropertiesContainer.Add(propertyContainer);
                }
            }
        }

        private void SetupMorphTargets()
        {
            morphTargetLabels = AvatarMorphTarget.MorphTargetAvatarAPI.Select(x => new Label(x)).ToList();
            morphTargetsParentVisualElement = new Dictionary<VisualElement, string>();
            selectedMorphTargets = root.Q<VisualElement>("SelectedMorphTargets");

            for (var i = 0; i < avatarConfigTarget.MorphTargets.Count; i++)
            {
                var defaultIndex = AvatarMorphTarget.MorphTargetAvatarAPI.IndexOf(avatarConfigTarget.MorphTargets[i]);
                CreateNewElement(defaultIndex);
            }

            var addButton = root.Q<Button>("AddButton");
            addButton.clicked += OnAddButtonClicked;
        }

        private void OnAddButtonClicked()
        {
            Undo.RecordObject(avatarConfigTarget, ADD_MORPH_TARGET);
            avatarConfigTarget.MorphTargets.Add(AvatarMorphTarget.MorphTargetAvatarAPI[0]);
            EditorUtility.SetDirty(avatarConfigTarget);
            CreateNewElement(0);
        }

        private void CreateNewElement(int popFieldDefaultIndex)
        {
            var parent = new VisualElement();
            parent.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
            parent.style.justifyContent = new StyleEnum<Justify>(Justify.SpaceBetween);

            morphTargetsParentVisualElement.Add(parent, AvatarMorphTarget.MorphTargetAvatarAPI[popFieldDefaultIndex]);
            parent.Add(CreatePopupField(popFieldDefaultIndex, parent));
            parent.Add(CreateRemoveButton(parent));
            selectedMorphTargets.Add(parent);
        }

        private PopupField<Label> CreatePopupField(int defaultIndex, VisualElement parent)
        {
            return new PopupField<Label>(string.Empty,
                morphTargetLabels,
                defaultIndex,
                x =>
                {
                    avatarConfigTarget.MorphTargets[GetIndex(morphTargetsParentVisualElement[parent])] = x.text;
                    morphTargetsParentVisualElement[parent] = x.text;
                    return x.text;
                },
                x => x.text);
        }

        private VisualElement CreateRemoveButton(VisualElement parent)
        {
            var removeButton = new Button(() =>
            {
                Undo.RecordObject(avatarConfigTarget, DELETE_MORPH_TARGET);
                avatarConfigTarget.MorphTargets.RemoveAt(GetIndex(morphTargetsParentVisualElement[parent]));
                selectedMorphTargets.Remove(parent);
                EditorUtility.SetDirty(avatarConfigTarget);
            });
            removeButton.text = REMOVE_BUTTON_TEXT;
            return removeButton;
        }

        private int GetIndex(string morphTarget)
        {
            return avatarConfigTarget.MorphTargets.FindIndex(x => x == morphTarget);
        }
    }
}
