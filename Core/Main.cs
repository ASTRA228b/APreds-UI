using GorillaLocomotion;
using GorillaLocomotion.Climbing;
using UnityEngine;
using UnityEngine.InputSystem;

namespace APreds.Core;

public class Main : MonoBehaviour
{
    private GameObject? LT;
    private GameObject? RT;
    private float PredSrength = 0f; 
    private bool IsPredOn = false;
    private bool lastPredState = false;
    private bool IsOpen = false;
    private Texture2D? WTex, BBackground, STex, SThumbTex;
    private GUIStyle? WStyle, BStyle, SStyle, STStyle;
    private Color WColor = new Color(0.1f, 0.1f, 0.1f, 1f);
    private Color BColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    private Color SColor = new Color(0.15f, 0.15f, 0.15f, 1f);
    private Color STColor = new Color(0.0f, 0.6f, 1f, 1f);
    private bool StylesLoaded = false;
    private Rect Window = new Rect(155, 155, 360, 460);

    private void OnGUI()
    {
        if (!StylesLoaded)
        {
            INIT();
            StylesLoaded = true;
        }
        if (IsOpen)
        {
            Window = GUILayout.Window(676998, Window, UIM, "APreds UI", WStyle);
        }
    }

    private void Update()
    {
        if (IsPredOn != lastPredState)
        {
            if (IsPredOn) EnablePreds();
            else DisablePreds();

            lastPredState = IsPredOn;
        }

        if (IsPredOn)
            MakePreds();

        if (Keyboard.current.uKey.wasPressedThisFrame)
            IsOpen = !IsOpen;
    }

    private void UIM(int id)
    {
        MMod();
        GUILayout.Space(5f);
        if (GUILayout.Button("Close", BStyle))
        {
            IsOpen = !IsOpen;
        }
        GUI.DragWindow();
    }

    private void MMod()
    {
        GUILayout.Label("Enable Preds:");
        IsPredOn = GUILayout.Toggle(IsPredOn, "Enable Predictions");
        GUILayout.Space(5f);
        GUILayout.Label("Set Preds Strength");
        PredSrength = GUILayout.HorizontalSlider(PredSrength, 0.001f, 0.2f, SStyle, STStyle);
        GUILayout.Label($"Strength set to {PredSrength:F3}");
        GUILayout.Space(5f);
        GUILayout.Label("Presets:");
        if (GUILayout.Button("Max", BStyle))
            PredSrength = 0.2f;
        if (GUILayout.Button("Random Setting", BStyle))
            PredSrength = UnityEngine.Random.Range(0.001f, 0.2f);
        if (GUILayout.Button("Reset", BStyle))
            PredSrength = 0.001f;
    }

    private void EnablePreds()
    {
        if (LT != null || RT != null)
            return;

        LT = GameObject.CreatePrimitive(PrimitiveType.Cube);
        LT.GetComponent<BoxCollider>().Obliterate();
        LT.GetComponent<Rigidbody>().Obliterate();
        LT.GetComponent<Renderer>().enabled = false;
        LT.AddComponent<GorillaVelocityTracker>();

        RT = GameObject.CreatePrimitive(PrimitiveType.Cube);
        RT.GetComponent<BoxCollider>().Obliterate();
        RT.GetComponent<Rigidbody>().Obliterate();
        RT.GetComponent<Renderer>().enabled = false;
        RT.AddComponent<GorillaVelocityTracker>();
    }

    private void DisablePreds()
    {
        if (LT != null)
        {
            Destroy(LT);
            LT = null;
        }

        if (RT != null)
        {
            Destroy(RT);
            RT = null;
        }
    }


