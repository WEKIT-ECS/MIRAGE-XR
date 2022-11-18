echo "Retrieving manual activation file for Unity version "

mkdir artifacts

"C:\Program Files\Unity\Hub\Editor\2021.3.8f1\Editor\Unity.exe" -batchmode -nographics -logFile artifacts\logfile.log -quit -createManualActivationFile

copy Unity_*.alf artifacts\UnityActivationFile.alf

type artifacts\logfile.log
::type artifacts\UnityActivationFile.log
