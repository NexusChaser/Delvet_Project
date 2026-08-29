# Unity Programming Guidelines & Preferences

These guidelines define the coding standards, architecture, and project structure for Unity projects.

## 1. Project Structure & Organization
- **Root Folder**: Never place assets directly in the root `Assets` folder.
- **Main Folder**: Create a `ProjectAssets` folder. All project-specific content goes here.
- **Script Separation**: 
  - `ProjectAssets/Scripts/`: Contains standard Monobehaviours and logic scripts, organized by domain (e.g., `Core`, `Utilities`, `Player`, `UI`, `AudioSystem`).
  - `ProjectAssets/ScriptableObjects/`: Contains scripts defining ScriptableObjects AND the asset instances themselves. Group them by feature (e.g., `ProjectAssets/ScriptableObjects/AudioConfig/` containing `AudioConfig_SO.cs` and the created asset files).

## 2. Naming Conventions & Namespaces
- **Language**: All variables, methods, and comments MUST be in English.
- **Namespaces**: Always use namespaces reflecting the folder structure, starting with the project name. (e.g., `namespace Delvet_Project.Player.Movement { ... }`).
- **Private Variables**: `camelCase` (e.g., `count`). **DO NOT** use underscores (`_count`).
- **Properties (Getters/Setters)**: `PascalCase` (e.g., `Count`).
- **Protected Variables**: `camelCase` (e.g., `count`).
- **ScriptableObjects**: Suffix class names and files with `_SO` (e.g., `AudioConfig_SO`).

## 3. Variables, Inspector Visibility & Tooltips
- **No Public Variables**: Do not use `public` fields for Inspector exposure.
- **Serialization**: Use `[SerializeField] private` for variables that need to be visible in the Inspector.
- **Tooltips**: Always use `[Tooltip("...")]` on serialized fields that require clarification of their function.
- **Pure Private**: Use `private` (without `[SerializeField]`) only for internal state that should never appear in the Inspector.
- **Protected Serialization**: Use `[SerializeField] protected` if a subclass needs access and it must be visible in the Inspector.
- **Properties**: Use explicit getters and setters instead of auto-properties when backing fields are needed.
  ```csharp
  [SerializeField] private int count;
  public int Count
  {
      get
      {
          return count;
      }
      set
      {
          count = value;
      }
  }
  ```

## 4. Regions and Script Layout
- **Regions Everywhere**: Use `#region` to organize every part of the script, including variables, Unity lifecycle methods, and specific feature methods (e.g., `#region Movement Methods`, `#region Input Methods`).
- **Header Order**: Headers in the Inspector must follow this strict order:
  1. `[Header("References")]`: Must always be at the very top.
  2. `[Header("Custom Settings")]`: Any other specific script configuration headers go here (e.g., Movement Settings, Health Settings).
  3. `[Header("Debug Settings")]`: Placed below all other configuration headers.
  4. `[Header("Gizmos Settings")]`: Placed at the very bottom.
- **Structure Example**:
  ```csharp
  #region Variables
  [Header("References")]
  [SerializeField, Tooltip("The main Rigidbody of the player.")] 
  private Rigidbody rb;

  [Header("Movement Settings")]
  [SerializeField, Tooltip("The movement speed in units per second.")] 
  private float speed;

  [Header("Debug Settings")]
  [SerializeField] private string logPrefix = "[MyScript]";
  [SerializeField] private bool showDebugLogs = true;

  [Header("Gizmos Settings")]
  [SerializeField] private bool showGizmos = true;
  [SerializeField] private bool alwaysShowGizmos = false;
  #endregion
  ```

## 5. Component References & Initialization
- **No Direct GetComponent**: Never rely solely on `GetComponent`. Assign references in the Inspector.
- **Awake Fallback**: In `Awake()`, validate references. If a reference is null, attempt `GetComponent`. If it still fails, log a red error (`Debug.LogError`).
  ```csharp
  #region Unity Lifecycle Methods
  // Initialize component references
  private void Awake()
  {
      if (rb == null)
      {
          rb = GetComponent<Rigidbody>();
          if (rb == null)
          {
              Debug.LogError(logPrefix + " Reference to Rigidbody not found!");
          }
      }
  }
  #endregion
  ```

## 6. Debugging and Logging
- **Log Prefix**: Include a string variable `logPrefix` defaulting to `"[ScriptName]"`. Use this for all logs and make it editable in the inspector.
- **Debug Toggle**: Include a boolean `showDebugLogs` to toggle console output.
- **String Concatenation**: Use `+` for string concatenation in logs. **DO NOT** use string interpolation (`$"{var}"`).
- **Log Types**:
  - `Debug.Log()`: Use for normal, correct functioning and information.
  - `Debug.LogWarning()`: Use for potential dangers, weird behaviors, or to give a heads-up.
  - `Debug.LogError()`: Use for actual errors, such as a missing component reference or a fatal failure.

## 7. Gizmos Settings
- **Visibility Toggles**:
  - `showGizmos`: Boolean to completely enable or disable Gizmos.
  - `alwaysShowGizmos`: Boolean. If true, use `OnDrawGizmos` (always visible). If false, use `OnDrawGizmosSelected` (only visible when object is selected).

## 8. Loops
- **Prefer `for`**: Always default to using `for` loops.
- **Avoid `foreach`**: Only use `foreach` when strictly necessary and logically useful.

## 9. Unity Methods Execution Order
- **Using Directives**: Only include necessary `using` directives at the top to avoid loading unused namespaces. Do not use fully qualified class names inline (e.g., avoid `System.Collections.IEnumerator`, put `using System.Collections;` at the top).
- **Execution Order**: Place Unity methods in this exact order.
- **Descriptive Comments**: Add a comment above **every single** Unity method explaining exactly what it handles in this specific script (e.g., if `Start` initializes the health system, write `// Initializes the health system`).
- **Keep it Clean**: The methods below are just examples. **In the final code, only include the Unity lifecycle methods that you are actually using.** Delete the rest.
  ```csharp
  #region Unity Lifecycle Methods
  // Initialize component references
  private void Awake() { }
  
  // Subscribe to events
  private void OnEnable() { }
  
  // Unsubscribe from events
  private void OnDisable() { }
  
  // Initializes the health system
  private void Start() { }
  
  // Handles the state machine updates and generic frame logic
  private void Update() { }
  
  // Handles physics-based movement and collisions
  private void FixedUpdate() { }
  #endregion
  ```

## 10. Input System
- **Use New Input System**: Avoid the legacy input system.
- **No C# Events for Input**: Do not subscribe to Input System via standard C# events.
- **CallbackContext Methods**: Create explicit methods that receive `InputAction.CallbackContext`.
- **Naming**: Prefix these methods with `On` and suffix with `Input` (e.g., `OnJumpInput`).
- **Context States**: Checking for `context.performed` is just an example. Use `performed`, `canceled`, `started`, etc., depending on the specific behavior required by the input action.
  ```csharp
  #region Input Methods
  // Processes jump input from the new Input System
  public void OnJumpInput(InputAction.CallbackContext context)
  {
      // Note: This is an example, use performed, canceled, started, etc. based on needs.
      if (context.performed)
      {
          // Jump logic
      }
  }
  #endregion
  ```
