namespace Subnautica.API.Extensions
{
    public static class CompatibilityExtensions
    {
        public static bool IsLoading(this uGUI_SceneLoading loading)
        {
            return loading != null && loading.isLoading;
        }

        public static bool isWorking(this PAXTerrainController controller)
        {
            return false;
        }

        public static void SetStageProgress(this uGUI_SceneLoading loading, string stage, float progress)
        {
        }
    }
}
