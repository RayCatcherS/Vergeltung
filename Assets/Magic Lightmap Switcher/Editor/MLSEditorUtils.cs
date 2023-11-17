using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MagicLightmapSwitcher
{
    public static class MLSEditorUtils
    {
        public static GUIStyle captionStyle;
        public static GUIStyle caption_1_Style;
        public static GUIStyle labelCenteredStyle;
        public static GUIStyle labelLeftStyle;
        public static GUIStyle labelRightStyle;
        public static GUIStyle boldLabelStyle;
        public static GUIStyle warningLabelStyle;
        public static GUIStyle centeredFoldoutLabel;
        public static GUIStyle greenLabelStyle;
        public static GUIStyle wrappedLabelStyle;
        public static GUIStyle blackBorderBoxStyle;
        public static GUIStyle bigButtonStyle;
        public static bool stylesInitialized;
        public static Rect globalBlendSlider;
        public static float cycleLength = 30;
        public static GUIStyle rollHeaderSilver;
        public static GUIStyle presetMainFoldout;

        public static void InitStyles()
        {
            stylesInitialized = true;            

            presetMainFoldout = new GUIStyle(EditorStyles.foldout);
            presetMainFoldout.fontStyle = FontStyle.Bold;
            presetMainFoldout.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
            presetMainFoldout.onNormal.textColor = Color.white;

            bigButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fixedHeight = 50
            };

            wrappedLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                wordWrap = true
            };

            captionStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(),
                padding = new RectOffset(10, 10, 5, 5),
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                wordWrap = true
            };

            caption_1_Style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(),
                padding = new RectOffset(10, 10, 5, 5),
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                wordWrap = true
            };

            labelCenteredStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(),
                padding = new RectOffset(0, 0, 5, 0),
                fontSize = 11,
                wordWrap = true
            };

            labelLeftStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                margin = new RectOffset(),
                padding = new RectOffset(0, 0, 5, 0),
                fontSize = 11,
                wordWrap = true
            };

            labelRightStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleRight,
                margin = new RectOffset(),
                padding = new RectOffset(0, 0, 5, 0),
                fontSize = 11,
                wordWrap = true
            };

            centeredFoldoutLabel = new GUIStyle(EditorStyles.foldout)
            {
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(),
                padding = new RectOffset(0, 0, 5, 0),
                fontSize = 11
            };

            boldLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                margin = new RectOffset(),
                padding = new RectOffset(0, 0, 3, 0),
                fontSize = 11,
                fontStyle = FontStyle.Bold
            };

            warningLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(),
                padding = new RectOffset(10, 10, 5, 5),
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                fixedWidth = 150
            };

            greenLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11
            };

            greenLabelStyle.normal.textColor = Color.green;
            warningLabelStyle.normal.textColor = Color.yellow;
        }

        public static GUIStyle DrawColorBox(Color backgroundColor)
        {
            GUIStyle newStyle = new GUIStyle();
            newStyle.normal.background = MakeTex(1, 1, backgroundColor);

            return newStyle;
        }
        static public bool DrawHeader(string text, bool forceOn, Color backoundColor, List<StoredLightingScenario> storedLightmapScenarios, int removeIndex)
        {
            return DrawHeader(text, text, forceOn, backoundColor, storedLightmapScenarios, removeIndex);
        }

        static public bool DrawHeader(string text, bool forceOn, Color backoundColor)
        {
            return DrawHeader(text, text, forceOn, backoundColor);
        }

        static public bool DrawHeader(string text, string key, bool forceOn, Color backgroundColor, List<StoredLightingScenario> storedLightmapScenarios, int removeIndex)
        {
            bool state = EditorPrefs.GetBool(key, true);
            GUIStyle myStyle = new GUIStyle();
            myStyle.fontStyle = FontStyle.Bold;
            myStyle.normal.textColor = Color.gray * 1.5f;
            //myStyle.normal.background = MakeTex(1, 1, backgroundColor);

            GUILayout.Space(3f);
            
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(3f);

                GUI.changed = false;

                text = "<b><size=11>" + text + "</size></b>";
                if (state)
                {
                    text = "\u25BC " + text;
                }
                else
                {
                    text = "\u25B6 " + text;
                }

                if (!GUILayout.Toggle(true, text, myStyle, GUILayout.MinWidth(20f)))
                {
                    state = !state;
                }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Remove"))
                {
                    MLSManager.RemoveStoredLightingScenario(removeIndex);
                    return false;
                }

                if (GUI.changed)
                {
                    EditorPrefs.SetBool(key, state);
                }

                GUILayout.Space(2f);
            }

            GUI.color = GUI.contentColor;

            if (!forceOn && !state)
            {
                GUILayout.Space(3f);
            }

            return state;
        }

        static public bool DrawHeader(string text, string key, bool forceOn, Color backgroundColor)
        {
            bool state = EditorPrefs.GetBool(key, true);
            GUIStyle myStyle = new GUIStyle();
            myStyle.fontStyle = FontStyle.Bold;
            myStyle.normal.textColor = Color.gray * 1.5f;
            myStyle.normal.background = MakeTex(1, 1, backgroundColor);

            GUILayout.Space(3f);


            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(3f);

                GUI.changed = false;

                text = "<b><size=11>" + text + "</size></b>";
                if (state)
                {
                    text = "\u25BC " + text;
                }
                else
                {
                    text = "\u25B6 " + text;
                }

                if (!GUILayout.Toggle(true, text, myStyle, GUILayout.MinWidth(20f)))
                {
                    state = !state;
                }

                if (GUI.changed)
                {
                    EditorPrefs.SetBool(key, state);
                }

                GUILayout.Space(2f);
            }

            GUI.color = GUI.contentColor;

            if (!forceOn && !state)
            {
                GUILayout.Space(3f);
            }

            return state;
        }

        private static Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width*height];

            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();

            return result;
        }

        private static object mainWindow;
        private static bool initializedMainViewMetadata;
        private static Type containerWinType;
        private static FieldInfo showModeField;
        private static PropertyInfo positionProperty;

        public static void CenterOnMainWin(this EditorWindow window)
        {
            Rect    main = GetEditorMainWindowPos();
            Rect    pos = window.position;
            float    w = (main.width - pos.width) * 0.5f;
            float    h = (main.height - pos.height) * 0.5f;

            pos.x = main.x + w;
            pos.y = main.y + h;
            window.position = pos;
        }

        public static Rect GetEditorMainWindowPos()
        {
            LazyInitializeMainViewMetadata();

            if (containerWinType == null)
                return default(Rect);

            if (mainWindow == null || mainWindow.Equals(null) == true || UnityEngine.Object.Equals(mainWindow, "null") == true)
            {
                UnityEngine.Object[]    windows = Resources.FindObjectsOfTypeAll(containerWinType);
                foreach (UnityEngine.Object win in windows)
                {
                    int    showmode = (int)showModeField.GetValue(win);
                    if (showmode == 4) // main window
                    {
                        mainWindow = win;
                        break;
                    }
                }
            }

            if (mainWindow.Equals(null) == false)
                return (Rect) positionProperty.GetValue(mainWindow, null);

            throw new NotSupportedException("Can't find internal main window. Maybe something has changed inside Unity.");
        }

        private static void LazyInitializeMainViewMetadata()
        {
            containerWinType = typeof(Editor).Assembly.GetType();
            if (containerWinType != null)
            {
                showModeField = containerWinType.GetField("m_ShowMode");
                positionProperty = containerWinType.GetProperty("position");

                if (showModeField == null || positionProperty == null)
                    containerWinType = null;
            }
        }

        public static void DrawBlendingConfigurator(MagicLightmapSwitcher activeSwitcherSource, StoredLightingScenario storedLightingScenario)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(MLSTooltipManager.MainComponent.GetParameter("Cycle Length", MLSTooltipManager.MainComponent.Tabs.LightmapScenario), GUILayout.MaxWidth(200));

                cycleLength = EditorGUILayout.FloatField(cycleLength, GUILayout.MaxWidth(250));

                if (!storedLightingScenario.stopPreviewPlayback)
                {
                    if (GUILayout.Button("Stop", GUILayout.MaxWidth(50)))
                    {
                        storedLightingScenario.StopEditorBlendingPreview();
                    }
                }
                else
                {
                    if (GUILayout.Button("Play", GUILayout.MaxWidth(50)))
                    {
                        if (cycleLength == 0)
                        {
                            EditorUtility.DisplayDialog("Magic Lightmap Switcher", "Cycle length must be greater than 0.", "OK");
                        }
                        else
                        {
                            storedLightingScenario.StartEditorBlendingPreview(cycleLength);
                        }
                    }
                }                

                GUILayout.FlexibleSpace();
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(MLSTooltipManager.MainComponent.GetParameter("Ranges Setting Mode", MLSTooltipManager.MainComponent.Tabs.LightmapScenario), GUILayout.MaxWidth(200));

                storedLightingScenario.rangesSettingMode = (StoredLightingScenario.RangesSettingMode)
                                EditorGUILayout.EnumPopup(storedLightingScenario.rangesSettingMode, GUILayout.MaxWidth(300));
            }

            GUI.enabled = true;

            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                GUILayout.Label("Global Blend", MLSEditorUtils.captionStyle);

                GUILayout.Space(15);

                if (storedLightingScenario.stopPreviewPlayback)
                {
                    storedLightingScenario.globalBlendFactor = GUILayout.HorizontalSlider(storedLightingScenario.globalBlendFactor, 0, 1);

                    if (storedLightingScenario.prevBlendValue != storedLightingScenario.globalBlendFactor && !Application.isPlaying)
                    {
                        activeSwitcherSource.currentLightmapScenario = storedLightingScenario;
                        storedLightingScenario.prevBlendValue = storedLightingScenario.globalBlendFactor;

                        Blending.Blend(activeSwitcherSource, storedLightingScenario.globalBlendFactor, storedLightingScenario, storedLightingScenario.targetScene);
                    }
                }
                else
                {
                    GUILayout.HorizontalSlider(storedLightingScenario.globalBlendFactor, 0, 1);
                }

                globalBlendSlider = new Rect(GUILayoutUtility.GetLastRect());

                GUILayout.Space(30);

                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    GUILayout.Label("Reflections Blending Range", MLSEditorUtils.captionStyle);
                    GUILayout.Space(30);

                    if (globalBlendSlider != null)
                    {
                        Rect arrowReflectionsRangeTopRect = new Rect(globalBlendSlider);
                        Rect arrowReflectionsRangeDownRect = new Rect(globalBlendSlider);
                        Rect arrowLightmapsRangeTopRect = new Rect(globalBlendSlider);
                        Rect arrowLightmapsRangeDownRect = new Rect(globalBlendSlider);


                        float xPos =
                    (globalBlendSlider.position.x - (globalBlendSlider.width / 2)) +
                    (((globalBlendSlider.position.x + ((globalBlendSlider.width - 8) / 2)) -
                    (globalBlendSlider.position.x - ((globalBlendSlider.width - 8) / 2))) *
                    storedLightingScenario.globalBlendFactor) + 8;

                        arrowReflectionsRangeTopRect.y += 85;
                        arrowReflectionsRangeTopRect.x = xPos;
                        arrowReflectionsRangeDownRect.y += 60;
                        arrowReflectionsRangeDownRect.x = xPos;
                        arrowLightmapsRangeTopRect.y += 147;
                        arrowLightmapsRangeTopRect.x = xPos;
                        arrowLightmapsRangeDownRect.y += 124;
                        arrowLightmapsRangeDownRect.x = xPos;

                        EditorGUI.LabelField(arrowReflectionsRangeTopRect, "\u25B2 ", MLSEditorUtils.labelCenteredStyle);
                        //EditorGUI.LabelField(arrowReflectionsRangeDownRect, "\u25BC ", MLSEditorUtils.labelCenteredStyle);
                        EditorGUI.LabelField(arrowLightmapsRangeTopRect, "\u25B2 ", MLSEditorUtils.labelCenteredStyle);
                        //EditorGUI.LabelField(arrowLightmapsRangeDownRect, "\u25BC ", MLSEditorUtils.labelCenteredStyle);

                        for (int j = 0; j < storedLightingScenario.blendableLightmaps.Count; j++)
                        {
                            Rect reflectionsRangeSliderRect = new Rect();
                            float rangeSliderWidth;

                            if (j == 0)
                            {

                            }
                            else if (j == storedLightingScenario.blendableLightmaps.Count - 1)
                            {
                                rangeSliderWidth =
                                    globalBlendSlider.width *
                                    (1 -
                                    storedLightingScenario.blendableLightmaps[j - 1].startValue) - 5;

                                reflectionsRangeSliderRect =
                                    new Rect(
                                        globalBlendSlider.position.x + (globalBlendSlider.width * 1) - rangeSliderWidth - 2.5f,
                                        globalBlendSlider.position.y + 75,
                                        rangeSliderWidth, 20);
                            }
                            else
                            {
                                rangeSliderWidth =
                                    globalBlendSlider.width *
                                    (storedLightingScenario.blendableLightmaps[j].startValue -
                                    storedLightingScenario.blendableLightmaps[j - 1].startValue) - 5;

                                reflectionsRangeSliderRect =
                                    new Rect(
                                        globalBlendSlider.position.x + (globalBlendSlider.width * storedLightingScenario.blendableLightmaps[j].startValue) - rangeSliderWidth - 2.5f,
                                        globalBlendSlider.position.y + 75,
                                        rangeSliderWidth, 20);
                            }

                            if (j > 0)
                            {
                                EditorGUI.BeginChangeCheck();

                                EditorGUI.MinMaxSlider(
                                    reflectionsRangeSliderRect,
                                    ref storedLightingScenario.blendableLightmaps[j].reflectionsBlendingRange.x,
                                    ref storedLightingScenario.blendableLightmaps[j].reflectionsBlendingRange.y,
                                    0, 1);

                                if (EditorGUI.EndChangeCheck())
                                {
                                    StoredLightingScenario.LightmapData currentSlider = storedLightingScenario.blendableLightmaps[j];
                                    StoredLightingScenario.LightmapData dependentSlider = storedLightingScenario.blendableLightmaps[storedLightingScenario.blendableLightmaps.Count - j];

                                    if (currentSlider != dependentSlider)
                                    {
                                        switch (storedLightingScenario.rangesSettingMode)
                                        {
                                            case StoredLightingScenario.RangesSettingMode.Copy:
                                                storedLightingScenario.blendableLightmaps[storedLightingScenario.blendableLightmaps.Count - j].
                                                    reflectionsBlendingRange.x = storedLightingScenario.blendableLightmaps[j].reflectionsBlendingRange.x;

                                                storedLightingScenario.blendableLightmaps[storedLightingScenario.blendableLightmaps.Count - j].
                                                    reflectionsBlendingRange.y = storedLightingScenario.blendableLightmaps[j].reflectionsBlendingRange.y;
                                                break;
                                            case StoredLightingScenario.RangesSettingMode.Mirror:
                                                storedLightingScenario.blendableLightmaps[storedLightingScenario.blendableLightmaps.Count - j].
                                                    reflectionsBlendingRange.y = 1;
                                                storedLightingScenario.blendableLightmaps[storedLightingScenario.blendableLightmaps.Count - j].
                                                    reflectionsBlendingRange.y -= storedLightingScenario.blendableLightmaps[j].reflectionsBlendingRange.x;

                                                storedLightingScenario.blendableLightmaps[storedLightingScenario.blendableLightmaps.Count - j].
                                                    reflectionsBlendingRange.x = 1;
                                                storedLightingScenario.blendableLightmaps[storedLightingScenario.blendableLightmaps.Count - j].
                                                    reflectionsBlendingRange.x -= storedLightingScenario.blendableLightmaps[j].reflectionsBlendingRange.y;
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    GUILayout.Label("Lightmaps Blending Range", MLSEditorUtils.captionStyle);
                    GUILayout.Space(30);

                    if (globalBlendSlider != null)
                    {
                        for (int j = 0; j < storedLightingScenario.blendableLightmaps.Count; j++)
                        {
                            Rect lightmapsRangeSliderRect = new Rect();
                            float rangeSliderWidth;

                            if (j == 0)
                            {

                            }
                            else if (j == storedLightingScenario.blendableLightmaps.Count - 1)
                            {
                                rangeSliderWidth =
                                    globalBlendSlider.width *
                                    (1 -
                                    storedLightingScenario.blendableLightmaps[j - 1].startValue) - 5;

                                lightmapsRangeSliderRect =
                                    new Rect(
                                        globalBlendSlider.position.x + (globalBlendSlider.width * 1) - rangeSliderWidth - 2.5f,
                                        globalBlendSlider.position.y + 137,
                                        rangeSliderWidth, 20);
                            }
                            else
                            {
                                rangeSliderWidth =
                                    globalBlendSlider.width *
                                    (storedLightingScenario.blendableLightmaps[j].startValue -
                                    storedLightingScenario.blendableLightmaps[j - 1].startValue) - 5;

                                lightmapsRangeSliderRect =
                                    new Rect(
                                        globalBlendSlider.position.x + (globalBlendSlider.width * storedLightingScenario.blendableLightmaps[j].startValue) - rangeSliderWidth - 2.5f,
                                        globalBlendSlider.position.y + 137,
                                        rangeSliderWidth, 20);
                            }

                            if (j > 0)
                            {
                                EditorGUI.BeginChangeCheck();

                                EditorGUI.MinMaxSlider(
                                    lightmapsRangeSliderRect,
                                    ref storedLightingScenario.blendableLightmaps[j].lightmapBlendingRange.x,
                                    ref storedLightingScenario.blendableLightmaps[j].lightmapBlendingRange.y,
                                    0, 1);

                                if (EditorGUI.EndChangeCheck())
                                {
                                    StoredLightingScenario.LightmapData currentSlider = storedLightingScenario.blendableLightmaps[j];
                                    StoredLightingScenario.LightmapData dependentSlider = storedLightingScenario.blendableLightmaps[storedLightingScenario.blendableLightmaps.Count - j];

                                    if (currentSlider != dependentSlider)
                                    {
                                        switch (storedLightingScenario.rangesSettingMode)
                                        {
                                            case StoredLightingScenario.RangesSettingMode.Copy:
                                                storedLightingScenario.blendableLightmaps[storedLightingScenario.blendableLightmaps.Count - j].
                                                    lightmapBlendingRange.x = storedLightingScenario.blendableLightmaps[j].lightmapBlendingRange.x;

                                                storedLightingScenario.blendableLightmaps[storedLightingScenario.blendableLightmaps.Count - j].
                                                    lightmapBlendingRange.y = storedLightingScenario.blendableLightmaps[j].lightmapBlendingRange.y;
                                                break;
                                            case StoredLightingScenario.RangesSettingMode.Mirror:
                                                storedLightingScenario.blendableLightmaps[storedLightingScenario.blendableLightmaps.Count - j].
                                                    lightmapBlendingRange.y = 1;
                                                storedLightingScenario.blendableLightmaps[storedLightingScenario.blendableLightmaps.Count - j].
                                                    lightmapBlendingRange.y -= storedLightingScenario.blendableLightmaps[j].lightmapBlendingRange.x;

                                                storedLightingScenario.blendableLightmaps[storedLightingScenario.blendableLightmaps.Count - j].
                                                    lightmapBlendingRange.x = 1;
                                                storedLightingScenario.blendableLightmaps[storedLightingScenario.blendableLightmaps.Count - j].
                                                    lightmapBlendingRange.x -= storedLightingScenario.blendableLightmaps[j].lightmapBlendingRange.y;
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                using (new GUILayout.HorizontalScope())
                {
                    for (int j = 0; j < storedLightingScenario.blendableLightmaps.Count; j++)
                    {
                        if (storedLightingScenario.blendableLightmaps[j].lightingData == null)
                        {
                            storedLightingScenario.blendableLightmaps.Clear();
                            return;
                        }

                        if (j == storedLightingScenario.startValues.Count)
                        {
                            storedLightingScenario.blendableLightmaps.RemoveAt(j);
                            return;
                        }

                        storedLightingScenario.blendableLightmaps[j].blendingIndex = j;
                        storedLightingScenario.blendableLightmaps[j].startValue = storedLightingScenario.startValues[j];

                        Rect labelRect = new Rect();
                        Rect mark = new Rect(globalBlendSlider);

                        mark.size = new Vector2(1, 10);

                        if (j == 0)
                        {
                            labelRect = new Rect(globalBlendSlider.position.x, globalBlendSlider.position.y + 20, 50, 20);

                            mark.position = new Vector2(
                            globalBlendSlider.position.x,
                            globalBlendSlider.position.y + 15);

                            EditorGUI.LabelField(
                                labelRect,
                                storedLightingScenario.blendableLightmaps[j].lightingData.dataName.ToString(),
                                labelLeftStyle);
                        }
                        else if (j == storedLightingScenario.blendableLightmaps.Count - 1)
                        {
                            labelRect = new Rect(globalBlendSlider.position.x + (globalBlendSlider.width - 50), globalBlendSlider.position.y + 20, 50, 20);

                            mark.position = new Vector2(
                            globalBlendSlider.position.x + globalBlendSlider.width,
                            globalBlendSlider.position.y + 15);

                            EditorGUI.LabelField(
                                labelRect,
                                storedLightingScenario.blendableLightmaps[j].lightingData.dataName.ToString(),
                                labelRightStyle);
                        }
                        else
                        {
                            labelRect = new Rect(
                            globalBlendSlider.position.x + (globalBlendSlider.width * storedLightingScenario.blendableLightmaps[j].startValue) - 23,
                            globalBlendSlider.position.y + 20, 50, 20);

                            mark.position = new Vector2(
                            globalBlendSlider.position.x + (globalBlendSlider.width * storedLightingScenario.blendableLightmaps[j].startValue),
                            globalBlendSlider.position.y + 15);

                            EditorGUI.LabelField(
                                labelRect,
                                storedLightingScenario.blendableLightmaps[j].lightingData.dataName.ToString(),
                                labelCenteredStyle);
                        }

                        EditorGUI.DrawRect(mark, Color.gray);

                        Rect startValueRect = new Rect(mark);

                        startValueRect.x -= 25;
                        startValueRect.y -= 30;
                        startValueRect.width = 50;
                        startValueRect.height = 15;

                        if (j > 0 && j < storedLightingScenario.blendableLightmaps.Count - 1)
                        {
                            EditorGUI.LabelField(startValueRect, storedLightingScenario.blendableLightmaps[j].startValue.ToString(), MLSEditorUtils.labelCenteredStyle);
                        }
                    }
                }
            }
        }
    }
}