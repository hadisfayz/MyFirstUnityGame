using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using Uduino;
using DMT;
using UnityEngine.Video;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(VideoPlayer))]

public class GameLogic : MonoBehaviour
{
    private UduinoManager u;
    private int counter = 0;

    public enum GameLanguage { Deutsch, Englisch };
    public GameLanguage myGameLanguage;

    private enum GameStates { Start, Video };
    private GameStates myGameState;

    private GameObject myInfoCanvas;

    // Audio
    public AudioClip[] audioClips;
    private AudioSource[] myAudioPlayers;
    private float myCurrentAudioLength = 0;

    // Video
    public VideoClip[] videoClips;
    private VideoPlayer[] myVideoPlayers;
    private double myCurrentVideoLength = 0;
    private bool videoPlayerToggle = false;

    // Timer Tools - for timeout
    private float timeStart;
    private float timeUser;
    public int timeOut  = 999; // time outTime - if no interaction

    public TextMeshProUGUI infoText;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI timerText;

    private DMT.Config.Config config;

    // Events from GameLogic

    public static event Action<string> GameInfoAction;

    // #####################################################################
    // #####################################################################

    void Start()
    {
        DontDestroyOnLoad(this.gameObject);

        config = GameObject.Find("Config").GetComponent<DMT.Config.Config>();
        Debug.Log("Config.XML: " + config.GetConfigAsText());

        Debug.Log("Init UDUINO");
        u = UduinoManager.Instance;

        myInfoCanvas = GameObject.Find("InfoCanvas");
        myInfoCanvas.SetActive(!config.bHideScreenInfo);

        myGameState = GameStates.Start;
        initGame();
        myGameState = GameStates.Video;

        timeStart = Time.time;
        timeUser = 0.0f;

        // Media Tools
        myAudioPlayers = this.GetComponents<AudioSource>();
        myVideoPlayers = this.GetComponents<VideoPlayer>();

        myVideoPlayers[0].prepareCompleted += Prepared;
        myVideoPlayers[1].prepareCompleted += Prepared;
        myVideoPlayers[0].Prepare();
        myVideoPlayers[1].Prepare();

        TimeUserInteract();
        Debug.Log("GAME-LOGIC Started at " + timeStart.ToString("00.00"));
        StartAudio(0);

        InvokeRepeating("UpdateTimer", 5, 1);
    }

    void Awake()
    {
        UduinoManager.Instance.OnDataReceived += OnDataReceived; //Create the Delegate
        UduinoManager.Instance.alwaysRead = true; // This value should be On By Default
        TimeUserInteract();
    }

    // #####################################################################

    void Update()
    {
        // good - nothing to do
    }

    void UpdateTimer()
    {
        // TIMER Checks > TimeOuts, Loops, etc
        // #############################################

        if (timerText != null)
        {
            int mySec = (int)getTimeUser;
            timerText.text = mySec.ToString();

            if (mySec > timeOut)  // TimeOut no User interaction -> go to Start
            {

                myGameState = GameStates.Start;
                TimeUserInteract(); // simulate interaction, set UserTime back to 0
            }
        }

        showStateInfo();
    }

    // #####################################################################
    // #####################################################################
    // Main Game Logic

    void OnKey1_Simulator()  // S1 Simulation from Arduino - see PlayerInput Script
    {
        OnDataReceived("S1");
    }

    void OnKeyDeutsch()
    {
        OnDataReceived("D1"); // button simulation D1 (deutsch pressed)
    }

    void OnKeyEnglisch()
    {
        OnDataReceived("E1"); // button simulation E1 (englisch pressed)
    }

    void OnMediaJump() // jump to end of media file (audio or video)
    {
        if (myAudioPlayers[0].isPlaying) JumpAudio(5.0F);
        if ((myVideoPlayers[0].isPlaying) || (myVideoPlayers[1].isPlaying)) JumpVideo(5.0F);
    }

    void OnTab_ToggleInfoCanvas()
    {
        bool stateInfoCanvas = myInfoCanvas.activeSelf;
        Debug.Log("InfoCanvas State old:" + stateInfoCanvas + " to " + !stateInfoCanvas);
        myInfoCanvas.SetActive(!stateInfoCanvas);
    }

    // ##########################################################################
    // ##### ARDUINO EVENT Handling
    // ##### MAIN INTERACTION LOOP
    // #####
    // ##########################################################################

    void OnDataReceived(string data, UduinoDevice deviceName = null)
    {

        Debug.Log("UDUINO Got: " + data + " from " + (deviceName == null ? "Simulator" : deviceName.name) + " >> " + counter);
        TimeUserInteract(); // Interaction > reset user timer

        // if (GameInfoAction != null) GameInfoAction(data);
        GameInfoAction?.Invoke(data);

        if (data == "S1") // wind is made
        {
            if (myGameState == GameStates.Video) // only now react on wind
            {

            }
        }

        if (data == "D1") myGameLanguage = GameLanguage.Deutsch;
        if (data == "E1") myGameLanguage = GameLanguage.Englisch;

        showStateInfo();
    }

    // #################
    // Uduino TOOLS
    // #################

    private void initGame()
    {
        // nothing special
    }

    // #################
    // Barcode Data GOT
    // #################

    public void playBarcodeVideo(string codeing)
    {
        int video = 0;
        if (int.TryParse(codeing[5].ToString(), out video))
        {
            if ((0 <= video) && (video < videoClips.Length)) StartVideo(video, false);
            Debug.Log("##### Barcode: " + codeing + " V>> " + codeing[5]);
        }
        else
        {
            Debug.Log("!!!!! ERROR Barcode: " + codeing + " V>> " + codeing[5]);
        }

        TimeUserInteract();
    }

