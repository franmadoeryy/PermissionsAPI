# PermissionsAPI
This project is a REST API for managing permissions, built with .NET 8, using SQL Server, Elasticsearch, and Kafka.  
All services run inside Docker containers and you only need Docker, Docker Compose and the .NET 8 SDK installed locally.

## How to run the project
 **Requirements:**  
 - Docker  
 - Docker Compose
 - .NET 8 SDK ([https://dotnet.microsoft.com/en-us/download/dotnet/8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0))
### 1.  Clone the repository
```
git clone https://github.com/franmadoeryy/PermissionsAPI.git
```
Then open a command prompt (cmd) and move to the path where you cloned the repository:
 ```
 cd Your_Local_Path
 ```


### 2. Build and start all services with Docker Compose

```
docker-compose up --build
```
This will start the following services:
- SQL Server 
- Elasticsearch 
- Kafka & Zookeeper
- Kafka UI
- Permissions API


### 3.  Apply database migrations

First, make sure you have the .NET 8 SDK installed.  
You can download it from: [https://dotnet.microsoft.com/en-us/download/dotnet/8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)


Then, from the root of the cloned repository, run:
```
dotnet ef database update --startup-project Permissions.API --project Permissions.Infrastructure"
```
This will run the migrations creating the database in the configured SQL server container.

Note that the PermissionsType table is automatically seeded with these three records:
| Id | Description      |
|----|------------------|
| 1  | Superadmin       |
| 2  | Admin            |
| 3  | User             |

You can use the ids **1**, **2**, or **3** for the `PermissionTypeId` field when testing or creating permissions via the API.  


### 4. Access the API
Once all containers are up and the migrations are complete, open [http://localhost:8080/swagger](http://localhost:8080/swagger)
Here you will find the Swagger UI to test all endpoints.

### 5. Access Kafka UI (optional)
Kafka UI will be available at:
[http://localhost:8081](http://localhost:8081)

Here you'll be able to see the messages posted in the Kafka topic.


---

## Running Tests

To run all unit and integration tests, make sure the containers are up (`docker-compose up --build` running) and then execute:

```
dotnet test
```

Tests will use the same environment and database defined in your Docker Compose setup.


## Default Credentials & Configuration

- **SQL Server**
  - User: `SA`
  - Password: `Fran1234`
  - Database: `PermissionsDb`
- **Elasticsearch:** [http://localhost:9200](http://localhost:9200)
- **Kafka UI:** [http://localhost:8081](http://localhost:8081)

All credentials and connection strings are configured in `docker-compose.yml` and `appsettings.json`.


## Common Issues

- **Ports already in use:** Make sure nothing else is running on ports 1433 (SQL Server), 9200 (Elasticsearch), 8080 (API), 8081 (Kafka UI), or 9092/29092 (Kafka).
- **Slow container startup:** Give containers a little bit to fully start before applying migrations or running tests, especially on the first run.
- **Resetting data:**  
  If you need to reset all persisted data (including the database and Elasticsearch indexes), stop the containers and run:

  ```
  docker-compose down -v
  ```

  Then repeat the steps above.

