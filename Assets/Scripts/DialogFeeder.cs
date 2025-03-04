using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogFeeder : MonoBehaviour
{
    [SerializeField, Header("シナリオを格納する"), TextArea] List<string> _scenarios;
    [SerializeField, Header("表示させるTextUI")] TextMeshPro _uiText;
    [SerializeField, Range(0.001f, 0.3f), Header("1文字の表示にかかる時間")] float _intervalForCharacterDisplay = 0.05f;
    [SerializeField, Range(0.1f, 5f), Header("テキストの切り替えにかかる時間")] float _switchScenarioTime = 1f;
    [SerializeField, Header("割り込みの可否")] bool _canInterrupt = false;

    string _currentText = string.Empty;      // 現在の文字列
    float _timeUntilDisplay = 0;             // 表示にかかる時間
    float _switchScenarioTimer = 0;          // テキストの切り替え用タイマー
    float _timeElapsed = 1;                  // 文字列の表示を開始した時間
    float _pauseTime = 0;                    // 一時停止時のタイマー
    int _currentLine = 0;                    // 現在の行番号
    int _lastUpdateCharacter = -1;           // 表示中の文字数
    bool _isUpdatingText = false;            // テキスト更新中かどうか
    bool _isPause = false;                   // 一時停止中かどうか
    Coroutine _coroutine = null;             // 起動中のTextUpdateを格納する

    /// <summary>文字の更新中かどうか</summary>
    public bool IsUpdatingText => _isUpdatingText;

    /// <summary>文字の表示が完了しているかどうか</summary>
    public bool IsCompleteDisplayText => Time.time > _timeElapsed + _timeUntilDisplay;

    /// <summary>1文字の表示にかかる時間を設定します</summary>
    public void SetIntervalForCharacterDisplay(float time) => _intervalForCharacterDisplay = time;

    /// <summary>テキストの切り替えにかかる時間を設定します</summary>
    public void SetSwitchScenarioTime(float time) => _switchScenarioTime = time;

    public void OverrideScenarios(List<string> texts) => _scenarios = texts;

    private void Start()
    {
        TextStart();
    }

    /// <summary>
    /// 文字送りを始める
    /// </summary>
    public void TextStart()
    {
        if (_canInterrupt && _coroutine != null)
        {
            PauseFeedText();
        }

        if (!_isUpdatingText)
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
            }

            // 子オブジェクトの最後尾に配置することで複数あっても一番上に見えます。
            this.gameObject.transform.SetAsLastSibling();
            _currentLine = 0;
            _switchScenarioTimer = 0;
            _pauseTime = 0;
            _isPause = false;
            SetNextLine();
            _coroutine = StartCoroutine(TextUpdate());
        }
    }

    /// <summary>
    /// 文字送りを行う
    /// </summary>
    /// <returns></returns>
    IEnumerator TextUpdate()
    {
        _isUpdatingText = true;

        while (_currentText != string.Empty)
        {
            while (_isPause == true)
            {
                _pauseTime += Time.deltaTime;
                yield return null;
            }

            if (IsCompleteDisplayText && _currentText != string.Empty)
            {
                //Debug.Log((int)_switchScenarioTimer);
                //時間経過で次のテキストに切り替わるようにする
                _switchScenarioTimer += Time.deltaTime;
            }

            // 文字の表示が完了してるかつ切り替え時間に達したなら次の行を表示する
            if (IsCompleteDisplayText && _switchScenarioTimer > _switchScenarioTime && _currentText != string.Empty)
            {
                SetNextLine();
                _switchScenarioTimer = 0;
                _pauseTime = 0;
            }

            // クリックから経過した時間が想定表示時間の何%か確認し、表示文字数を出す
            int displayCharacterCount = (int)(Mathf.Clamp01((Time.time - _timeElapsed - _pauseTime) / _timeUntilDisplay) * _currentText.Length);

            // 表示文字数が前回の表示文字数と異なるならテキストを更新する
            if (displayCharacterCount != _lastUpdateCharacter)
            {
                _uiText.text = _currentText.Substring(0, displayCharacterCount);
                _lastUpdateCharacter = displayCharacterCount;
            }

            yield return new WaitForEndOfFrame();
        }

        StopFeedText();
    }

    /// <summary>
    /// テキストを更新する
    /// </summary>
    void SetNextLine()
    {
        // 配列の最後に達していないなら時刻等をキャッシュする
        if (_currentLine < _scenarios.Count)
        {
            _currentText = _scenarios[_currentLine];
            // 想定表示時間と現在の時刻をキャッシュ
            _timeUntilDisplay = _currentText.Length * _intervalForCharacterDisplay;
            _timeElapsed = Time.time;
            _currentLine++;
            // 文字カウントを初期化
            _lastUpdateCharacter = -1;
        }
        else
        {
            // シナリオデータがなくなったらテキストは表示しない
            //_currentText = string.Empty;
            // 文字カウントを初期化
            _lastUpdateCharacter = -1;
        }
    }

    /// <summary>
    /// 文字送りを一時停止します。
    /// </summary>
    public void PauseFeedText()
    {
        if (_coroutine != null)
        {
            _isUpdatingText = false;
            _isPause = true;
        }
    }

    /// <summary>
    /// 文字送りを再開します。
    /// </summary>
    public void RestartFeedText()
    {
        if (_coroutine != null)
        {
            _isUpdatingText = true;
            _isPause = false;
        }
    }

    /// <summary>
    /// 文字送りを停止します。
    /// </summary>
    public void StopFeedText()
    {
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
        }

        _isUpdatingText = false;
    }
}
