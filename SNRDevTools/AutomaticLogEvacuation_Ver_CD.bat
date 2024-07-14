@echo off
setlocal enabledelayedexpansion

rem -------------------------------------------------------------------------------------
rem Among Usが起動されている個数を取得
rem -------------------------------------------------------------------------------------

set COUNT=0
set LOGFILENAME=LogOutput.log

rem Among Usが起動済みかを判定
tasklist /FI "IMAGENAME eq Among Us.exe" | find "Among Us.exe" > NUL

rem 起動台数のカウント

if not %errorlevel% == 0 (
    echo "このウィンドウは [ %LOGFILENAME% ] を出力している Among Us.exe の起動状態を監視しています。"
) else (
    set RAN=1
    for /f %%a in ('tasklist /nh /fi "imagename eq Among Us.exe"') do ( 
        set /a COUNT+=1
        set /a RAN+=1
    )
    set LOGFILENAME=LogOutput.!COUNT!.log
    echo "このウィンドウは Among Us.exe ( !RAN!台目 ) の起動状態を監視しています。"
    echo "アプリケーション終了時 [ !LOGFILENAME! ] を該当フォルダに退避します。"
)

rem -------------------------------------------------------------------------------------

rem -------------------------------------------------------------------------------------
rem 使用するフォルダおよびファイルパス関連の変数を作成 (カレントディレクトリ 使用 Ver.)
rem -------------------------------------------------------------------------------------

rem 起動対象のAmong Us.exeが存在するフォルダを指定する。
set AMONGUSFOLDER=%~dp0

rem 以下の様な形で Among Us.exe が存在するフォルダを指定する形に変更すると、当バッチファイルがAmong Us.exeと別階層に存在しても、使用可能になる
rem set AMONGUSFOLDER=C:\Program Files (x86)\Steam\steamapps\common

set MODFOLDER=%AMONGUSFOLDER%\SuperNewRoles
set AUTOSAVELOGFOLDER=%MODFOLDER%\AutoSaveLogFolder

rem -------------------------------------------------------------------------------------


rem -------------------------------------------------------------------------------------
rem Among Us 起動 & 終了待機
rem -------------------------------------------------------------------------------------

start "" /wait "%AMONGUSFOLDER%\Among Us"

rem -------------------------------------------------------------------------------------


rem -------------------------------------------------------------------------------------
rem 保存フォルダが存在するか確認し, 存在しないなら作成する。(既に存在するならコメントアウト推奨)
rem -------------------------------------------------------------------------------------

if not exist "%AUTOSAVELOGFOLDER%" md "%AUTOSAVELOGFOLDER%"

rem -------------------------------------------------------------------------------------


rem -------------------------------------------------------------------------------------
rem ログファイルの更新日時を取得し, ``yyyyMMdd_hhmm``の形に変換する
rem -------------------------------------------------------------------------------------

for %%i in ("%AMONGUSFOLDER%\BepInEx\%LOGFILENAME%") do set "UPDATE=%%~ti"
set YYYYMMDD_HHMM=%UPDATE:~0,4%%UPDATE:~5,2%%UPDATE:~8,2%_%UPDATE:~11,2%%UPDATE:~14,2%

rem 名前を変換してコピーする
copy /y "%AMONGUSFOLDER%\BepInEx\%LOGFILENAME%" "%AUTOSAVELOGFOLDER%\%YYYYMMDD_HHMM%_%LOGFILENAME%"
echo "[ %YYYYMMDD_HHMM%_%LOGFILENAME% ] を 出力しました。"

rem -------------------------------------------------------------------------------------

rem pause
endlocal


rem -------------------------------------------------------------------------------------
rem # 参考

rem ## プロセスの状態関連の処理

rem ### プロセスが起動済みかの判定
rem - https://cool.japan.ne.jp/win-dos-batch_proc
rem - https://itlogs.net/windows-bat-process-check/

rem ### プロセスの状態の取得
rem - https://note.com/good_lilac166/n/n0b13bb383737
rem - https://wa3.i-3-i.info/word12514.html

rem ### 指定プロセスの起動台数の確認
rem - https://note.com/good_lilac166/n/n0b13bb383737
rem - https://qiita.com/plcherrim/items/9cba5a42273e10915c8f

rem ## 遅延展開
rem - https://qiita.com/talesleaves/items/8990a55b7a770de3d34f#%E6%8B%AC%E5%BC%A7%E3%81%AE%E4%B8%AD%E3%81%A7%E5%A4%89%E6%95%B0%E3%82%92set%E3%81%97%E3%81%9F%E3%82%89%E9%81%85%E5%BB%B6%E5%B1%95%E9%96%8B%E3%82%92%E4%BD%BF%E3%81%86%E3%82%88
rem - https://qiita.com/tana_tomo_1025/items/7f824a154f004f610386

rem ## アプリの終了を待機する方法
rem - https://qiita.com/talesleaves/items/8990a55b7a770de3d34f#%E5%88%A5%E3%82%A2%E3%83%97%E3%83%AA%E3%82%B1%E3%83%BC%E3%82%B7%E3%83%A7%E3%83%B3%E3%82%92%E8%B5%B7%E5%8B%95%E3%81%95%E3%81%9B%E3%82%8Bstart

rem ## 最終更新時間の取得とフォーマット
rem - https://tekuzo.org/cmd-date-filename/
rem - https://tecsingularity.com/windows/update/

rem ## ディレクトリの確認及び作成
rem - https://windows.command-ref.com/cmd-md.html

rem ## ファイルのコピー
rem - https://www.javadrive.jp/command/file/index5.html

rem ## カレントディレクトリの取得
rem - https://qiita.com/shin1rok/items/efb5052ef5fb8138c26d
rem -------------------------------------------------------------------------------------