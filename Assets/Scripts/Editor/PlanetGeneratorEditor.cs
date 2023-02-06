using Earthgen.planet.terrain;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Earthgen.unity
{
    [CustomEditor(typeof(PlanetGenerator))]
    public class PlanetGeneratorEditor : Editor
    {
        public VisualTreeAsset inspectorXML;

        private VisualElement defaultInspector;
        private Button GenerateTerrain;
        private Button GenerateClimate;
        private Button GenerateMeshes;
        private Button GenerateTextures;
        private TextField seedField;
        private Slider elevationScale;

        private PlanetGenerator generator => serializedObject.targetObject as PlanetGenerator;

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement myInspector = new VisualElement();

            inspectorXML.CloneTree(myInspector);

            defaultInspector = myInspector.Q("Default_Inspector");
            InspectorElement.FillDefaultInspector(defaultInspector, serializedObject, this);
            myInspector.Insert(0, defaultInspector.ElementAt(1));
            defaultInspector.style.display = StyleKeyword.None;

            myInspector.Q<Toggle>("AutoRegenerate").RegisterCallback<ChangeEvent<bool>>((_) => CheckRegenerate());
            myInspector.Q<Toggle>("AutoRender").RegisterCallback<ChangeEvent<bool>>((_) => CheckRegenerate());

            Toggle toggle = myInspector.Q<Toggle>("DefaultInspectorToggle");
            toggle.RegisterCallback<ChangeEvent<bool>>((evt) => showElement(defaultInspector, evt.newValue));

            HelpBox saveDataWarning = myInspector.Q<HelpBox>("saveDataWarning");
            HelpBox saveObjectsWarning = myInspector.Q<HelpBox>("saveObjectsWarning");
            showElement(saveDataWarning, generator.SaveGeneratedData);
            showElement(saveObjectsWarning, generator.SaveRenderedObjects);

            myInspector.Q<Toggle>("saveGeneratedData").RegisterCallback<ChangeEvent<bool>>(
                (evt) => showElement(saveDataWarning, generator.SaveGeneratedData = evt.newValue));
            myInspector.Q<Toggle>("saveRenderedObjects").RegisterCallback<ChangeEvent<bool>>(
                (evt) => showElement(saveObjectsWarning, generator.SaveRenderedObjects = evt.newValue));

            GenerateTerrain = myInspector.Q<Button>("Generate_Terrain");
            GenerateClimate = myInspector.Q<Button>("Generate_Climate");
            GenerateMeshes = myInspector.Q<Button>("Generate_Meshes");
            GenerateTextures = myInspector.Q<Button>("Generate_Textures");
            GenerateTerrain.RegisterCallback<ClickEvent>((_) => { try { generator.GenerateTerrain(); } finally { CheckRegenerate(); } });
            GenerateClimate.RegisterCallback<ClickEvent>((_) => { try { generator.GenerateClimate(); } finally { CheckRegenerate(); } });
            GenerateMeshes.RegisterCallback<ClickEvent>((_) => { try { generator.GenerateMeshes(); } finally { CheckRegenerate(); } });
            GenerateTextures.RegisterCallback<ClickEvent>((_) => { try { generator.GenerateTextures(); } finally { CheckRegenerate(); } });

            seedField = myInspector.Q<TextField>("seed");

            myInspector.Q("grid_size").RegisterCallback<ChangeEvent<int>>((_) => CheckRegenerate(generator.terrainDirty = generator.climateDirty = true));
            myInspector.Q("axis").RegisterCallback<ChangeEvent<Vector3>>((_) => CheckRegenerate(generator.terrainDirty = true));
            myInspector.Q("seedSource").RegisterCallback<ChangeEvent<Enum>>((evt) => CheckRegenerate(generator.terrainDirty |= (PlanetGenerator.SeedSource)evt.newValue != PlanetGenerator.SeedSource.Specified));
            seedField.RegisterCallback<ChangeEvent<string>>((_) => CheckRegenerate(generator.terrainDirty |= generator.seedSource != PlanetGenerator.SeedSource.Random));
            myInspector.Q("iterations").RegisterCallback<ChangeEvent<int>>((_) => CheckRegenerate(generator.terrainDirty = true));
            myInspector.Q("waterPercentage").RegisterCallback<ChangeEvent<float>>((_) => CheckRegenerate(generator.terrainDirty = true));

            myInspector.Q("seasons").RegisterCallback<ChangeEvent<int>>((_) => CheckRegenerate(generator.climateDirty = true));
            myInspector.Q("axialTiltInDegrees").RegisterCallback<ChangeEvent<float>>((_) => CheckRegenerate(generator.climateDirty = true));
            myInspector.Q("error_tolerance").RegisterCallback<ChangeEvent<float>>((_) => CheckRegenerate(generator.climateDirty = true));

            myInspector.Q("modelTopography").RegisterCallback<ChangeEvent<bool>>((_) => CheckRegenerate(generator.meshDirty = true));
            elevationScale = myInspector.Q<Slider>("elevationScale");
            elevationScale.RegisterCallback<ChangeEvent<float>>((_) => CheckRegenerate(generator.meshDirty |= generator.meshParameters.modelTopography));

            myInspector.Q("colorScheme").RegisterCallback<ChangeEvent<Enum>>((_) => CheckRegenerate(generator.textureDirty = true));
            myInspector.Q("timeOfYear").RegisterCallback<ChangeEvent<float>>((_) => CheckRegenerate(generator.textureDirty |= true));

            myInspector.Q<Foldout>("Generation_Settings").value = generator.terrainDirty || generator.climateDirty;
            
            CheckRegenerate();

            return myInspector;
        }

        private void CheckRegenerate(bool _ = false)
        {
            //Debug.Log($"Checking regenerate, terrainDirty={generator.terrainDirty}, climateDirty={generator.climateDirty}");
            generator.AutoRender();
            seedField.SetEnabled(generator.seedSource == PlanetGenerator.SeedSource.Specified);
            elevationScale.SetEnabled(generator.meshParameters.modelTopography);
            GenerateTerrain.SetEnabled(generator.terrainDirty || generator.seedSource == PlanetGenerator.SeedSource.Random);
            GenerateClimate.SetEnabled(generator.climateDirty);
            GenerateMeshes.SetEnabled(generator.meshDirty);
            GenerateTextures.SetEnabled(generator.textureDirty);
        }

        private void showElement(VisualElement elem, bool newValue) => elem.style.display = newValue ? StyleKeyword.Initial : StyleKeyword.None;
    }
}
