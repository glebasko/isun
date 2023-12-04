## Overview

The Weather Forecaster Console Application is a command-line tool that provides weather forecasts for specified cities. It interacts with a weather forecast API and stores the retrieved data in a database.


### Built With

* .NET 8
* Microsoft SQL Server


## Solution Structure

Application consists of 5 individual projects (layers):

* **WeatherForecaster.ConsoleUI**: entry point of the application. Start the application and print outs the results to the console.
* **WeatherForecaster.Domain**: domain layer of the application. Contains DTOs, interfaces and services.
* **WeatherForecaster.Infrastructure**: infrastructure layer of the application. Contains a helper class to build the configuration file alongside with logging.
* **WeatherForecaster.Persistance**: persistance layer of the application. Contains the EF Core DbContext, Entities, Migrations, Repositories and everything that is related to the database access.
* **WeatherForecaster.Tests**: contains integration and unit tests


<!-- GETTING STARTED -->
## Getting Started

### Prerequisites

* Database connection (configured in appsettings.json)
* API base URL (configured in appsettings.json)

### Running the application

1. Open a terminal.
2. Navigate to the project (.exe file) directory.
3. Run application by typing: the following command:
  ```sh
  isun city1, city2, city3
  ```
  or
  ```sh
  isun city1 city2 city3
  ```

Replace city1, city2, etc., with the names of the cities from the API:
* Vilnius
* Klaipėda
* Riga
* Tallinn
* Rome
* Berlin
* Paris
* Tunis
* Oslo
* N'Djamena"

<!-- CONTACT -->
## Contact

glebas.kovalenka@gmail.com
