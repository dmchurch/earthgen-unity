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
        private Toggle AutoRegenerate;
        private Button GenerateTerrain;
        private Button GenerateClimate;

        private PlanetGenerator generator => serializedObject.targetObject as PlanetGenerator;

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement myInspector = new VisualElement();

            inspectorXML.CloneTree(myInspector);

            defaultInspector = myInspector.Q("Default_Inspector");
            InspectorElement.FillDefaultInspector(defaultInspector, serializedObject, this);
            myInspector.Insert(0, defaultInspector.ElementAt(1));
            defaultInspector.style.display = StyleKeyword.None;

            AutoRegenerate = myInspector.Q<Toggle>("AutoRegenerate");
            AutoRegenerate.RegisterCallback<ChangeEvent<bool>>((_) => CheckRegenerate());

            Toggle toggle = myInspector.Q<Toggle>("DefaultInspectorToggle");
            toggle.RegisterCallback<ChangeEvent<bool>>((evt) => showElement(defaultInspector, evt.newValue));

            var saveDataWarning = myInspector.Q<HelpBox>("saveDataWarning");
            saveDataWarning.style.display = generator.SaveGeneratedData ? StyleKeyword.Initial : StyleKeyword.None;

            PropertyField propField = myInspector.Q<PropertyField>("saveGeneratedData");
            propField.RegisterCallback<ChangeEvent<bool>>((evt) => showElement(saveDataWarning, generator.SaveGeneratedData = evt.newValue));

            GenerateTerrain = myInspector.Q<Button>("Generate_Terrain");
            GenerateClimate = myInspector.Q<Button>("Generate_Climate");
            GenerateTerrain.RegisterCallback<ClickEvent>((_) => { generator.GenerateTerrain(); CheckRegenerate(); });
            GenerateClimate.RegisterCallback<ClickEvent>((_) => { generator.GenerateClimate(); CheckRegenerate(); });

            myInspector.Q("terrainParameters").RegisterCallback<SerializedPropertyChangeEvent>((_) => CheckRegenerate(generator.terrainDirty = true));
            myInspector.Q("climateParameters").RegisterCallback<SerializedPropertyChangeEvent>((_) => CheckRegenerate(generator.climateDirty = true));
            
            CheckRegenerate();

            return myInspector;
        }

        private void CheckRegenerate(bool _ = false)
        {
            //Debug.Log($"Checking regenerate, terrainDirty={generator.terrainDirty}, climateDirty={generator.climateDirty}");
            if (AutoRegenerate.value) {
                if (generator.terrainDirty) {
                    generator.GenerateTerrain();
                }
                if (generator.climateDirty) {
                    generator.GenerateClimate();
                }
            }
            GenerateTerrain.SetEnabled(generator.terrainDirty);
            GenerateClimate.SetEnabled(generator.climateDirty);
        }

        private void showElement(VisualElement elem, bool newValue) => elem.style.display = newValue ? StyleKeyword.Initial : StyleKeyword.None;
    }
}
