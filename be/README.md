1. Create be folder
2. dotnet new webapi --use-controllers -o server (https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-web-api?view=aspnetcore-8.0&tabs=visual-studio-code)
3. cd server, dotnet add package Microsoft.EntityFrameworkCore.InMemory
4. Create docker for mongodb:
  docker run -d \
  --name mongodb-assignment1 \
  -p 27017:27017 \
  -v mongodbdata:/data/db \
  -e MONGODB_USERNAME=oscar \
  -e MONGODB_PASSWORD=123asd \
  -e MONGODB_DATABASE=musicappdb \
  mongo:latest
5. mongosh mongodb://localhost:27017
6. use musicappdb
7.  db.createUser({
...     user: "oscar", 
...     pwd: "123asd", 
...     roles: [ { role: "readWrite", db: "musicappdb" } ] 
... })