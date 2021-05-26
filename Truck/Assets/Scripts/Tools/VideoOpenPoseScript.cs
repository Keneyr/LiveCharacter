using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoOpenPoseScript : MonoBehaviour
{
    private bool ContollerOpenUpdate = true;
    // Output
    private OpenPose.OPDatum datum;

    // OpenPose settings
    public OpenPose.ProducerType inputType = OpenPose.ProducerType.Video;
    public static string producerString = null;
    public static string folderSaveKeyPoints = null;
    public static string folderSaveImage = null;

    public ulong videoframeStep = 1;
    public int maxPeople = 1;
    public bool
        handEnabled = false,
        faceEnabled = false;
    public Vector2Int
            netResolution = new Vector2Int(-1, 368),
            handResolution = new Vector2Int(368, 368),
            faceResolution = new Vector2Int(368, 368);
    public float renderThreshold = 0.05f;
    // Number of people
    int numberPeople = 1;
    // Frame rate calculation
    private int queueMaxCount = 20;
    private Queue<float> frameTimeQueue = new Queue<float>();
    private float avgFrameRate = 0f;
    private int frameCounter = 0;

    bool setbegin = true;
    public static bool stopOpenPose = false;
    private void Start()
    {
        //videoframeStep = (ulong)VideoSliderController.instance.sliderInterval.value;
        videoframeStep = 1;
    }

    private void Update()
    {
        
        if (producerString == null)
            return;

        if (stopOpenPose)
        {
            //立刻停止openPose
            OpenPose.OPWrapper.CusOnDestroy();
            Console.Log("Video PoseExtract Complete.");
            stopOpenPose = false;
            producerString = null;
        }
        if (producerString != null && setbegin)
        {
            CusStart();
            setbegin = false;
            //producerString = null;
        }
        if(OpenPose.OPWrapper.OPGetOutput(out datum))
        {
            VideoPreprocessing.ExtractVideoMotionPoseDatum(datum);
        }
    }
    public void CusStart()
    {
        OpenPose.OPWrapper.OPRegisterCallbacks();
        OpenPose.OPWrapper.OPEnableDebug(true);
        OpenPose.OPWrapper.OPEnableOutput(true);
        OpenPose.OPWrapper.OPEnableImageOutput(true);

        UserConfigureOpenPose();
        OpenPose.OPWrapper.OPRun();
    }

    private void UserConfigureOpenPose()
    {
        OpenPose.OPWrapper.OPConfigurePose(
                /* poseMode */ OpenPose.PoseMode.Enabled, /* netInputSize */ netResolution, /* outputSize */ null,
                /* keypointScaleMode */ OpenPose.ScaleMode.ZeroToOne,
                /* gpuNumber */ -1, /* gpuNumberStart */ 0, /* scalesNumber */ 1, /* scaleGap */ 0.25f,
                /* renderMode */ OpenPose.RenderMode.Gpu, /* poseModel */ OpenPose.PoseModel.COCO_18,
                /* blendOriginalFrame */ true, /* alphaKeypoint */ 0.6f, /* alphaHeatMap */ 0.7f,
                /* defaultPartToRender */ 0, /* modelFolder */ null,
                /* heatMapTypes */ OpenPose.HeatMapType.None, /* heatMapScaleMode */ OpenPose.ScaleMode.ZeroToOne,
                /* addPartCandidates */ true, /* renderThreshold */ renderThreshold, /* numberPeopleMax */ maxPeople,
                /* maximizePositives */ false, /* fpsMax fps_max */ -1.0,
                /* protoTxtPath */ "", /* caffeModelPath */ "", /* upsamplingRatio */ 0f);

        OpenPose.OPWrapper.OPConfigureHand(
            /* enable */ handEnabled, /* detector */ OpenPose.Detector.Body, /* netInputSize */ handResolution,
            /* scalesNumber */ 1, /* scaleRange */ 0.4f, /* renderMode */ OpenPose.RenderMode.Auto,
            /* alphaKeypoint */ 0.6f, /* alphaHeatMap */ 0.7f, /* renderThreshold */ 0.2f);

        OpenPose.OPWrapper.OPConfigureFace(
            /* enable */ faceEnabled, /* detector */ OpenPose.Detector.Body,
            /* netInputSize */ faceResolution, /* renderMode */ OpenPose.RenderMode.Auto,
            /* alphaKeypoint */ 0.6f, /* alphaHeatMap */ 0.7f, /* renderThreshold */ 0.4f);

        OpenPose.OPWrapper.OPConfigureExtra(
            /* reconstruct3d */ false, /* minViews3d */ -1, /* identification */ false, /* tracking */ -1,
            /* ikThreads */ 0);

        OpenPose.OPWrapper.OPConfigureInput(
            /* producerType */ inputType, /* producerString */ producerString,
            /* frameFirst */ 0, /* frameStep */ videoframeStep, /* frameLast */ ulong.MaxValue,
            /* realTimeProcessing */ false, /* frameFlip */ false,
            /* frameRotate */ 0, /* framesRepeat */ false,
            /* cameraResolution */ null, /* cameraParameterPath */ null,
            /* undistortImage */ false, /* numberViews */ -1);

        OpenPose.OPWrapper.OPConfigureOutput(
            /* verbose */ -1.0, /* writeKeypoint */ folderSaveKeyPoints, /* writeKeypointFormat */ OpenPose.DataFormat.Json,
            /* writeJson */ "", /* writeCocoJson */ "", /* writeCocoJsonVariants */ 1,
            /* writeCocoJsonVariant */ 1, /* writeImages */ folderSaveImage, /* writeImagesFormat */ "png",
            /* writeVideo */ "", /* writeVideoFps */ -1.0, /* writeVideoWithAudio */ false,
            /* writeHeatMaps */ "", /* writeHeatMapsFormat */ "png", /* writeVideo3D */ "",
            /* writeVideoAdam */ "", /* writeBvh */ "", /* udpHost */ "", /* udpPort */ "8051");

        OpenPose.OPWrapper.OPConfigureGui(
            /* displayMode */ OpenPose.DisplayMode.NoDisplay, /* guiVerbose */ true, /* fullScreen */ false);

        OpenPose.OPWrapper.OPConfigureDebugging(
            /* loggingLevel */ OpenPose.Priority.High, /* disableMultiThread */ false, /* profileSpeed */ 1000);
    }
}
