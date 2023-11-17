using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MagicLightmapSwitcher
{
    [InitializeOnLoad]
    public static class MLSTooltipManager
    {
        static MLSTooltipManager()
        {
            MainComponent.GenerateTooltips();
        }

        public static class MainComponent
        {
            public enum Tabs
            {
                Storing,
                LightmapScenario
            }

            private static Dictionary<string, string> mainComponenStoring;
            private static Dictionary<string, string> lightmapScenario;

            public static void GenerateTooltips()
            {
                mainComponenStoring = new Dictionary<string, string>();
                lightmapScenario = new Dictionary<string, string>();

                #region Storing

                mainComponenStoring.Add("Lightmapper", "If Bakery is installed in your project, then you can choose with which lightmapper to bake the scene.");
                mainComponenStoring.Add("Workflow", "Choose your workflow.");
                mainComponenStoring.Add("Storing Mode", "The \"Storing Mode\" parameter allows you to select the mode in which lightmaps will be baked and stored. \r\n" +
                    "\"Once\"\r\n" +
                    "In this mode, the ilightmap with the current settings will be baked and stored under the name you " +
                    "specified. You can also bake a lightmap yourself using the standard Unity \"Lighting\" window. " +
                    "To save the lightmap, click the \"Store Baked Lightmap\" button.\r\n" +
                    "\"Queue\"\r\n" +
                    "In this mode, baking and storing occurs in automatic mode for each saved preset sequentially. " +
                    "Each stored lightmap will have the name of the preset from which the settings were taken.");
                mainComponenStoring.Add("Lightmap Name", "The name of the lightmap to save.");
                mainComponenStoring.Add("Create Lightmap Scenario", "Automatically create a blending scenario and add all stored lightmaps to it.");
                mainComponenStoring.Add("Scenario To Replace Data", "This option allows you to overwrite the data in the Scenario Blending Queue.\r\n" +
                    "If \"-None-\" is selected, a new scenario will be created, if a scenario name is selected, the settings for that scenario will be " +
                    "reset and the blending queue will be filled with newly baked lightmaps.");
                mainComponenStoring.Add("Clear Default Data Folder", "If checked, stored copies of lightmaps will be automatically removed from the default folder.");
                mainComponenStoring.Add("Store Path Mode", 
                    "\"Scene Relative\"\r\n" +
                    "The data will be saved in the folder next to the scene file (\"SceneName_MLS_DATA\").\r\n" +
                    "\"Custom\"\r\n" +
                    "The data will be saved in the path you specified relative to the \"Assets\" folder.");
                mainComponenStoring.Add("Custom Path", "The path where the lighting data will be stored. (\"Assets\" folder releative)");
                mainComponenStoring.Add("Load From Asset Bundles", "Activate this option if your project uses AssetBundles and you need " +
                    "to have control over when and how much assets are loaded. Upon activation, automatic loading will be disabled. The system " +
                    "will tell you where to add your own code for further work.");                

                #endregion

                #region Lightmap Scenario

                lightmapScenario.Add("Scenario Name", "Lightmap Scenario name.");
                lightmapScenario.Add("Blending Modules", "Which of the blending modules will be used for this scenario.");
                lightmapScenario.Add("Cyclic Blend", "If true, a lightmap at index 0 will be added to the end of the blend queue.");
                lightmapScenario.Add("Cycle Length", "The \"Cycle Length\" parameter is only used for previewing in the editor. Set the desired time in seconds and click \"Play\".");
                lightmapScenario.Add("Ranges Setting Mode",
                    "Allows you to choose how to customize the blending ranges. There are three options available.\r\n" +
                    "\"Separate\" - each range is adjusted separately.\r\n" +
                    "\"Mirror\" - changes in one range will be mirrored to the range with the index counted from the end of the queue.\r\n" +
                    "\"Copy\" - changes in one range will be copied to the range with the index counted from the end of the queue.");
                #endregion
            }

            public static GUIContent GetParameter(string name, Tabs componentTab)
            {
                string tooltip = "";

                switch (componentTab)
                {
                    case Tabs.Storing:
                        if (!mainComponenStoring.TryGetValue(name, out tooltip))
                        {
                            tooltip = "No description for this parameter.";
                        }
                        break;
                    case Tabs.LightmapScenario:
                        if (!lightmapScenario.TryGetValue(name, out tooltip))
                        {
                            tooltip = "No description for this parameter.";
                        }
                        break;
                }

                return new GUIContent(name, tooltip);
            }
        }
    }
}
