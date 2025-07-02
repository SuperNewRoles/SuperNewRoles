// スマートフォン検出
function isMobileDevice() {
  return /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent) ||
    (window.innerWidth <= 768);
}

// デバイスに応じた表示切り替え
function detectDeviceAndShowContent() {
  if (isMobileDevice()) {
    document.getElementById('mobile-message').style.display = 'block';
    document.getElementById('pc-content').style.display = 'none';
  } else {
    document.getElementById('mobile-message').style.display = 'none';
    document.getElementById('pc-content').style.display = 'block';
    // 現在のURLから全パラメータを取得し、joinGameエンドポイントにアクセスする処理
    connectToServer();
  }
}

function connectToServer() {
  const queryParams = window.location.search;
  console.log("connecting: " + `http://localhost:49152/joinGame${queryParams}`);

  // DOM要素をキャッシュして取得
  const connectingScreen = document.getElementById('connecting-screen');
  const errorScreen = document.getElementById('error-screen');
  const errorText = document.getElementById('error-text');

  fetch(`http://localhost:49152/joinGame${queryParams}`)
    .then(response => {
      console.log("RESPONSED!!!!!!!");
      return response.text().then(text => ({ status: response.status, text }));
    })
    .then(({ status, text }) => {
      connectingScreen.style.display = 'none';
      errorScreen.style.display = 'block';
      if (status === 200 && text === "接続しました。") {
        errorText.textContent = "サーバーに接続できました。";
      } else {
        errorText.innerText = text;
      }
    })
    .catch(error => {
      console.error("joinGameへのアクセスでエラーが発生:", error);
      connectingScreen.style.display = 'none';
      errorScreen.style.display = 'block';
      errorText.innerText = "サーバーに接続できませんでした。";
    });
}

function createStarBackground() {
  // 星を生成
  const starContainer = document.getElementById('starBackground');
  // DocumentFragment を使用して再描画回数を軽減
  const starFragment = document.createDocumentFragment();

  // 100個の星を生成して配置
  for (let i = 0; i < 100; i++) {
    const star = document.createElement('div');
    const size = Math.random() * 2 + 1;
    const top = `${Math.random() * 100}%`;
    const left = `${Math.random() * 100 - 40}%`;
    const duration = `${Math.random() * 40 + 20}s`;
    const delay = `-${Math.random() * 40}s`;

    star.classList.add('star');
    star.style.width = `${size}px`;
    star.style.height = `${size}px`;
    star.style.top = top;
    star.style.left = left;
    star.style.animationDelay = `${Math.random() * 5}s, ${delay}`;
    star.style.animationDuration = `3s, ${duration}`;
    star.style.animationName = 'twinkle, moveRight';
    star.style.animationIterationCount = 'infinite, infinite';
    star.style.animationDirection = 'alternate, normal';
    star.style.animationTimingFunction = 'ease-in-out, linear';

    // DocumentFragment に追加
    starFragment.appendChild(star);
  }
  // 一度に DOM に追加して再描画回数を軽減
  starContainer.appendChild(starFragment);
}

// domLoaded
document.addEventListener('DOMContentLoaded', () => {
  console.log("DOMContentLoaded");
  createStarBackground();
});

// デバイス検出を実行
detectDeviceAndShowContent();

// 再試行ボタンのクリックイベント
document.querySelector('.retry-button').addEventListener('click', () => {
  document.getElementById('connecting-screen').style.display = 'block';
  document.getElementById('error-screen').style.display = 'none';
  connectToServer();
});