using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using I2.Loc;
using Plawius;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Debug = UnityEngine.Debug;

[CreateAssetMenu(fileName = "Control Mapping", menuName = "Massive Monster/Control Mapping", order = 1)]
public class ControlMappings : ScriptableObject
{
    private const string kPCFontPath = "Assets/Plawius-ControllerIcons/Fonts/PC-Filled SDF.asset";

    private const string kSwitchFontPath = "Assets/Plawius-ControllerIcons/Fonts/Switch-Filled SDF.asset";

    private const string kPlaystationFontPath = "Assets/Plawius-ControllerIcons/Fonts/PS4-Filled SDF.asset";

    private const string kPS5FontPath = "Assets/Plawius-ControllerIcons/Fonts/PS5-Filled SDF.asset";

    private const string kXboxFontPath = "Assets/Plawius-ControllerIcons/Fonts/Xbox-Filled SDF.asset";

    [Header("Fonts")]
    [SerializeField]
    private ControllerIcons _fontController;

    private static TMP_FontAsset _PCFontAsset;

    private static TMP_FontAsset _switchFontAsset;

    private static TMP_FontAsset _ps4FontAsset;

    private static TMP_FontAsset _ps5FontAsset;

    private static TMP_FontAsset _xboxFontAsset;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LoadPlatformFonts()
    {
        DoLoadPlatformFonts();
    }

    private static async void DoLoadPlatformFonts()
    {
        _PCFontAsset = await LoadFontAsset("Assets/Plawius-ControllerIcons/Fonts/PC-Filled SDF.asset");
        _switchFontAsset = await LoadFontAsset("Assets/Plawius-ControllerIcons/Fonts/Switch-Filled SDF.asset");
        _ps4FontAsset = await LoadFontAsset("Assets/Plawius-ControllerIcons/Fonts/PS4-Filled SDF.asset");
        _ps5FontAsset = await LoadFontAsset("Assets/Plawius-ControllerIcons/Fonts/PS5-Filled SDF.asset");
        _xboxFontAsset = await LoadFontAsset("Assets/Plawius-ControllerIcons/Fonts/Xbox-Filled SDF.asset");
    }

    private static async Task<TMP_FontAsset> LoadFontAsset(string path)
    {
        Debug.Log(("Load font asset at path: " + path).Colour(Color.yellow));
        AsyncOperationHandle<TMP_FontAsset> asyncOperation = Addressables.LoadAssetAsync<TMP_FontAsset>(path);
        await asyncOperation.Task;
        if (asyncOperation.Result != null)
        {
            return asyncOperation.Result;
        }
        Debug.Log("Font failed to load!".Colour(Color.red));
        return null;
    }

    public TMP_FontAsset GetFontForPlatform(Platform platform)
    {
        switch (platform)
        {
            case Platform.Undefined:
            case Platform.PC:
                return _PCFontAsset;
            case Platform.Switch:
                return _switchFontAsset;
            case Platform.PS4:
                return _ps4FontAsset;
            case Platform.PS5:
                return _ps5FontAsset;
            case Platform.XboxOne:
            case Platform.XboxSeries:
                return _xboxFontAsset;
            default:
                return null;
        }
    }

    public static string GetControllerCodeFromID(int id)
    {
        switch (id)
        {
            case 4:
                return "\ue900";
            case 5:
                return "\ue901";
            case 7:
                return "\ue902";
            case 8:
                return "\ue903";
            case 10:
                return "\ue915";
            case 11:
                return "\ue916";
            case 12:
                return "\ue917";
            case 13:
                return "\ue918";
            case 14:
                return "\ue919";
            case 15:
                return "\ue91a";
            case 16:
                return "\ue919";
            case 23:
                return "\ue904";
            case 17:
                return "\ue909";
            case 3:
            case 24:
                return "\ue90f";
            case 18:
                return "\ue914";
            case 19:
                return "\ue90b";
            case 21:
                return "\ue90d";
            case 20:
                return "\ue90c";
            case 22:
                return "\ue90e";
            default:
                return id.ToString();
        }
    }

