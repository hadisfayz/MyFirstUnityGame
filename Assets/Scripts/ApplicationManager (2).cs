using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * ApplicationManager.cs (in empty GO ApplicationManager)
 * Version 2.2
 * 
 * author: Alexander Nischelwitzer, FH JOANNEUM, IMA, DMT, NIS
 * last changed: 22.03.2020
 * 
 * description:
 * Unity DMT Tools for general use, Config at start UP, etc.
 * Screen Init, Startup Logging, Cursor On/Off, Window resize, etc.
 * 
 * Features with public var
 * 
 * c ... Cursor On/Off
 * f ... full screen to window toggle - default: full screen
 * ! ... inGame DebugConsole - default: not shown
 * r ... reset playerPrefs (delete) 
 * i ... infos
 * esc ... Quit
 * 
 * generale Features with IngameDebugConsole (always needed)
 * https://assetstore.unity.com/packages/tools/gui/in-game-debug-console-68068
 * 
 */


namespace DMT
{
    /// <summary>
    /// ApplicationManager with general inits and tools
    /// </summary>
    /// <remarks>
    /// needs the wonderful IngameDebugConsole from yasirkula 
    /// </remarks>

    public class ApplicationManager : MonoBehaviour
    {
        [Space(1, order = 0)]
        [Header("DMT Application Manager with FullHD Configuration", order = 1)]
        [Space(-8, order = 2)]
        [Header("press [h] for Help when running", order = 3)]
        [Space(10, order = 4)]

        [Tooltip("Hide (true) the Cursor at startup [c]")]
        public bool hideCursor = true;

        [Tooltip("Hide/Show the InGame DebugConsole at startup [!]")]
        public bool hideConsole = true;  // inGame DebugConsole

        [Space(10)]
        [Header("Screen resolution parameters")]
        [Tooltip("Use FullHD LandScape Mode, off = portrait Mode")]
        public bool screenLandscape = true;
        [Tooltip("X Resolution [1920 px]")]
        public int resX = 1920;
        [Tooltip("Y Resolution [1080 px]")]
        public int resY = 1080;

        // ---------------------------------------------------------

        // install https://assetstore.unity.com/packages/tools/gui/in-game-debug-console-68068 
        private GameObject myDebugConsole;  // reference to InGameDebugConsole 
        private bool screenFull = true;     // full screen 

        [HideInInspector]
        public int  startLog   = 0;        // count startups of app 

        [ContextMenu("Show Help Info in Console")]
        public void WriteHelpMsg()
        {
            ShowInfos(); 
        }

        // ---------------------------------------------------------
        // ---------------------------------------------------------

        void Start()
        {
            Debug.Log("##### Init ApplicationManager.cs >> c..Cursor, !..inGameDebugger f..full/window");
            #region AppMngr Init Area
            if (Application.systemLanguage == SystemLanguage.German)
                Debug.Log("##### System Langage: " + Application.systemLanguage + " -- Platform: " + Application.platform +
                    " -- Sys: " + SystemInfo.operatingSystem);
            else
                Debug.LogWarning("##### System Language (NOT GERMAY): " + Application.systemLanguage + " -- Platform: " + Application.platform +
                    " -- Sys: " + SystemInfo.operatingSystem);

            DontDestroyOnLoad(this.gameObject);
            if (hideCursor) Cursor.visible = false;
            myDebugConsole = GameObject.Find("IngameDebugConsole");
            if (hideConsole) myDebugConsole.SetActive(false);

            IncStartLog(); // log StartUp in PlayerPrefs

            // set and show screen infos
            if (screenLandscape)
                Screen.SetResolution(resX, resY, true);  // changed 17/4/2019 - kimus other smaller screen
            else
              Screen.SetResolution(resY, resX, true);
                // Screen.SetResolution(768, 1366, true);
                // Screen.SetResolution(1080, 1920, true);

                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            Debug.Log("##### Screen Info >> " + Screen.width + " x " + Screen.height + " -- Orient: " + Screen.orientation +
            " -- MonitorRes: " + Screen.currentResolution + " -- FullScr: " + Screen.fullScreen);
            Debug.Log("##### ==NIS============================================SOP=StartOPrg==");

            // AppInit END ###################################################################
            #endregion
        }

