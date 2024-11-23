# Inventory Management System

This is a simple **Inventory Management System** that allows users to reserve stock for a product. The application follows Domain-Driven Design (DDD) principles with the **CQRS** pattern and implements the **Outbox Pattern** for asynchronous messaging with RabbitMQ/Kafka. The project also leverages **Docker** and **Kubernetes** for containerization and orchestration.

## Features

- **Inventory Management**: Reserve and release stock from the inventory.
- **CQRS Architecture**: Separate read and write operations to improve scalability.
- **Domain-Driven Design (DDD)**: Clean architecture with aggregates, entities, and domain services.
- **Outbox Pattern**: For ensuring reliable messaging and integration with other systems.
- **FluentValidation**: For validating commands and requests.
- **Serilog**: For structured logging.
- **Integration with RabbitMQ and Kafka**: For messaging events across services.
- **Docker and Kubernetes**: Containerized microservices for easy deployment.

## Technologies Used

- **.NET 9**: Backend framework
- **CQRS**: Command Query Responsibility Segregation pattern
- **FluentValidation**: Input validation
- **PostgreSQL**: Database for storing inventory data
- **RabbitMQ & Kafka**: For messaging between microservices
- **Redis**: Caching layer
- **Docker**: Containerization
- **Kubernetes**: Container orchestration
- **Serilog**: Structured logging
- **xUnit**: Unit and integration tests
- **Dapper & Entity Framework**: Database access with Dapper for performance and EF for rich queries

## Getting Started

To run this project locally, follow these steps:

### Prerequisites

1. **.NET SDK** (version 9) - [Install .NET SDK](https://dotnet.microsoft.com/download/dotnet)
2. **Docker** - [Install Docker](https://www.docker.com/products/docker-desktop)
3. **Kubernetes** (optional, for local development) - [Install Kubernetes](https://kubernetes.io/docs/tasks/tools/install-kubectl/)

### Clone the Repository

```bash
git clone https://github.com/marttheus/inventory-management.git
cd inventory-management
```

### Setup Docker and PostgreSQL
This project uses Docker to run PostgreSQL, RabbitMQ, and other services.

1. Ensure Docker is running.
2. Build and start the containers.
```bash
docker-compose up --build
```

This will start the following services:

1. PostgreSQL (db)
2. RabbitMQ (rabbitmq)
3. Kafka (kafka)
4. Redis (redis)

### Configure the Application
The application configuration is in the appsettings.json file. Ensure that the connection strings to PostgreSQL, Redis, RabbitMQ, and Kafka are correct. You can also adjust other settings like logging and messaging configuration.

### Running the Application
Open the solution in Visual Studio, Rider or use the .NET CLI:

```bash
dotnet run --project src/InventoryManagement.API\InventoryManagement.API.csproj
```
The API will be available at http://localhost:5000.

### Running Tests
You can run the unit and integration tests using xUnit.

1. To run the tests from the command line:
```bash
dotnet test
```
2. For specific tests, navigate to the test project and run:
```bash
dotnet test <path-to-test-project>
```

### Contributing
Feel free to fork the repository and submit pull requests. Contributions are welcome! Here’s how you can contribute:

1. Fork the repository.
2. Create a new branch for your feature or bug fix.
3. Make your changes and commit them.
4. Open a pull request to the main branch.

###License
This project is licensed under the MIT License - see the LICENSE file for details.

---
If you have any questions or need further assistance, feel free to reach out via issues or direct messages!