    // ##########################################################################
    // ##### Info and Debugging Tools
    // ##########################################################################

    private String showStateInfo(String moreInfo = "")
    {
        String infoString = "STATE: [" + (int)myGameState + " <> " + myGameState.ToString() + "] ";
        byte currentVPlayer = (byte)(videoPlayerToggle ? 1 : 0);

        infoString += "Video: " + currentVPlayer;
        infoString +=  myGameLanguage.ToString();
        if (statusText != null) statusText.text = infoString;

        // infoString += "  Time:" + getTimeUser.ToString("00.00");
        // infoString += "  TotalTime:" + getTimeTotal.ToString("00.00");

        return infoString;
    }

    // ##########################################################################
    // ##### Time Tools & Reset
    // ##########################################################################

    private void TimeUserInteract()
    { // for timeout, interaction happened
        timeUser = Time.time;
    }

    private float getTimeUser
    {
        get { return Time.time - timeUser; }
    }

    private float getTimeTotal
    {
        get { return Time.time - timeStart; }
    }

    // ##########################################################################
    // ##### VIDEO Tools
    // ##### with one VideoPlayer
    // ##########################################################################
    // https://docs.unity3d.com/ScriptReference/Video.VideoPlayer.html

    void JumpVideo(float jumpTime = 5)  // jump to next audio
    {
        CancelInvoke("VideoReady");
        byte currentVPlayer = (byte)(videoPlayerToggle ? 1 : 0);
        Invoke("VideoReady", jumpTime);

        Debug.Log("VVVVV Video JUMP: V>" + currentVPlayer + " Length: from " + myVideoPlayers[currentVPlayer].time + " to " + (myCurrentVideoLength - jumpTime));
        myVideoPlayers[currentVPlayer].time = myCurrentVideoLength - jumpTime;
    }

    void StartVideo(int myVideoFile, bool looping = true)
    {
        videoPlayerToggle = !videoPlayerToggle;
        byte currentVPlayer = (byte)(videoPlayerToggle ? 1 : 0);

        myVideoPlayers[1 - currentVPlayer].Pause(); // stop old player
        myVideoPlayers[1 - currentVPlayer].time = 0;

        if (currentVPlayer == 0) // 1 vorne - unsichtbar machen
            myVideoPlayers[1].enabled = false;
        else
            myVideoPlayers[1].enabled = true;

        // myVideoPlayers[0].targetCameraAlpha = 1.0f; // hinten immer sichtbar

        myVideoPlayers[currentVPlayer].clip = videoClips[myVideoFile];
        myVideoPlayers[currentVPlayer].Prepare();

        myVideoPlayers[currentVPlayer].isLooping = looping;
        myCurrentVideoLength = myVideoPlayers[currentVPlayer].clip.length;
        myVideoPlayers[currentVPlayer].time = 0;
        myVideoPlayers[currentVPlayer].Play();

        CancelInvoke("VideoReady"); // delete old invokes

        if (!looping) Invoke("VideoReady", (float)myCurrentVideoLength);

        showStateInfo();
        Debug.Log("VVVVV Video stated: >>>" + myGameState + "<<< Length: " + myCurrentVideoLength + " -- File:" + myVideoFile);
    }

    private void VideoReady()
    {
        Debug.Log("VVVVV Video over");
    }

    void Prepared(UnityEngine.Video.VideoPlayer vPlayer)
    {
        Debug.Log("VVVVV Video prepeard: " + vPlayer.clip.name);
        // vPlayer.Play();
    }

    // ##########################################################################
    // ##### AUDIO Tools
    // ##### with two Audiosources
    // ##########################################################################

    void JumpAudio(float jumpTime = 5)  // jump to next audio
    {
        CancelInvoke("AudioReady");
        Invoke("AudioReady", jumpTime);
        myAudioPlayers[0].time = myCurrentAudioLength - jumpTime;
    }

    void StartAudio(int myAudioFile, float playDelay = 0)
    {

        if (playDelay == 0)
        {
            myAudioPlayers[0].Stop();
            myAudioPlayers[1].Stop(); // player 0 stops also 1 (delayed one)

            myAudioPlayers[0].clip = audioClips[myAudioFile];
            myAudioPlayers[0].loop = false;
            myCurrentAudioLength = myAudioPlayers[0].clip.length;
            myAudioPlayers[0].time = 0;
            myAudioPlayers[0].Play();
        }
        else
        {
            // special source for 2 clips at nearly the same time
            myAudioPlayers[1].Stop();
            myAudioPlayers[1].clip = audioClips[myAudioFile];
            myAudioPlayers[1].loop = false;
            myCurrentAudioLength = myAudioPlayers[1].clip.length;
            myAudioPlayers[1].time = 0;
            myAudioPlayers[1].PlayDelayed(playDelay);
        }

        CancelInvoke("AudioReady"); // delete old invokes
        Invoke("AudioReady", myCurrentAudioLength + playDelay);
        showStateInfo();
        Debug.Log("AAAAA Audio stated: >>>" + myGameState + "<<< Length: " + myCurrentAudioLength + " delay:" + playDelay + " -- File:" + myAudioFile);
    }

    private void AudioReady()
    {
        Debug.Log("AAAAA Audio over");
    }

    // ##########################################################################
    // ##########################################################################

}

// EOF                                                                    NIS
// ##########################################################################

