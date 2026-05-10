# Contributing Guidelines

## Coding Standards
1. Use Dependency Injection (DI) and interface-based design to decouple components.
2. Abstract File I/O, UI components (like MessageBox), and external dependencies behind interfaces to enable unit testing and mocking.
3. Keep business logic separate from Windows Forms UI code.

## Testing Standards
1. **Framework:** Use **NUnit** for unit and integration testing.
2. **Coverage:** Aim for high code coverage for all business logic (Models, Services, Managers).
3. **Mocking:** Use libraries like **Moq** to mock external dependencies (e.g., File System, UI Dialogs, Databases).
4. **Integration Tests:** Ensure realistic interactions between components using test doubles or tailored datasets where appropriate.
5. **Testability Refactoring:** Code should be refactored to support testability without breaking existing behavior.