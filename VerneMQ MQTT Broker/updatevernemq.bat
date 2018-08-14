docker stop gracious_yalow
docker cp ./vernemq.conf gracious_yalow:/etc/vernemq/vernemq.conf
docker start gracious_yalow
docker logs gracious_yalow