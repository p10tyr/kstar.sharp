cls
@echo off

docker build -f kstar.sharp.console/Dockerfile -t kstar-sharp-console .

echo.
echo docker run kstar.sharp/console -- --ip-192.168.1.150 --mqtt-homeassistant --silent-
echo.
echo.
echo docker image tag kstar-sharp-console:latest dockerubuntu:49153/kstar-sharp-console
echo docker image push dockerubuntu:49153/kstar-sharp-console
echo Press any key to set latest tag and push to repo
pause
docker image tag kstar-sharp-console:latest dockerubuntu:49153/kstar-sharp-console
docker image push dockerubuntu:49153/kstar-sharp-console