using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MagicLightmapSwitcher
{
    public static class MLSProgressBarHelper 
    {
        private static float totalProgress;
        private static int totalProgressCounter = 0;
        private static int totalProgressFrameSkipper = 0;

        private static string currentStageName;
        private static float currentStageProgress;
        private static int currentStageProgressCounter = 0;
        private static int currentStageProgressFrameSkipper = 0;      

        public static void DrawTwoLineProgressBar(int totalStages)
        {
            GUILayout.Label("Total Progress", MLSEditorUtils.captionStyle);
            EditorGUI.ProgressBar(
                EditorGUILayout.BeginVertical(),

                (float) totalProgress,
                "Storing Lighmap Data...");
            GUILayout.Space(20);

            GUILayout.EndVertical();

            GUILayout.Label("Current Stage Progress", MLSEditorUtils.captionStyle);
            EditorGUI.ProgressBar(
                EditorGUILayout.BeginVertical(),

                currentStageProgress / 100.0f,
                currentStageName + " - " + Mathf.RoundToInt(currentStageProgress).ToString() + "%");

            GUILayout.Space(20);
            GUILayout.EndVertical();
        }

        public static void ResetProgress()
        {
            totalProgress = 0;
            totalProgressCounter = 0;
            currentStageProgress = 0;
        }

        public static void StartNewStage(string name)
        {
            currentStageName = name;
            currentStageProgress = 0;
            currentStageProgressCounter = 0;
            currentStageProgressFrameSkipper = 0;
        }

        public static bool UpdateProgress(int count, int period = 100)
        {
            currentStageProgress = ((float) currentStageProgressCounter / (float) count) * 100.0f;
            currentStageProgressCounter++;

            if (period > 0)
            {
                currentStageProgressFrameSkipper++;

                if (currentStageProgressFrameSkipper == period)
                {
                    currentStageProgressFrameSkipper = 0;

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        public static bool UpdateTotalProgress(int count, int period = 100)
        {
            totalProgress = ((float) totalProgressCounter / (float) count) * 100.0f;
            totalProgressCounter++;

            if (period > 0)
            {
                totalProgressFrameSkipper++;

                if (totalProgressFrameSkipper == period)
                {
                    totalProgressFrameSkipper = 0;

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }
    }
}
