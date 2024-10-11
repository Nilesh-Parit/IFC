# CIVIT_BIMDCR Project

---

## Important Points to be Considered

### RuleTree Hierarchy

**In RuleObject.xml file:**
- RuleSet
  - RuleGroups
    - Rules
      - Filters
        - Parameters
          - Conditions

**On Application after Successful Running:**
- RuleSet
  - DLL Name
    - Rule
      - Model
        - Site -> CheckAllFilters
          - Buildings
            - BuildingStorey
              - Element required according to DLL

---

### RuleSet

Contains:
- Authority
- Agency
- RuleBook

### Filters

- Provides info whether a rule is applicable to an element or not.
- Filters out the element.
- Contains: Set of Ids, Name of filter.

### Parameters

- Contains: Id, Name, Model, Level, Operator, Type, Unit, Value.
- If `parameterType == Required` → Proposed value >= Specified value  
- If `parameterType == Permissible` → Proposed value < Specified value

### ClsCondition

- Contains: Id, Name, Model, Level, Operator, Type (AND, Required), Unit, Value.
- Functions: `CalculateProposedValue()`, `Compare()`

---

## Project Workflow

### Entrypoint Function

When we run the project, the entrypoint is `Startup()` in `BIMDCRIFCApp -> App.xaml.cs` (called dynamically).

1. **Mutex Creation**  
   Used to synchronize process resources.
   
2. **Create Instance of `ClsStartUp` Class**  
   The constructor calls `GetServerPathFromArgs()`, which:
   - Provides the server path if present, otherwise returns an empty string.
   
   - Calls `ClsSession(serverpath)`:
     - If `serverpath` is present:
       - Calls `DownloadDataXMLTemporory(Server_Path)`
     - If empty:
       - Calls `GetFilesFromSelectedProposal(GetBaseDataXMLPath)`  
         `GetBaseDataXMLPath()` → `ClsSession.cs` → `GetFolderPath()`  
         Fetches the Proposal path:  
         `D:\UnnatiK\OneDrive - SoftTech Engineers Ltd\Desktop\DesktopProposalDataFolder`
         
     - `GetFilesFromSelectedProposal` returns a file list from the proposal.
     - Retrieves values like:
       - `CUID = ReadCUID() -> GetCUID()`
       - `CUIDValue = GetCUID()`
       - `PathSettings = GetConfigSettings()` → returns tempDictionary
       - `DesktopFileset = GetDesktopProposalDataFolderPath()` → returns DesktopProposalDataFolder file set (i.e., `Rulemaster.xml`, `RuleObject.xml`, `Data.xml`)

3. **ClsStartUp.Execute()**
   - Creates an instance of `ClsSession`.
   - Checks if the server path exists:
     - If it exists:
       - Calls methods on the instance of `ClsSession`:
         - `CleanRuleObjectXML()`
         - `PrepareAppForDesktopRoute()`
           - Inside, it calls methods like:
             - `DeletePathTXT()`
             - `CreateDownloadedFileStructure()`
             - Processes `DataXml`, `Model`, `RuleObject`, `RuleMaster`
     - If empty:
       - Calls `CleanRuleObjectXML()`
       - Calls `PrepareAppForWebRoute()`

4. **GetActivationFilePath()**
   - Calls `SetFilePath()` of `BM`, `SM`, `BA`, `BS`, `CM`

---

### MainWindow.xaml.cs (Dynamically Called)

- Initializes variables and lists like:
  - `_progress`, `ClsIFCObjectAvailability`, `ClsXBIMIFCDataCheck`, `ClsIFCDataCheck`, `ColTypesId`, `BMTypeId`, `ModelCheckStatus`, `IsScrutinyDone`
  
- `ClsUtils` → Initializes `Log4net.ILog`
- `IClsRuleDB`
- `ScaleTransform`

### MainWindow

1. **MainWindow()**
   - Calls `InitialiseComponent()`
   - Calls `InitialiseSprockets()` ->open UI Application
   - `ViewHandler.CanvasColorSet()`
   
2. **CheckRuleObject()**
   - Retrieves a list of RuleGroup, rule, parameters, conditions.
   
3. **LoadInputFromDataXML()**  
   Calls `LoadModelInitiated()`.
   
4. **OnContentRendering()**  -> Initialise PythonEngine and BeginAllowThread on it.
  - Calls `LoadModelAsync()` → Starts loading models on the application by using DoEvent()
   
  - Calls `PrepareRuleTree_Click()`
         - Calls `PrepareRuleTree()`
         - Calls `GenerateRuleTree()` → Generates the rule tree on the application.
         - Calls `GetProposalInformation()` → Retrieves a list for `ProposalInformationDataGrid`.
  
--------------------------------------------------------------------------------

