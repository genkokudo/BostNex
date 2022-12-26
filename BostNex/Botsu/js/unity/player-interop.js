// HTMLにこれらのIDを配置しておき、取得してスクリプトで表示などを操作する
var container = document.querySelector("#unity-container");
var canvas = document.querySelector("#unity-canvas");
var loadingBar = document.querySelector("#unity-loading-bar");
var progressBarFull = document.querySelector("#unity-progress-bar-full");
var fullscreenButton = document.querySelector("#unity-fullscreen-button");
var warningBanner = document.querySelector("#unity-warning");
var unityTest = document.querySelector("#unity-test");

// 上記は1度しか実行されないため、ページ遷移をすると見失う
// そのために画面の要素を再取得する
function getElements() {
    container = document.querySelector("#unity-container");
    canvas = document.querySelector("#unity-canvas");
    loadingBar = document.querySelector("#unity-loading-bar");
    progressBarFull = document.querySelector("#unity-progress-bar-full");
    fullscreenButton = document.querySelector("#unity-fullscreen-button");
    warningBanner = document.querySelector("#unity-warning");
    unityTest = document.querySelector("#unity-test");
}

// 一時的なメッセージバナーリボンを数秒間表示し、type=='error' の場合は永久的なエラーメッセージをキャンバスの上部に表示します。
// type == 'warning' の場合、黄色のハイライトカラーが使用されます。
// この関数を変更または削除して、重要でない警告とエラーメッセージの視覚的な表示方法をカスタマイズします。
function unityShowBanner(msg, type) {
    function updateBannerVisibility() {
        warningBanner.style.display = warningBanner.children.length ? 'block' : 'none';
    }
    var div = document.createElement('div');
    div.innerHTML = msg;
    warningBanner.appendChild(div);
    if (type == 'error') div.style = 'background: red; padding: 10px;';
    else {
        if (type == 'warning') div.style = 'background: yellow; padding: 10px;';
        setTimeout(function () {
            warningBanner.removeChild(div);
            updateBannerVisibility();
        }, 5000);
    }
    updateBannerVisibility();
}

// 画面の要素に対して設定を行う
function setEnv(width, height) {
    // デフォルトでは、Unity は WebGL canvas のレンダーターゲットサイズを canvas 要素の DOM サイズと一致させています（window.devicePixelRatio でスケーリング）。
    // この同期をエンジン内部から切り離し、代わりに canvas DOM サイズと WebGL レンダーターゲットサイズのサイズを自分で設定したい場合は false を設定します。
    // config.matchWebGLToCanvasSize = false;

    if (/iPhone|iPad|iPod|Android/i.test(navigator.userAgent)) {
        // モバイル用のStyle:
        // ブラウザのクライアント領域全体をゲームキャンバスで埋め尽くします。
        var meta = document.createElement('meta');
        meta.name = 'viewport';
        meta.content = 'width=device-width, height=device-height, initial-scale=1.0, user-scalable=no, shrink-to-fit=yes';
        document.getElementsByTagName('head')[0].appendChild(meta);
        container.className = "unity-mobile";
        canvas.className = "unity-mobile";

        // モバイルデバイスでキャンバスの解像度を下げてパフォーマンスを上げるには、
        // 次の行をアンコメントしてください：
        // config.devicePixelRatio = 1;

    } else {
        // デスクトップ用のStyle:
        // ゲームキャンバスを、最大化からフルスクリーンまで可能なウィンドウでレンダリングします。
        canvas.style.width = width + "px";
        canvas.style.height = height + "px";
    }

    loadingBar.style.display = "block";
}

// #unity-containerタグを配置したページでこれを呼ぶことで、Unityを実行出来る
// 引数のconfigはC#側でPlayerConfigオブジェクトを作成して渡す。
export function init(config) {
    // ページ遷移すると見失うので、要素の再取得が必要
    getElements();
    
    // コンフィグにJS関数をセットする（C#ではできないので）
    config.showBanner = unityShowBanner;

    // 画面の要素に対して環境設定を適用する
    setEnv(config.width, config.height);

    // 作成したアプリの設定を適用して
    // loader.jsを使ってUnityの画面を描画する
    // loader.jsは各アプリで違うかもしれないので、一応各アプリフォルダに入れる（要確認）
    //var loaderUrl = "Escape/Build/Escape.loader.js";
    var script = document.createElement("script");
    script.src = config.loaderUrl;

    // 読み込んでから実行する
    script.onload = () => {
        createUnityInstance(canvas, config, (progress) => {
            progressBarFull.style.width = 100 * progress + "%";
        }).then((unityInstance) => {
            loadingBar.style.display = "none";
            fullscreenButton.onclick = () => {
                unityInstance.SetFullscreen(1);
            };
        }).catch((message) => {
            console.log(message);
            // alert(message);
        });
    };

    // 画面のbodyに生成した<script>タグを付け加える
    //document.body.appendChild(script);
    // ※これをやると、どんどん要素が増えてしまって画面遷移しても消えない。。。
    // 取り敢えずalertは切って、ページ遷移時に消すスクリプトを付けるとか。

    // こいつらが追加される
    // <script src="Escape/Build/Escape.loader.js"></script>
    // <script src="Escape/Build/Escape.framework.js.br"></script>

    // Blazorの機能で遷移を検知して消す
    unityTest.appendChild(script);
}

//// ページ遷移時にscriptを削除する
//export function deleteScripts() {
//    // 生成したscriptタグを削除する
//    var scripts = document.getElementsByTagName("script");
//    for (var i = 0; i < scripts.length; i++) {
//        if (scripts[i].src.includes("loader.js") || scripts[i].src.includes("framework.js.br")) {
//            scripts[i].remove();
//            i--;
//        }
//    }
//}


