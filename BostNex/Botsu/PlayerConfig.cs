namespace BlizzardHaunt.Unity
{
    /// <summary>
    /// UnityPlayerのJSのコンフィグのデータ構造を定義
    /// </summary>
    public class PlayerConfig
    {
        public string DataUrl { get; set; }
        public string FrameworkUrl { get; set; }
        public string CodeUrl { get; set; }
        public string StreamingAssetsUrl { get; set; }
        public string CompanyName { get; set; }
        public string ProductName { get; set; }
        public string ProductVersion { get; set; }
        public string LoaderUrl { get; set; }

        // 表示する大きさ
        public int Width { get; set; }
        public int Height { get; set; }

        // ※この他にShowBannerというJavaScript関数を持たせる
        // C#からは設定できない

        // pathNameは"Escape"とか。
        // appNameは"My Revolution"とか。
        public PlayerConfig(string pathName, string appName, int width = 800, int height = 600, string buildUrl = "Build", string companyName = "BostNex", string appVersion = "1.0")
        {
            DataUrl = $"{pathName}/{buildUrl}/{pathName}.data.br";
            FrameworkUrl = $"{pathName}/{buildUrl}/{pathName}.framework.js.br";
            CodeUrl = $"{pathName}/{buildUrl}/{pathName}.wasm.br";
            StreamingAssetsUrl = $"{pathName}/StreamingAssets";
            CompanyName = companyName;
            ProductName = appName;
            ProductVersion = appVersion;
            LoaderUrl = $"{pathName}/{buildUrl}/{pathName}.loader.js";  // "Escape/Build/Escape.loader.js"
            Width = width;
            Height = height;
        }
    }
}
