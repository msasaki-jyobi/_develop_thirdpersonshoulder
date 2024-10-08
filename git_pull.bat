@echo off
:: バッチファイルが存在するディレクトリを取得
set scriptdir=%~dp0

:: プロジェクトディレクトリに移動
cd /d "%scriptdir%"

:: 作業内容を取り消してリセット
git reset --hard
git clean -fd

:: 最新の変更を取得
git pull

:: 終了メッセージ
echo Pull completed. Your repository is up to date.
pause