    public static string GetKeyboardCode(KeyboardKeyCode keyCode, out bool isSpecialCharacter)
    {
        isSpecialCharacter = true;
        switch (keyCode)
        {
            case KeyboardKeyCode.UpArrow:
                return "\ue90b";
            case KeyboardKeyCode.DownArrow:
                return "\ue90d";
            case KeyboardKeyCode.LeftArrow:
                return "\ue90e";
            case KeyboardKeyCode.RightArrow:
                return "\ue90c";
            case KeyboardKeyCode.Space:
                return "\ue921";
            case KeyboardKeyCode.Return:
            case KeyboardKeyCode.KeypadEnter:
                return "\ue900";
            case KeyboardKeyCode.RightShift:
            case KeyboardKeyCode.LeftShift:
                return "\ue902";
            case KeyboardKeyCode.Escape:
                return "\ue91a";
            case KeyboardKeyCode.RightControl:
            case KeyboardKeyCode.LeftControl:
                return "\ue916";
            case KeyboardKeyCode.RightAlt:
            case KeyboardKeyCode.LeftAlt:
                return "\ue918";
            case KeyboardKeyCode.Backspace:
                return "\ue901";
            case KeyboardKeyCode.LeftBracket:
                return "\ue915";
            case KeyboardKeyCode.RightBracket:
                return "\ue917";
            case KeyboardKeyCode.Tab:
                return "\ue903";
            default:
                isSpecialCharacter = false;
                switch (keyCode)
                {
                    case KeyboardKeyCode.PageDown:
                        return "PgDn";
                    case KeyboardKeyCode.PageUp:
                        return "PgUp";
                    case KeyboardKeyCode.Backslash:
                        return "\\";
                    case KeyboardKeyCode.Alpha0:
                    case KeyboardKeyCode.Keypad0:
                        return "0";
                    case KeyboardKeyCode.Alpha1:
                    case KeyboardKeyCode.Keypad1:
                        return "1";
                    case KeyboardKeyCode.Alpha2:
                    case KeyboardKeyCode.Keypad2:
                        return "2";
                    case KeyboardKeyCode.Alpha3:
                    case KeyboardKeyCode.Keypad3:
                        return "3";
                    case KeyboardKeyCode.Alpha4:
                    case KeyboardKeyCode.Keypad4:
                        return "4";
                    case KeyboardKeyCode.Alpha5:
                    case KeyboardKeyCode.Keypad5:
                        return "5";
                    case KeyboardKeyCode.Alpha6:
                    case KeyboardKeyCode.Keypad6:
                        return "6";
                    case KeyboardKeyCode.Alpha7:
                    case KeyboardKeyCode.Keypad7:
                        return "7";
                    case KeyboardKeyCode.Alpha8:
                    case KeyboardKeyCode.Keypad8:
                        return "8";
                    case KeyboardKeyCode.Alpha9:
                    case KeyboardKeyCode.Keypad9:
                        return "9";
                    case KeyboardKeyCode.Numlock:
                        return "NumLk";
                    case KeyboardKeyCode.Slash:
                    case KeyboardKeyCode.KeypadDivide:
                        return "/";
                    case KeyboardKeyCode.LeftBracket:
                        return "[";
                    case KeyboardKeyCode.RightBracket:
                        return "]";
                    case KeyboardKeyCode.Equals:
                    case KeyboardKeyCode.KeypadEquals:
                        return "=";
                    case KeyboardKeyCode.KeypadMultiply:
                        return "*";
                    case KeyboardKeyCode.Minus:
                    case KeyboardKeyCode.KeypadMinus:
                        return "-";
                    case KeyboardKeyCode.KeypadPlus:
                        return "+";
                    case KeyboardKeyCode.Period:
                    case KeyboardKeyCode.KeypadPeriod:
                        return ".";
                    case KeyboardKeyCode.Print:
                        return "PrtSc";
                    case KeyboardKeyCode.ScrollLock:
                        return "ScrLk";
                    case KeyboardKeyCode.Pause:
                        return "Pause";
                    case KeyboardKeyCode.DoubleQuote:
                        return "\"";
                    case KeyboardKeyCode.Colon:
                        return ":";
                    case KeyboardKeyCode.Semicolon:
                        return ";";
                    case KeyboardKeyCode.BackQuote:
                        return "`";
                    case KeyboardKeyCode.Quote:
                        return "'";
                    case KeyboardKeyCode.Comma:
                        return ",";
                    default:
                        return keyCode.ToString();
                }
        }
    }

