# Fitness Tracker (WinForms + EF Core)

<img width="800" height="623" alt="image" src="https://github.com/user-attachments/assets/d3135d34-389e-4bb5-9553-16b2ee7bc84e" />

---

## Структура проєкту

- `Program.cs`  
  Точка входу застосунку, запускає `Form1`.

- `Form1.cs`  
  Основна логіка форми та побудова інтерфейсу. Містить вкладки:
  - Manage
  - ListView
  - DataGridView

  Також містить CRUD-обробники та метод `RefreshData()` для синхронізації даних.

- `Form1.Designer.cs`  
  Автоматично згенерований дизайнером шаблон форми.

- `FitnessActivity.cs`  
  Модель сутності:
  - `FitnessActivityId`
  - `Title`
  - `Duration`
  - `CaloriesBurned`
  - `Intensity`
  - `Date`

- `FitnessDbContext.cs`  
  EF Core `DbContext`, який містить:
  - `DbSet<FitnessActivity>`
  - метод `OnConfiguring()` для підключення до LocalDB

- `winforms_ef_crud.csproj`  
  Файл конфігурації проєкту, який використовує `.NET Framework 4.7.2`.

---

## Принцип роботи

- Під час запуску застосунок перевідтворює базу даних LocalDB за допомогою:
  ```csharp
  EnsureDeleted();
  EnsureCreated();
  ```

- Після створення автоматично додаються тестові дані.

- Метод `RefreshData()`:
  - завантажує сутності у `BindingList<FitnessActivity>`
  - встановлює список як `DataSource` для:
    - `ListBox`
    - `ComboBox`
    - `DataGridView`
  - вручну заповнює `ListView`

- CRUD-операції:
  - Додавання
  - Оновлення
  - Видалення

  змінюють `DbContext` та викликають `RefreshData()`, щоб усі елементи інтерфейсу залишались синхронізованими.

