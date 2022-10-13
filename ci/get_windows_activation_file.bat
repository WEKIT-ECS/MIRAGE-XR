echo "Retrieving manual activation file for Unity version 2019.4.5f1."

mkdir artifacts

:: old path "C:\Program Files\Unity\Hub\Editor\2019.4.5f1\Editor\Unity.exe"
"C:\Program Files\Unity\Hub\Editor\2020.3.38f1\Editor\Unity.exe" -batchmode -nographics -logFile artifacts\logfile.log -quit -createManualActivationFile

:: old name copy Unity_v2019.4.5f1.alf artifacts\Unity_v2019.4.5f1.alf
copy Unity_*.alf artifacts\UnityActivationFile.alf

type artifacts\logfile.log
type artifacts\UnityActivationFile.log
