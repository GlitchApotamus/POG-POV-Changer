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
        public virtual void OnLateUpdate() { }
        public virtual void OnGUI() { }
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
        public static void Destroy(Object obj) { }
    }

    public class GameObject : Object
    {
        public GameObject(string name) { }
        public Transform transform => null!;
        public int layer { get; set; }
    }

    public struct Quaternion
    {
    }

    public class Component : Object
    {
        public Transform transform => null!;
        public T GetComponent<T>() where T : Component => null!;
    }

    public struct Vector3
    {
        public float x;
        public float y;
        public float z;

        public static float Distance(Vector3 a, Vector3 b) => 0f;
    }

    public struct Color
    {
        public Color(float r, float g, float b, float a) { }
        public static Color white => default;
    }

    public struct Rect
    {
        public Rect(float x, float y, float width, float height) { }
    }

    public class Texture2D : Object
    {
        public static Texture2D whiteTexture => null!;
    }

    public static class Screen
    {
        public static int width => 0;
        public static int height => 0;
    }

    public static class GUI
    {
        public static Color color { get; set; }
        public static void DrawTexture(Rect position, Texture2D image) { }
    }

    public struct LayerMask
    {
        public int value;
    }

    public enum QueryTriggerInteraction
    {
        UseGlobal,
        Ignore,
        Collide
    }

    public class Collider : Component
    {
        public string name => string.Empty;
        public bool isTrigger => false;
        public Vector3 ClosestPoint(Vector3 position) => position;
        public GameObject gameObject => null!;
    }

    public struct RaycastHit
    {
        public Collider collider;
        public float distance;
    }

    public static class Physics
    {
        public static Collider[] OverlapSphere(Vector3 position, float radius, int layerMask, QueryTriggerInteraction queryTriggerInteraction) => Array.Empty<Collider>();

        public static bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float maxDistance, int layerMask)
        {
            hitInfo = default;
            return false;
        }
    }

    public class Transform : Component
    {
        public Vector3 position { get; set; }
        public Quaternion rotation { get; set; }
        public Vector3 forward => default;
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

namespace Il2CppGame.Interaction
{
    public class PlayerInteract : UnityEngine.Component
    {
        public UnityEngine.Transform m_rayOrigin { get; set; } = null!;
        public float m_distance { get; set; }
        public float m_sphereRadius { get; set; }
        public UnityEngine.LayerMask m_layers { get; set; }
        public bool m_rayActive { get; set; }
        public bool CanInteract { get; set; }
        public object? m_currentSelectedInteractable { get; set; }
        public void ToggleRay(bool enabled, bool deselectCurrentInteractable = false) { }
    }
}

namespace Il2CppEntities
{
    public class EntityPlayer : UnityEngine.Component
    {
        public Il2Cpp.PlayerController m_playerController { get; set; } = null!;
        public Il2CppGame.Interaction.PlayerInteract PlayerInteract { get; set; } = null!;
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
