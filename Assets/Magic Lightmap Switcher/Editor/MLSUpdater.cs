using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.Networking;

namespace MagicLightmapSwitcher
{
    [InitializeOnLoad]
    public static class MLSUpdater
    {         
        private static IEnumerator downloadNewVersionRoutine;
        private static IEnumerator checkForUpdatesRoutine;

        public static string installedVersion = "1.1.1";
        public static bool authorization;
        public static bool updateChecking;
        public static bool updateCheckingLoop;
        public static bool forceUpdateChecking;
        public static bool downloading;
        public static float downloadingProgress;
        public static bool forceCheck;

        [DidReloadScripts]
        public static void StartCheckForUpdatesLoop()
        {
            //checkForUpdatesRoutine = null;
            //checkForUpdatesRoutine = CheckForUpdatesEnumerator();
            //EditorApplication.update -= CheckForUpdatesLoop;
            //EditorApplication.update += CheckForUpdatesLoop;
        }

        private static void CheckForUpdatesLoop()
        {
            updateCheckingLoop = true;

            if (checkForUpdatesRoutine != null && checkForUpdatesRoutine.MoveNext())
            {
                return;
            }

            checkForUpdatesRoutine = null;
            checkForUpdatesRoutine = CheckForUpdatesEnumerator();
        }

        private static IEnumerator CheckForUpdatesEnumerator()
        {
            if (forceCheck)
            {
                forceUpdateChecking = true;
            }
            
            if (System.DateTime.Now.Hour != EditorPrefs.GetInt("MLS_PRO_lastCheck") || forceCheck)
            {
                forceCheck = false;
                EditorPrefs.SetInt("MLS_PRO_lastCheck", System.DateTime.Now.Hour);
            }
            else
            {
                yield break;
            }

            Dictionary<string, string> form = new Dictionary<string, string>();

            form.Add("checkForUpdates", "1"); 
            form.Add("version", "Pro");

            using (UnityWebRequest checkUpdate = UnityWebRequest.Post("https://www.motiongamesstudio.com/mls-update/checkForUpdates.php", form))
            {
                checkUpdate.SendWebRequest();

                while (!checkUpdate.isDone)
                {
                    yield return null;
                }
#if UNITY_2020_1_OR_NEWER
                if (checkUpdate.result == UnityWebRequest.Result.ConnectionError || 
                    checkUpdate.result == UnityWebRequest.Result.DataProcessingError ||
                    checkUpdate.result == UnityWebRequest.Result.ProtocolError)
#else
                if (checkUpdate.isNetworkError || checkUpdate.isHttpError)
#endif
                {
                    if (forceCheck)
                    {
                        Debug.LogFormat("<color=yellow>MLS Check For Updates:</color> Network Error: " + checkUpdate.error);
                    }
                }
                else
                {
                    char[] trims = { 'b' };
                    string currentVersion = installedVersion.TrimEnd(trims);
                    string latestVersion = checkUpdate.GetResponseHeader("Latest-Version").TrimEnd(trims); 

                    if (float.Parse(currentVersion, CultureInfo.InvariantCulture) < float.Parse(latestVersion, CultureInfo.InvariantCulture))
                    {
                        EditorPrefs.SetBool("MLS_PRO_newVersionAvailable", true);
                        EditorPrefs.SetString("MLS_PRO_installedVersion", installedVersion);
                        EditorPrefs.SetString("MLS_PRO_latestVersion", checkUpdate.GetResponseHeader("Latest-Version"));
                    } 
                    else
                    {
                        EditorPrefs.SetBool("MLS_PRO_newVersionAvailable", false);
                        EditorPrefs.SetString("MLS_PRO_installedVersion", installedVersion);
                    }
                }
            }

            forceUpdateChecking = false;
        }

        public static void StartDownload(string uName, string uInvoice)
        {
            downloadNewVersionRoutine = null;
            downloadNewVersionRoutine = DownloadUpdateEnumerator(uName, uInvoice);
            EditorApplication.update -= DownloadUpdate;
            EditorApplication.update += DownloadUpdate;
        }

        private static void DownloadUpdate()
        {
            updateChecking = true;

            if (downloadNewVersionRoutine != null && downloadNewVersionRoutine.MoveNext())
            {
                return;
            }

            EditorApplication.update -= DownloadUpdate;
            updateChecking = false;
        }

        private static IEnumerator DownloadUpdateEnumerator(string uName, string uInvoice)
        {
            Dictionary<string, string> form = new Dictionary<string, string>();

            form.Add("user_name", uName);
            form.Add("invoice_num", uInvoice);
            form.Add("installedVersion", EditorPrefs.GetString("MLS_PRO_installedVersion"));

            string savePath = string.Format("{0}/{1}", Application.dataPath, "MagicLightmapSwitcherTmp");

            using (UnityWebRequest download = UnityWebRequest.Post("https://www.motiongamesstudio.com/mls-update/checkForUpdates.php", form))
            {
                download.SendWebRequest();
                
                downloadingProgress = 0;

                while (!download.isDone)
                {
                    if (download.downloadProgress > 0)
                    {
                        downloading = true;
                        downloadingProgress = download.downloadProgress;
                    }

                    yield return null;
                }

                downloading = false;

#if UNITY_2020_1_OR_NEWER
                if (download.result == UnityWebRequest.Result.ConnectionError ||
                    download.result == UnityWebRequest.Result.DataProcessingError ||
                    download.result == UnityWebRequest.Result.ProtocolError)
#else
                if (download.isNetworkError || download.isHttpError)
#endif
                {
                    Debug.LogFormat("<color=yellow>MLS:</color> Network Error: " + download.error);
                }
                else
                {
                    if (download.GetResponseHeader("Content-Description") == "File Transfer")
                    {
                        authorization = false;

                        EditorPrefs.SetString("MLS_PRO_uName", uName);
                        EditorPrefs.SetString("MLS_PRO_uInvoice", uInvoice);
                        EditorPrefs.SetBool("MLS_PRO_Authorized", true);

                        System.IO.File.WriteAllBytes(savePath, download.downloadHandler.data);

                        AssetDatabase.ImportPackage(savePath, true);
                        AssetDatabase.importPackageCompleted += ImportPackageCompleted;
                    }
                    else
                    {
                        Debug.LogFormat("<color=yellow>MLS:</color> Authorization Error: " + download.downloadHandler.text);
                    }
                }
            }
        }

        private static void ImportPackageCompleted(string packageName)
        {
            EditorPrefs.SetBool("MLS_PRO_newVersionAvailable", false);
            EditorPrefs.SetString("MLS_PRO_installedVersion", installedVersion);
        }
    }
}
