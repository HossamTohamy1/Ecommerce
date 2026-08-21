# ECommerce API

Welcome! Imagine you want to build a giant online toy and game store. 

This project is the **brain** behind that online store!  
When someone clicks "Buy", looks at a cool product, or checks out their shopping cart, this program does all the thinking and math behind the scenes.

---

## How the Project is Built (The Cake Layers)

We organized this project using something called **Clean Architecture** (a smart way of organizing code so things don't get messy, just like keeping your toys in labeled boxes).

Think of our project like a 4-layer cake. Each layer has one special job:

```
┌──────────────────────────────────────────────┐
│  1. API Layer (The Frosting / Front Desk)    │
├──────────────────────────────────────────────┤
│  2. Application Layer (The Kitchen / Cooks)  │
├──────────────────────────────────────────────┤
│  3. Domain Layer (The Secret Family Recipe)  │
├──────────────────────────────────────────────┤
│  4. Infrastructure Layer (The Delivery Van)  │
└──────────────────────────────────────────────┘
```

1. **Domain Layer (The Core Rules)**  
   This is the very center of the store. It defines what a "Product", "User", "Order", or "Discount" is. These rules never change, no matter what kind of computer runs them.

2. **Application Layer (The Brain & Workers)**  
   This layer tells the store what to do when an action happens. For example: *"When a customer orders a toy, check if we have it in stock, calculate the total price, and save the order!"*

3. **Infrastructure Layer (The Helpers & Tools)**  
   This layer talks to the outside world. It saves data into the **database** (a giant electronic notebook that never forgets anything) and sends real emails.

4. **API Layer (The Front Door)**  
   This is the **API** (a way for websites and phone apps to talk to our store, like passing notes). It takes requests from the outside world, gives them to the application layer, and sends back the answer.

5. **Shared Layer (The Common Toolbox)**  
   A little helper box with shared tools, error handlers, and words in multiple languages (English and Arabic) that every layer can use.

---

## Cool Tools We Use

Here are the tools (technologies) inside this project:

- **.NET 10 (C#)**: The super-fast engine and programming language we used to write this whole project.
- **ASP.NET Core**: The web toolbox that helps our program listen for visitors on the internet.
- **SQL Server**: Our electronic filing cabinet that keeps all customer and product information safe.
- **Entity Framework Core 10**: The translator that lets our C# code talk to the database without needing complex commands.
- **MediatR**: A traffic cop inside the code that passes messages to the right worker without confusion.
- **FluentValidation**: The rule-checker that makes sure forms are filled out correctly (like checking that prices are never negative numbers).
- **Mapster**: A copy-paste helper that easily transforms data from one shape to another.
- **SignalR**: A walkie-talkie system that sends instant live messages and chat updates to users.
- **Serilog**: A diary keeper that writes down everything that happens so we can spot and fix bugs easily.
- **Razor Pages**: Simple web pages built right into the app for admin dashboards and store management.

---

## Project Folder Map

Here is a quick tour of what is inside the project folders:

```text
src/
│
├── ECommerce.Domain/
│   ├── Entities/        --> The main store items (User, Product, Order, Discount, Review)
│   └── ValueObjects/    --> Special details (like Money or Address)
│
├── ECommerce.Application/
│   ├── Features/        --> All the things users can do (Commands & Queries)
│   ├── DTOs/            --> Lightweight packages of data sent back and forth
│   ├── Interfaces/      --> Contracts and promises describing what tools should do
│   └── Mapping/         --> Rules for transforming data models
│
├── ECommerce.Infrastructure/
│   ├── Persistence/     --> The Database Context (where data gets saved to SQL Server)
│   ├── Migrations/      --> Blueprint history of how the database tables are built
│   ├── Realtime/        --> Instant chat and notification hubs
│   └── Email/           --> The email sending service
│
├── ECommerce.API/
│   ├── Controllers/     --> Web endpoints where mobile apps and websites send requests
│   ├── Pages/           --> Web pages for products, orders, cart, and admin panels
│   ├── Middleware/      --> Security guards checking requests as they come in
│   └── Program.cs       --> The starting button that turns on the whole app
│
└── ECommerce.Shared/
    ├── Common/          --> General helper functions
    ├── Pagination/      --> Helpers for breaking long lists into pages (like 1, 2, 3)
    └── Resources/       --> Translations for multiple languages
```

---

## How to Run This Project (Step-by-Step Recipe)

Follow these steps like a cooking recipe to start the store on your computer!

### Ingredients You Need Before Starting:
1. **.NET 10 SDK** installed on your computer.
2. **SQL Server** (LocalDB, SQL Express, or a remote SQL Server instance).
3. **Visual Studio 2022 / 2025**, **VS Code**, or **JetBrains Rider**.

---ٍِ

### Step 1: Open the Project Folder
Open your command terminal (PowerShell or Terminal) and go to the project folder:
```bash
cd "path/to/ECommerce/src"
```

### Step 2: Check the Database Connection
Open `ECommerce.API/appsettings.json` and look at the `ConnectionStrings` section. Make sure it points to your SQL Server:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=ECommerceDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### Step 3: Restore Packages
Download all the tools and packages the project needs:
```bash
dotnet restore
```

### Step 4: Build the Project
Check that all code compiles without any errors:
```bash
dotnet build
```

### Step 5: Run the Project!
Start up the web server:
```bash
dotnet run --project ECommerce.API
```

When you see messages in the terminal saying the app has started, open your web browser and visit:
- **Web Pages**: `https://localhost:7000` (or the port shown in your terminal)
- **Chat & Notifications**: Live on `/hubs/chat` and `/hubs/notifications`

*(Note: The app will automatically create any missing database tables and load starting sample data the very first time it starts!)*

---

## Testing & Exploring the App

- **Web Pages**: Browse through products, view categories, manage the shopping cart, and see the admin dashboard.
- **Languages**: The app supports English (`en`) and Arabic (`ar`) out of the box!
- **HTTP Requests**: You can also use the included file `ECommerce.API/ECommerce.API.http` inside your code editor to test sending requests directly.

---

## Want to Help? (Contributing)

We love help from friends! If you want to make this project even better:

1. **Fork** this project (make your own copy).
2. **Create a branch** for your new feature (give it a fun name like `feature/magic-discount`).
3. **Write your code** and make sure it builds nicely.
4. **Send a Pull Request** (ask us to review and add your changes)!

---

## License

This project is licensed under the **MIT License** — you are free to learn from it, play with it, and build cool things!
