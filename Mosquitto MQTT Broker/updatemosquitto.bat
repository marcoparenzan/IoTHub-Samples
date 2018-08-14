docker stop sleepy_vaughan
docker cp ./mosquitto.conf sleepy_vaughan:/mosquitto/config/mosquitto.conf
docker start sleepy_vaughan
docker logs sleepy_vaughan