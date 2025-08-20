using UnityEngine;
// System.Diagnostics 名前空間を使用するために、この行を追加します
using System.Diagnostics;

// このスクリプトがアタッチされたオブジェクトが非表示にされた瞬間に、
// 誰がそれを実行したかの履歴をログに出力する
public class DisableViewDetector : MonoBehaviour
{
    // このオブジェクトが非アクティブにされた瞬間に、Unityによって自動的に呼ばれる命令
    private void OnDisable()
    {
        // 現在の呼び出し履歴（コールスタック）を取得
        StackTrace stackTrace = new StackTrace(true); // trueで行番号などの詳細情報を取得

        // 取得した履歴を、見やすいように整形してログに出力する
        // Debug.Log自体も履歴に含まれるため、1フレーム分スキップして表示
        UnityEngine.Debug.Log(gameObject.name + " was disabled! Call Stack:\n" + stackTrace.ToString());
    }
}