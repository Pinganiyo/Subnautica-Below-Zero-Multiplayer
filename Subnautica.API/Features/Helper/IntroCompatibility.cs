public class IntroVignette : UnityEngine.MonoBehaviour
{
    private static IntroVignette _main;
    public static IntroVignette main
    {
        get
        {
            if (_main == null)
            {
                var go = new UnityEngine.GameObject("IntroVignetteCompatibility");
                UnityEngine.Object.DontDestroyOnLoad(go);
                _main = go.AddComponent<IntroVignette>();
            }
            return _main;
        }
        set => _main = value;
    }

    public static bool isIntroActive;
    public global::Player player => global::Player.main;

    public void OnDone()
    {
        if (GameModeManager.GetOption<bool>(GameOption.InitialEquipmentPack))
        {
            global::Player.main?.AddInitialEquipment();
        }

        if (!GameModeManager.GetOption<bool>(GameOption.Story))
        {
            Jukebox.UnlockAll();
        }
    }

    public System.Collections.IEnumerator Start()
    {
        yield break;
    }
}

