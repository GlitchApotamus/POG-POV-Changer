using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Il2Cpp;
using Il2CppEntities;
using Il2CppGame.Interaction;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;

namespace POGPOVChanger;

public sealed class POGPOVChanger : MelonMod
{
    private const string ModConfigDirectory = "POGPOVChanger";
    private const string ConfigFileName = "config.json";
    private const string DefaultPresetKey = "F5";

    private KeyCode _toggleKey = KeyCode.F5;
    private PlayerController? _thirdPersonController;
    private PlayerInteract? _playerInteract;
    private Transform? _originalInteractionRayOrigin;
    private float _originalInteractionDistance;
    private GameObject? _rayProxy;

    public override void OnInitializeMelon()
    {
        LoggerInstance.Msg("POGPOVChanger has been loaded.");
        LoadConfiguredKey();
        RegisterPogConfig();
        LoggerInstance.Msg($"POV toggle key: {_toggleKey}");
    }

    public override void OnUpdate()
    {
        if (Input.GetKeyDown(_toggleKey))
        {
            TogglePov();
        }

        UpdateThirdPersonInteractionRay();
    }

    public override void OnGUI()
    {
        if (_thirdPersonController == null ||
            _playerInteract == null ||
            _thirdPersonController.CurrentCameraType != PlayerController.CameraType.ThirdPersonCamera)
        {
            return;
        }

        // Always-on aim dot; it turns green while an interactable is selected.
        float centerX = Screen.width / 2f;
        float centerY = Screen.height / 2f;
        Color previous = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.8f);
        GUI.DrawTexture(new Rect(centerX - 3f, centerY - 3f, 6f, 6f), Texture2D.whiteTexture);
        GUI.color = _playerInteract.m_currentSelectedInteractable != null ? new Color(0.3f, 1f, 0.3f, 1f) : Color.white;
        GUI.DrawTexture(new Rect(centerX - 2f, centerY - 2f, 4f, 4f), Texture2D.whiteTexture);
        GUI.color = previous;
    }

    private void TogglePov()
    {
        PlayerController? controller = GetLocalPlayerController();
        if (controller == null)
        {
            LoggerInstance.Warning("No local PlayerController was found for POV switching.");
            return;
        }

        try
        {
            if (controller.CurrentCameraType == PlayerController.CameraType.ThirdPersonCamera)
            {
                controller.SetFirstPersonCamera();
                RestoreInteractionRay();
                return;
            }

            controller.ChangeCamera(PlayerController.CameraType.ThirdPersonCamera, controller.CameraRoot);
            SetThirdPersonInteractionRay(controller);
        }
        catch (Exception ex)
        {
            LoggerInstance.Warning($"Could not switch POV: {ex.Message}");
        }
    }

    private void SetThirdPersonInteractionRay(PlayerController controller)
    {
        EntityPlayer? player = UnityEngine.Object.FindObjectsOfType<EntityPlayer>()
            .FirstOrDefault(candidate => candidate.m_playerController == controller);
        PlayerInteract? playerInteract = player?.PlayerInteract;
        if (playerInteract == null || playerInteract.m_rayOrigin == null)
        {
            LoggerInstance.Warning("Could not update the interaction ray for third-person view.");
            return;
        }

        RestoreInteractionRay();
        _thirdPersonController = controller;
        _playerInteract = playerInteract;
        _originalInteractionRayOrigin = playerInteract.m_rayOrigin;
        _originalInteractionDistance = playerInteract.m_distance;
        UpdateThirdPersonInteractionRay();
    }

    private void UpdateThirdPersonInteractionRay()
    {
        if (_thirdPersonController == null && _playerInteract == null)
        {
            return;
        }

        // The game can leave third person on its own (e.g. ragdoll or death), so undo our changes when that happens.
        if (_thirdPersonController == null ||
            _playerInteract == null ||
            _originalInteractionRayOrigin == null ||
            _thirdPersonController.CurrentCameraType != PlayerController.CameraType.ThirdPersonCamera)
        {
            RestoreInteractionRay();
            return;
        }

        Camera? camera = _thirdPersonController.MainCamera;
        if (camera == null)
        {
            return;
        }

        if (!_playerInteract.m_rayActive)
        {
            _playerInteract.ToggleRay(true);
        }

        // The game's ray origin is the camera, which sits behind the player in third person.
        // Cast from the first-person head pivot instead, along the camera's view direction.
        if (_rayProxy == null)
        {
            _rayProxy = new GameObject("POGPOVChanger_InteractRay");
        }

        Transform proxy = _rayProxy.transform;
        Transform headPivot = _thirdPersonController.CameraRoot;
        proxy.position = headPivot != null ? headPivot.position : _originalInteractionRayOrigin.position;
        proxy.rotation = camera.transform.rotation;
        _playerInteract.m_rayOrigin = proxy;
    }

    public override void OnLateUpdate()
    {
        UpdateThirdPersonInteractionRay();
    }

    private void RestoreInteractionRay()
    {
        if (_playerInteract != null && _originalInteractionRayOrigin != null)
        {
            try
            {
                _playerInteract.m_rayOrigin = _originalInteractionRayOrigin;
                _playerInteract.m_distance = _originalInteractionDistance;
            }
            catch (Exception ex)
            {
                LoggerInstance.Warning($"Could not restore the interaction ray: {ex.Message}");
            }
        }

        if (_rayProxy != null)
        {
            UnityEngine.Object.Destroy(_rayProxy);
            _rayProxy = null;
        }

        _thirdPersonController = null;
        _playerInteract = null;
        _originalInteractionRayOrigin = null;
        _originalInteractionDistance = 0f;
    }

    private static PlayerController? GetLocalPlayerController()
    {
        PlayerController[] players = UnityEngine.Object.FindObjectsOfType<PlayerController>();
        return players.Length == 0 ? null : players[0];
    }

    private void RegisterPogConfig()
    {
        try
        {
            Type? configType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(type => type.FullName == "POGMods.Config.POGConfig" || type.Name == "POGConfig");
            if (configType == null)
            {
                LoggerInstance.Msg("POG Config was not detected; default keybind only.");
                return;
            }

            MethodInfo? registerMethod = configType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(method => method.Name == "Register" && method.GetParameters().Length == 2);
            if (registerMethod == null)
            {
                LoggerInstance.Warning("POG Config register method could not be found.");
                return;
            }

            Type? configEntryType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(type => type.FullName == "POGMods.Config.ConfigEntry" || type.Name == "ConfigEntry");
            Type? keyEntryType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(type => type.FullName == "POGMods.Config.KeyEntry" || type.Name == "KeyEntry");
            if (configEntryType == null || keyEntryType == null)
            {
                LoggerInstance.Warning("POG Config keybind types could not be found.");
                return;
            }

            Func<KeyCode> getter = () => _toggleKey;
            Action<KeyCode> setter = value =>
            {
                _toggleKey = value;
                SaveConfiguredKey();
            };

            ConstructorInfo? entryConstructor = keyEntryType.GetConstructors()
                .FirstOrDefault(constructor =>
                    constructor.GetParameters().Length >= 3 &&
                    constructor.GetParameters()[0].ParameterType == typeof(string) &&
                    constructor.GetParameters()[1].ParameterType == typeof(Func<KeyCode>) &&
                    constructor.GetParameters()[2].ParameterType == typeof(Action<KeyCode>));
            if (entryConstructor == null)
            {
                LoggerInstance.Msg("POG Config keybind entry constructor is unavailable in this version of POG Config.");
                return;
            }

            object keyEntry;
            object[] arguments = entryConstructor.GetParameters().Length >= 4
                ? new object[] { "POV Toggle Key", getter, setter, "Change the POV toggle key" }
                : new object[] { "POV Toggle Key", getter, setter };
            keyEntry = entryConstructor.Invoke(arguments);

            Type entryListType = typeof(List<>).MakeGenericType(configEntryType);
            object entries = Activator.CreateInstance(entryListType)!;
            entryListType.GetMethod("Add")!.Invoke(entries, new[] { keyEntry });

            registerMethod.Invoke(null, new[] { "POG POV Changer", entries });
            LoggerInstance.Msg("POG Config entry registered for custom POV keybind.");
        }
        catch (Exception ex)
        {
            LoggerInstance.Warning($"Could not register the POG Config option: {ex.Message}");
        }
    }

    private void LoadConfiguredKey()
    {
        string configPath = Path.Combine(MelonEnvironment.UserDataDirectory, "ModManager", ModConfigDirectory, ConfigFileName);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(configPath)!);
            if (!File.Exists(configPath))
            {
                SaveConfiguredKey();
                return;
            }

            using FileStream stream = File.OpenRead(configPath);
            using StreamReader reader = new(stream);
            string json = reader.ReadToEnd();
            if (string.IsNullOrWhiteSpace(json))
            {
                SaveConfiguredKey();
                return;
            }

            using JsonDocument document = JsonDocument.Parse(json);
            if (document.RootElement.TryGetProperty("toggleKey", out JsonElement toggleElement) && toggleElement.ValueKind == JsonValueKind.String)
            {
                _toggleKey = ParseKeyCode(toggleElement.GetString());
            }
        }
        catch
        {
            _toggleKey = ParseKeyCode(DefaultPresetKey);
            SaveConfiguredKey();
        }
    }

    private void SaveConfiguredKey()
    {
        string configPath = Path.Combine(MelonEnvironment.UserDataDirectory, ModConfigDirectory, ConfigFileName);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(configPath)!);
            var data = new Dictionary<string, string>
            {
                ["toggleKey"] = _toggleKey.ToString()
            };
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(configPath, json);
        }
        catch
        {
            // Ignore config save failures; the default keybind still remains available.
        }
    }

    private static KeyCode ParseKeyCode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return KeyCode.F5;
        }

        if (Enum.TryParse(value, true, out KeyCode keyCode))
        {
            return keyCode;
        }

        return KeyCode.F5;
    }
}