    private void MakePreds()
    {
        if (LT == null || RT == null)
            return;
        if (GTPlayer.Instance == null || GorillaTagger.Instance == null)
            return;
        if (GorillaTagger.Instance.leftHandTransform == null ||
            GorillaTagger.Instance.rightHandTransform == null ||
            GorillaTagger.Instance.headCollider == null)
            return;
        Transform leftHand = GorillaTagger.Instance.leftHandTransform;
        Transform rightHand = GorillaTagger.Instance.rightHandTransform;
        Transform head = GorillaTagger.Instance.headCollider.transform;
        LT.transform.position = leftHand.position;
        RT.transform.position = rightHand.position;
        GorillaVelocityTracker leftTracker = LT.GetComponent<GorillaVelocityTracker>();
        GorillaVelocityTracker rightTracker = RT.GetComponent<GorillaVelocityTracker>();
        if (leftTracker == null || rightTracker == null)
            return;
        Vector3 leftVel = leftTracker.GetAverageVelocity(true, 1f);
        Vector3 rightVel = rightTracker.GetAverageVelocity(true, 1f);
        leftVel = Vector3.ClampMagnitude(leftVel, 5f);
        rightVel = Vector3.ClampMagnitude(rightVel, 5f);
        float movementThreshold = 0.1f;
        bool leftMoving = leftVel.magnitude > movementThreshold;
        bool rightMoving = rightVel.magnitude > movementThreshold;
        float smoothness = 0.2f;
        float maxArmLength = 1.5f;
        if (leftMoving)
        {
            Vector3 target = leftHand.position + leftVel * PredSrength;
            leftHand.position = Vector3.Lerp(leftHand.position, target, smoothness);
        }
        if (rightMoving)
        {
            Vector3 target = rightHand.position + rightVel * PredSrength;
            rightHand.position = Vector3.Lerp(rightHand.position, target, smoothness);
        }
        leftHand.position = Vector3.ClampMagnitude(leftHand.position - head.position, maxArmLength) + head.position;
        rightHand.position = Vector3.ClampMagnitude(rightHand.position - head.position, maxArmLength) + head.position;
    }


    private void INIT()
    {
        WTex = MakeTexture(1, 1, WColor);
        BBackground = MakeTexture(1, 1, BColor);
        STex = MakeTexture(1, 1, SColor);
        SThumbTex = MakeTexture(1, 1, STColor);

        WStyle = new GUIStyle(GUI.skin.window);
        WStyle.normal.background = WTex;
        WStyle.hover.background = WTex;
        WStyle.active.background = WTex;
        WStyle.focused.background = WTex;
        WStyle.onActive.background = WTex;
        WStyle.onNormal.background = WTex;
        WStyle.onFocused.background = WTex;
        WStyle.normal.textColor = Color.white;
        WStyle.fontStyle = FontStyle.Normal;

        BStyle = new GUIStyle(GUI.skin.button);
        BStyle.normal.background = BBackground;
        BStyle.active.background = BBackground;
        BStyle.hover.background = BBackground;
        BStyle.focused.background = BBackground;
        BStyle.onHover.background = BBackground;
        BStyle.onNormal.background = BBackground;
        BStyle.onActive.background = BBackground;
        BStyle.onFocused.background = BBackground;
        BStyle.normal.textColor = Color.white;
        BStyle.hover.textColor = Color.blue;
        BStyle.active.textColor = Color.red;
        BStyle.focused.textColor = Color.white;
        BStyle.onNormal.textColor = Color.blue;
        BStyle.onHover.textColor = Color.blue;
        BStyle.onActive.textColor = Color.blue;
        BStyle.onFocused.textColor = Color.blue;

        SStyle = new GUIStyle(GUI.skin.horizontalSlider);
        STStyle = new GUIStyle(GUI.skin.horizontalSliderThumb);
        SStyle.normal.background = STex;
        SStyle.active.background = STex;
        SStyle.hover.background = STex;
        STStyle.normal.background = SThumbTex;
        STStyle.active.background = SThumbTex;
        STStyle.hover.background = SThumbTex;
    }

    private Texture2D MakeTexture(int WW, int HH, Color CC)
    {
        Texture2D value = new Texture2D(WW, HH);
        value.SetPixel(0, 0, CC);
        value.Apply();
        return value;
    }
}