        void Update()
        {
            #region AppMngr Update Area

            // ###################################################################
            // ## Application Manager - Update
            //
            // Keys for Debugging 
            //
            // f full screen to window toggle - default: full screen
            // ! inGame DebugConsole - default: not shown
            // c Cursor
            // r reset playerPrefs (delete) 
            // i..infos
            // 

            if (Input.GetKey("escape")) Application.Quit();

            // https://docs.unity3d.com/ScriptReference/PlayerPrefs.DeleteAll.html
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown("1"))
            {
                myDebugConsole.SetActive(!myDebugConsole.activeSelf);  // ! .. show Console
                Cursor.visible = myDebugConsole.activeSelf; // also turn cursor on or off 
            }

            if (Input.GetKeyDown("f")) // f full toggle
            {
                if (screenFull)
                    Screen.fullScreenMode = FullScreenMode.Windowed;
                else
                    Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                screenFull = !screenFull;
            }

            if (Input.GetKeyDown("c")) Cursor.visible = !Cursor.visible; // add startLog
            if (Input.GetKeyDown("h")) ShowInfos(); // show help
            if (Input.GetKeyDown("r")) ResetPlayerPrefs(); // init all PlayerPrefs

            // ## Application Manager: Update code end
            // ###################################################################
            #endregion
        }


        #region AppMngr methods definition 

        // ###################################################################
        // ## Application Manager: methods start

        void IncStartLog()
        {
            // https://docs.unity3d.com/ScriptReference/PlayerPrefs.DeleteAll.html
            int startLog = PlayerPrefs.GetInt("startLog", 0);
            PlayerPrefs.SetInt("startLog", ++startLog);

            Debug.Log("##### ApplicationManager: startLog Counter (prefab) " + startLog + " [regedit: " + Application.companyName + "]");
        }

        void ResetPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.SetInt("startLog", startLog);

            startLog = PlayerPrefs.GetInt("startLog", 0);
            Debug.Log("##### ApplicationManager [r]: ResetPlayerPrefs (prefab) StartLog>" + startLog);
        }

        void ShowInfos()
        {
            Debug.Log("##### ********************************************************************************");
            Debug.Log("##### ================================================================================");
            Debug.Log("##### INFO AppMngr >> h   ... HelpScreen NOW       f ... FullScreen/Window            ");
            Debug.Log("#####              >> c   ... Cursor ON/OFF        r ... ResetPrefabs                 ");
            Debug.Log("#####              >> esc ... End Program                                             ");
            Debug.Log("#####                                                                                 ");

            if (Application.systemLanguage == SystemLanguage.German)
                Debug.Log("##### System Langage: " + Application.systemLanguage + " -- Platform: "
                    + Application.platform + " -- Sys: " + SystemInfo.operatingSystem);
            else
                Debug.LogWarning("##### System Language (NOT GERMAY): " + Application.systemLanguage
                     + " -- Platform: " + Application.platform + " -- Sys: " + SystemInfo.operatingSystem);

            int startLog = PlayerPrefs.GetInt("startLog", 0);
            Debug.Log("##### ApplicationManager: startLog Counter (prefab) " + startLog + " [regedit: " + Application.companyName + "]");
            Debug.Log("##### Screen Info >> " + Screen.width + " x " + Screen.height + " -- Orient: " + Screen.orientation +

               " -- MonitorRes: " + Screen.currentResolution + " -- FullScr: " + Screen.fullScreen);
            Debug.Log("##### Info END                                                                        ");
            Debug.Log("##### ################################################################################");
            Debug.Log("##### ===========================================================================NIS==");

        }

        // ## Application Manager: methods end
        // ###################################################################

        #endregion
    }
}
