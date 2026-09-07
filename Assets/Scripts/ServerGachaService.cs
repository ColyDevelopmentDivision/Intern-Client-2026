using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace GachaWorkshop
{
    /// <summary>
    /// サーバー会用の結果サービス。IFどおり POST {serverUrl}/api/gacha/draw に
    /// GachaDrawInParam（gachaId, userId）を送り、返ってきた GachaDrawOutParam をそのまま渡します。
    /// 選択中のテーブルの結果を取得できなかった場合（サーバーがテーブル1つのみ等）は、
    /// 一つ目のテーブル（gachaId=1）で1回だけ再試行します（予防動作）。
    /// それでも失敗した場合は空の結果を返します（Console にエラー表示）。
    /// </summary>
    public class ServerGachaService : IGachaService
    {
        private const string DrawPath = "/api/gacha/draw";
        private const string DefaultServerUrl = "http://localhost:5109";

        private readonly string _serverUrl;

        public ServerGachaService(string serverUrl)
        {
            _serverUrl = string.IsNullOrEmpty(serverUrl) ? DefaultServerUrl : serverUrl.TrimEnd('/');
        }

        public async void Draw(GachaTable table, long userId, Action<GachaDrawOutParam> onCompleted)
        {
            if (table == null)
            {
                Debug.LogError("[ServerGachaService] GachaTable が未設定です（リクエストの gachaId に使います）。");
                onCompleted?.Invoke(new GachaDrawOutParam());
                return;
            }

            GachaDrawOutParam response = await SendDrawRequestAsync(table.gachaId, userId);

            // 予防動作: 選択テーブルの結果が取れず、それが一つ目のテーブルでないなら gachaId=1 で再試行
            if ((response == null || response.results.Count == 0)
                && table.gachaId != GachaServiceFactory.PrimaryGachaId)
            {
                Debug.LogWarning($"[ServerGachaService] gachaId={table.gachaId} の結果を取得できなかったため、一つ目のテーブル（gachaId={GachaServiceFactory.PrimaryGachaId}）で再試行します（サーバーがテーブル1つのみの場合の予防動作）。");
                response = await SendDrawRequestAsync(GachaServiceFactory.PrimaryGachaId, userId);
            }

            onCompleted?.Invoke(response ?? new GachaDrawOutParam());
        }

        /// <summary>1回分のAPI呼び出し。失敗時は null を返す（呼び出し側でフォールバック判断）。</summary>
        private async Task<GachaDrawOutParam> SendDrawRequestAsync(int gachaId, long userId)
        {
            string url = _serverUrl + DrawPath;
            string json = JsonUtility.ToJson(new GachaDrawInParam { gachaId = gachaId, userId = userId });

            try
            {
                using (var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
                {
                    request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
                    request.downloadHandler = new DownloadHandlerBuffer();
                    request.SetRequestHeader("Content-Type", "application/json");
                    request.timeout = 10;

                    await request.SendWebRequest();

                    string responseText = request.downloadHandler.text;

                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        // 失敗時もサーバーからの応答本文があれば表示する（エラー内容の確認用）
                        string errorBody = string.IsNullOrEmpty(responseText) ? "" : $"\nサーバーからの応答: {responseText}";
                        Debug.LogError($"[ServerGachaService] 通信に失敗しました: {request.error}（URL: {url}, gachaId: {gachaId}, userId: {userId}, HTTP {request.responseCode}。サーバーは起動していますか？）{errorBody}");
                        return null;
                    }

                    var response = JsonUtility.FromJson<GachaDrawOutParam>(responseText);
                    if (response == null || response.results == null)
                    {
                        Debug.LogError($"[ServerGachaService] レスポンスを読み取れませんでした: {responseText}");
                        return null;
                    }

                    // サーバーから返ってきた情報の全文をコンソールで確認できるようにする
                    // （ログをクリックすると全体が見られます）
                    var log = new System.Text.StringBuilder();
                    log.AppendLine($"[ServerGachaService] サーバーから結果を受信しました（gachaId: {gachaId}, userId: {userId}, HTTP {request.responseCode}, {response.results.Count}件）");
                    log.AppendLine($"受信JSON: {responseText}");
                    for (int i = 0; i < response.results.Count; i++)
                    {
                        GachaDrawResult r = response.results[i];
                        log.AppendLine($"  {i + 1}枚目: itemId={r.itemId}, isNew={r.isNew}");
                    }
                    Debug.Log(log.ToString());
                    return response;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ServerGachaService] 通信中にエラーが発生しました: {e.Message}（URL: {url}, gachaId: {gachaId}, userId: {userId}）");
                return null;
            }
        }
    }
}