    public static string GetMouseCode(MouseInputElement mouseInputElement, Pole axisContribution)
    {
        switch (mouseInputElement)
        {
            case MouseInputElement.AxisX:
                if (axisContribution == Pole.Positive)
                {
                    return "\ue911";
                }
                return "\ue913";
            case MouseInputElement.AxisY:
                if (axisContribution == Pole.Positive)
                {
                    return "\ue910";
                }
                return "\ue912";
            case MouseInputElement.Axis3:
                if (axisContribution == Pole.Positive)
                {
                    return "\ue91d";
                }
                return "\ue91f";
            case MouseInputElement.Button0:
                return "\ue909";
            case MouseInputElement.Button1:
                return "\ue914";
            case MouseInputElement.Button2:
                return "\ue919";
            case MouseInputElement.Button3:
                return "\ue920";
            case MouseInputElement.Button4:
                return "\ue91e";
            default:
                return mouseInputElement.ToString();
        }
    }

    
        public static string LocForAction(int action)
        {
            switch (action)
            {
                case 1:
                    return "Move Horizontal";
                case 0:
                    return "Move Vertical";
                case 2:
                    return ScriptLocalization.UI_Settings_Controls.Attack;
                case 94:
                    return ScriptLocalization.UI_Settings_Controls_Gameplay.HeavyAttack;
                case 9:
                    return ScriptLocalization.UI_Settings_Controls.Interact;
                case 68:
                    return ScriptLocalization.UI_Settings_Controls_Gameplay.Interact2;
                case 67:
                    return ScriptLocalization.UI_Settings_Controls_Gameplay.Interact3;
                case 66:
                    return ScriptLocalization.UI_Settings_Controls_Gameplay.Interact4;
                case 13:
                    return ScriptLocalization.UI_Settings_Controls.Shoot;
                case 16:
                    return ScriptLocalization.UI_Settings_Controls.Dodge;
                case 73:
                    return ScriptLocalization.UI_Settings_Controls_Gameplay.CancelFishing;
                case 17:
                    return ScriptLocalization.UI_Settings_Controls.Pause;
                case 23:
                    return ScriptLocalization.Interactions.ReturnToBase;
                case 26:
                    return ScriptLocalization.UI_Settings_Controls.Menu;
                case 31:
                    return ScriptLocalization.UI_PauseScreen_Quests.TrackQuest;
                case 58:
                    return ScriptLocalization.UI_Settings_Controls_Gameplay.Bleat;
                case 59:
                    return ScriptLocalization.UI_Settings_Controls.Meditate;
                case 64:
                    return ScriptLocalization.UI_Settings_Controls_Gameplay.AdvanceDialogue;
                case 69:
                    return ScriptLocalization.UI_Settings_Controls_Gameplay.PlaceMoveUpgrade;
                case 70:
                    return ScriptLocalization.UI_Settings_Controls_Gameplay.RemoveFlip;
                case 71:
                    return ScriptLocalization.UI_Settings_Controls.UseRelic;
                case 93:
                    return ScriptLocalization.UI_Settings_Controls_Gameplay.FleeceAbility;
                case 35:
                    return "UI Horizontal";
                case 34:
                    return "UI Vertical";
                case 38:
                    return ScriptLocalization.UI_Generic.Accept;
                case 39:
                    return ScriptLocalization.Interactions.Cancel;
                case 43:
                    return ScriptLocalization.UI_Settings_Controls_UI.TabLeft;
                case 44:
                    return ScriptLocalization.UI_Settings_Controls_UI.TabRight;
                case 48:
                    return ScriptLocalization.UI_Generic.ApplyChanges;
                case 49:
                    return ScriptLocalization.UI_Generic.ResetAll;
                case 56:
                    return ScriptLocalization.Interactions.Cook;
                case 60:
                    return ScriptLocalization.UI_Settings_Controls.ResetBinding;
                case 65:
                    return ScriptLocalization.UI_Settings_Controls_Bindings.Unbind;
                case 85:
                    return ScriptLocalization.UI_Settings_Controls_PhotoMode.PlaceSticker;
                case 87:
                    return "Scale Sticker";
                case 86:
                    return "Rotate Sticker";
                case 88:
                    return ScriptLocalization.UI_Settings_Controls_PhotoMode.FlipSticker;
                case 89:
                    return ScriptLocalization.UI_Settings_Controls_PhotoMode.UndoSticker;
                case 90:
                    return ScriptLocalization.UI_Settings_Controls_PhotoMode.SavePhoto;
                case 91:
                    return ScriptLocalization.UI_Settings_Controls_PhotoMode.ClearStickers;
                case 76:
                    return ScriptLocalization.UI_Settings_Controls_PhotoMode.TakePhoto;
                case 80:
                    return ScriptLocalization.UI_Settings_Controls_PhotoMode.DeletePhoto;
                case 75:
                    return ScriptLocalization.UI_Settings_Controls_PhotoMode.OpenGalleryLocateFolder;
                case 77:
                    return "Camera Height";
                case 83:
                    return "Camera Focus";
                case 84:
                    return "Camera Tilt";
                default:
                    return string.Empty;
            }
        }
    
}