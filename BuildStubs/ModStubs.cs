using System;
using System.Collections.Generic;

namespace MelonLoader
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class MelonInfoAttribute : Attribute
    {
        public MelonInfoAttribute(Type modType, string name, string version, string author) { }
    }

    public class MelonLogger
    {
        public void Msg(string message) { }
        public void Warning(string message) { }
    }

    public abstract class MelonMod
    {
        public MelonLogger LoggerInstance { get; } = new MelonLogger();

        public virtual void OnInitializeMelon() { }
        public virtual void OnUpdate() { }
    }
}

namespace MelonLoader.Utils
{
    public static class MelonEnvironment
    {
        public static string UserDataDirectory { get; } = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    }
}

namespace UnityEngine
{
    public enum KeyCode
    {
        None,
        F5,
        V,
        A,
        B,
        C,
        D,
        E,
        F,
        G,
        H,
        I,
        J,
        K,
        L,
        M,
        N,
        O,
        P,
        Q,
        R,
        S,
        T,
        U,
        W,
        X,
        Y,
        Z
    }

    public static class Input
    {
        public static bool GetKeyDown(KeyCode keyCode) => false;
    }

    public class Object
    {
        public static T[] FindObjectsOfType<T>() where T : Object => Array.Empty<T>();
    }

    public class Component : Object
    {
        public T GetComponent<T>() where T : Component => null!;
    }

    public class Transform : Component
    {
    }

    public class Camera : Component
    {
    }
}

namespace Il2Cpp
{
    public class PlayerController : UnityEngine.Component
    {
        public enum CameraType
        {
            FirstPersonCamera,
            ThirdPersonCamera,
            SpectateCamera
        }

        public CameraType CurrentCameraType { get; set; }
        public UnityEngine.Transform CameraRoot => null!;
        public UnityEngine.Camera MainCamera => null!;

        public void SetFirstPersonCamera() { }
        public void ChangeCamera(CameraType cameraType, UnityEngine.Transform lookAt) { }
    }
}

namespace POGMods.Config
{
    public abstract class ConfigEntry
    {
    }

    public class ModInfo
    {
    }

    public static class POGConfig
    {
        public static void Register(string modName, List<ConfigEntry> entries) { }
        public static void Register(string modName, List<ConfigEntry> entries, ModInfo info) { }
    }

    public class KeyEntry : ConfigEntry
    {
        public KeyEntry(string label, Func<UnityEngine.KeyCode> getter, Action<UnityEngine.KeyCode> setter)
        {
        }

        public KeyEntry(string label, Func<UnityEngine.KeyCode> getter, Action<UnityEngine.KeyCode> setter, string description)
        {
        }
    }

    public class ToggleEntry : ConfigEntry
    {
        public ToggleEntry(string label, Func<bool> getter, Action<bool> setter)
        {
        }

        public ToggleEntry(string label, Func<bool> getter, Action<bool> setter, string description)
        {
        }
    }
}
