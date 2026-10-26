using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Il2Cpp;
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
                return;
            }

            controller.ChangeCamera(PlayerController.CameraType.ThirdPersonCamera, controller.CameraRoot);
        }
        catch (Exception ex)
        {
            LoggerInstance.Warning($"Could not switch POV: {ex.Message}");
        }
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
        string configPath = Path.Combine(MelonEnvironment.UserDataDirectory, ModConfigDirectory, ConfigFileName);
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
