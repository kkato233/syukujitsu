pushd app\SyukujitsuGet
del _tmp.json
del _tmp.csv
dotnet run jsonFile=_tmp.json csvFile=_tmp.csv
if errorlevel 1 goto error_bat
popd

del syukujitsu.json
del syukujitsu.csv
move app\SyukujitsuGet\_tmp.json syukujitsu.json
move app\SyukujitsuGet\_tmp.csv syukujitsu.csv
copy syukujitsu.json docs\lib\ja-holiday\syukujitsu.json /Y

goto exit_bat

:error_bat
echo 処理エラー
popd

goto exit_bat

:exit_bat

