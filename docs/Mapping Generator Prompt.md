alright, i need help once again with generating an implementation .md file that an ai agent can use to fully flesh out a mapping module.  

You can refer to the prompt text pasted at the end of this prompt (After the line of "========="s) for the methodology we used to produce the EasyAFV3 Development prompt.md file.  This is going to get treated like another sidequest the same way that C:\src\EasyAFv3\app\EasyAF.Shell\Docs\OpenBackstage.prompt.md did.  

We are going to make a module that links into some core libraries that we developed as part of another attempt at this software that didn't adhere to MVVM design at all.  If you look at the sandbox\easyaf-stopgap code, that is a fully working copy of another version of this app that has the base libraries, etc. that we will need to look at.  

Of key importance are the following libraries\projects:
- EasyAFConsole
- EasyAFEngine
- EasyAFExportLib
- EasyAFImportLib
- EasyAFLib

These projects house the core backend data processing that we've built so far.  they aren't perfect and are still being tweaked as we go, but this does plug into the working frontend of EasyAF.UI in the sandbox.  What we need to do is bring these modules into our codebase and ensure they are fully documented with good xml documentation/comments so that we can get intellisense support and ensure we are using things properly.  so the first thing will be an audit of the existing code.  The intent is that these will be DLL files that are linked and could be swapped (not while the program is running) to provide updated functionality.  This is all work that must take place before we can even look at developing our new custom modules of phase 3 in the EasyAFV3 Development Prompt.md file.  

Next, we will need to focus on the actual mapping module that we are tasking ourselves with in this phase of development.  The mapping module has never been implemented before, but it is effectively a module to develop the .ezmap files associated with the EasyAFImportLib.  we need a UI/UX to either drag and drop or load in files that we can scrape to generate associations between data in files like the one being scraped and the classes of a project (reference EasyAFLib).  

I had envisioned:
- The mapping modules document interface would be a multi-tab canvas.
- The first tab of the map document would be a summary/statistics page that would have the following:
  - Fields for the map name, EasyPower Software Version, Date Modified, and an optional long text description.
  - A list of the referenced files that has behavior similar to our quick access list in the open backstage of the shell app
  - A table of Properties for the map: data types (with clean/dirty adornment red/green/blue), fields available, fields mapped, a column of listboxes that list the available tables found so we can select the table to scrape for each tab
- All subsequent tabs would be dynamically generated for each datatype.  These tabs would:
  - have some kind of interface, possibly drag and drop, or a pair of lists.  I am asking you for guidance on a good UX for data association like this - there could be up to 100 data types to map in an absolute worst case.
  - the user will make associations between elements
  - if the user happens to change the associated table on the first tab, then the associated data type tab will reset (only if user agrees to a confirmation.  if user refuses, then the table association reverts)

I would like some kind of AI/fuzzy/heuristic approach to auto-associate values.  i would even like the module to be able to reference other maps to see if things are similar.  The reason we have to have a mapping module in the first place is that the software that produces the data being mapped to our classes has a tendency to revise column names or add/remove spaces from the column names from version to version.  I am trying to make it easy on our users to build maps so they don't have to manually corelate data all the time.

Additional polish/UX features I want to have:
- undo/redo - being able to undo/redo mapping assignments and table assignments would be wonderful.
- I would like some way to see some sample data, perhaps we have a table at the bottom of the document in a buttongroup called "Sample Data" that displays mapping corelation for one or two rows of data from the file being used to generate the map.
- the saved document is the .ezmap, so we need to make whatever serialization, etc. related to the base library class work.  


OK, this is a lot, so the question is: can you generate the file that I can save and share with other, lesser AI agents to implement?

=====================================================================
OLD PROMPT USED TO GENERATE EASYAFV3 DEVELOPMENT PROMPT.MD
=====================================================================

+Request:
>can you take a good hard look at only the code in the src folder and the prompts in the prompts folder for me?  I tried making a desktop application that got a little bit too fragmented and unwieldy to manage.  with the new release of visual studio insiders (v2026), I am wanting to take another crack at creating my program.  I need you to generate a thorough, thoughtful, and comprehensive .prompt.md file with a section per major element of the application.  The intent is that you will help me leverage the new visual studio AI agents by creating a good prompt for them to build a vibe-coded application from.  

>I'll give a summary of my vision for the different components, but it is far from exhaustive of all the different layers there are to creating a full-fledged desktop application.  There are some lessons that I have learned as the program was developed that I will try to spell out in this prompt.  I need you to help me steer the development to avoid the issues we've encountered thusfar.  

+My thought process:
>I need to start with a container application that i can use as a host, editor, and general interface for various document types that i create.  each document type should be its own module.  the goal is that i can have a multi-document interface, with a tabstrip on the left or right side of the application window that hosts the documents.  the reasoning for a multiple document interface is that i eventually want to be able to run batch jobs.
>The previous iterations of the program became unwieldy when trying to integrate multiple modules.  A lesson learned from preivous attempts: small edits on certain modules could cause breaking actions to occur on other modules.  we need to be very militant about maintaining modularity and keep the edit scope very focused.
>While the intent is to be extremely modular and rigid with how we make edits, I have learned from previous attempts at this that there WILL be instances where we need to make targeted edits of other modules/classes to achieve a desired outcome.  It is imperative that we document these instances very specifically and clearly in the code comments of the classes being cross-edited so that we can keep track of these exceptions to the modularity rule and use them as a tool if we need to backtrack or trace issues later on.
>When you create the prompt for me, I want you to put extreme emphasis on one rule for the AI agents that will be building the application as we vibe-code.  The rule is this: "follow the targeted list and do not break down tasks into sub-tasks in the prompt."  The reasoning for this is that in previous attempts, the AI agents would often break down tasks into sub-tasks that would lead to scope creep and the direction of coding would fragment/fracture.  By keeping the tasks focused and targeted, we can ensure that the AI agents stay on track and deliver the desired outcomes without getting sidetracked by unnecessary details.  Because of this, you need to be very careful about how you word the tasks in the prompt to ensure that they are clear, concise, and focused on measurable steps towards the desired outcome without inviting unnecessary breakdowns.
>There will inevitably be instances where we need to revisit tasks because of unforseen complications or necessary changes.  When this happens, we need to note the current task as paused waiting to resume, and reopen the previous task that we are modifying.  please have the AI agents keep a journal of which tasks we are working on and their current status as they develop.  We need this to be kept in the same prompt file as the one you are generating in a dedicated journal section.  the purpose of the journal is to give me some history and to allow future AI agents to pick up where we left off if needed and understand the context of what we were working on.

+General Development comments:
> The application is first and foremost a data processing utility.  It will effectively take in multiple output files that are produced by a third-party application, parse them, and then allow the user to manipulate and analyze the data in various ways.  The end goal is to be able to export the processed data into various formats, such as reports, labels, and tables that can be submitted as part of an engineering study.  The application should be designed with extensibility in mind, as we may want to add new data processing modules in the future.  Each module should be self-contained and able to operate independently of the others, while still being able to share data and resources as needed.
>The program should be a WPF application, utilizing the very latest version of the fluent ribbon library.  the fluent ribbon is an excellent resource for a ribbon-based application and we have had success in the current development path, with some tweaking.
>The entire application needs to be fully themable through a centralized theming library/engine.  I want a dark mode and a light mode to start with.  each theme will have its own set of brushes that will need to override the visual representation of EVERY item in the application.  A lesson learned in our current/past iterations was that theming can be very challenging because there are a lot of different ways that controls can be themed.  we need to be very selective about applying our theme brushes to the correct elements.  Centralized theming is going to be a necessity because we want to be able to add new modules in the future and have them automatically pick up the current theme without any additional work.
>In previous and current iterations, we have used the Prism library for displaying different views and modules within the application.  I am fine with continuing to use Prism, but we need to be very careful about how we structure our modules and their interactions.  
>We want to adhere to MVVM principles as closely as possible to ensure that our codebase remains maintainable and testable.  This means we need to have a clear separation between our views, view models, and models.  We should also leverage data binding and commands to minimize code-behind in our views.  This seems like a great idea, but a lesson learned is that we suffered from a lot of code-behind creeping into our views in previous iterations.  We need to be vigilant about keeping our views clean and free of logic.  We also need to make sure we have a solid understanding of how to use data binding effectively to avoid unnecessary complexity.
>We need to have a centralized logging engine that can be used by all modules and components of the application.  logging should be configurable to log to different outputs (console, file, etc) and have different log levels (info, debug, error, etc).  this will help with debugging and maintaining the application as it grows in complexity.
>We need to have a robust settings management system that allows for easy storage and retrieval of application settings.  this should include user preferences, module-specific settings, and global application settings.  the settings should be easily accessible and modifiable through a dedicated settings UI within the application -- preferably accessible from the ribbon.  The settings interface should be a popup dialog with a tab-driven interface with an application-specific tab and a tab for each module that is installed.  the popup should be resizable and allow scrolling if the page extends beyond the limits of the popup window.
>Our code needs to be thoroughly documented, with clear comments and documentation for each class, method, and property.  this will help with maintainability and onboarding new developers to the project.  My intent is that this code base can be maintained for years to come.
>The EasyAF namespace in the src folder is used for libraries that were developed independently of the main application.  These libraries should be treated as separate projects that can be versioned and maintained independently of the main application.  We need to ensure that these libraries have their own documentation and versioning system to avoid confusion.

+Container App thoughts:
>The application should be able to host multiple modular filetypes, each fully encapsulated in its own module definition that operates fully within the document space of the container app.  The modules should have their own ribbon tabs ready to go that are dynamically displayed on the container app's ribbon as if they are part of the container app.
>The container app should have a tabstrip on the left or right side of the window that allows for easy navigation between open documents.  Each document should be represented by a tab that displays the document's name and an icon representing its type.  The user should be able to easily switch between documents by clicking on the tabs.
>The container app should have a robust file management system that allows for easy opening, saving, and closing of documents.  This should include support for common file operations such as drag-and-drop, recent files, and file associations.
>In the current implementation, the container app functions as if it were a web browser, with each document being reloaded on open.  This seems like it has led to some unnecessary complexity and performance issues.
>As a lesson learned, the intent is that we will have multiple documents open at once, and we want to be able to switch between them quickly and easily.  We need to ensure that the document management system is robust and can handle multiple open documents without performance degradation.  This may mean increased memory usage, which we need to be cognizant of, but the tradeoff for performance is worth it.  One issue we had in the current iteration is that the document state was not always preserved when switching between documents, leading to data loss and user frustration.  We need to ensure that the document state is always preserved and that users can pick up exactly where they left off when switching between documents.
>Since modules can be added in the future, we need to make sure that the core application is not hardcoded to any specific module or file type.  This means that each module will need to be fairly self-sufficient and able to register itself with the container app dynamically.  This will allow for easy addition of new modules in the future without requiring changes to the core application.

+Module thoughts:
>Each module should be fully encapsulated and able to operate independently of the others.  This means that each module should have its own view, view model, and model, as well as its own ribbon tab and commands.
>The modules should be designed with extensibility in mind, allowing for easy addition of new modules in the future.  This means that we need to have a clear and consistent module interface that all modules adhere to.
>Each module should be able to share data and resources with other modules as needed.  This means that we need to have a centralized data management system that allows for easy sharing of data between modules.
>Since the overall container application is intended to be a sandbox, we need some way of registering interactions and interoperability between modules with the contrainer app.  The goal is to allow modules to communicate with each other and the container app without creating tight coupling between them.  This may involve using an event aggregator or message bus to facilitate communication between modules.
>We need to ensure that each module adheres to MVVM principles and minimizes code-behind in its views.  This will help with maintainability and testability of the modules.
>Some modules will be self-contained multi-tab interfaces, while others will be single-view editors.  We need to ensure that the module architecture can accommodate both types of modules without unnecessary complexity.
>We created a custom control element, the "DiffGrid" in the current iteration.  The raw control is great, but the implementation became very clunky and difficult to manage through vibe-coding.  I want to keep the general idea of that control, but we need to rethink how it is implemented to make it more manageable and easier to vibe-code.

+Module: Map (MapEditor)
>This is currently best described as the MapView in the current implementation.  The Map module is intended to be a multi-tab interface that allows users to view and manipulate data mapping associations.  The module should have its own ribbon tab with commands specific to map editing and analysis.
>The MapEditor should allow the user to load in sample files that contain mapping data.  The module should strip out the data headers and allow the user to create associations between columns and data types.  The user should be able to save and load mapping configurations for future use.
>The MapEditor should provide a visual representation of the mapping data, allowing users to easily see and understand the associations they have created.  This may involve using a grid or table view to display the data, as well as visual indicators for different data types.
>Since the module is mapping real-world data to application-specific data types, we need to ensure that the classes being mapped are dynamically processed instead of being hardcoded.  this is because we may add or remove properties from the classes in the future, and we want to ensure that the mapping process remains flexible and adaptable.
>We need robust validation and handling of tables in the MapEditor.  For example, we should only be able to map a specific table once.  We need to use a pick-without-replacement approach when mapping fields to ensure that the user does not accidentally create duplicate mappings.  Additionally, we need to provide clear feedback to the user when they attempt to create invalid mappings or when there are conflicts in the mapping data.

+Module: Project (ProjectEditor)
>The ProjectEditor is best described by the ProjectView in the current implementation.  
>The Project module is intended to be a multi-tab editor that allows users to manage and analyze project data.  The module should have its own ribbon tab with commands specific to project management and analysis.
>The tabs within the ProjectEditor should include:
  - Overview Tab: Provides a summary of the project, including key metrics and data visualizations.  This tab will also have fields where the user can input project metadata such as project name, description, date, and other relevant information that will be mapped to the output report.
  - Project Tab: Allows users to view and manipulate the raw project data, including filtering and sorting capabilities.
  - Data Tabs: These will be dynamically generated based on the data types present in the project.  Each data tab will provide a detailed summary and tabular view of a specific data type, allowing users to analyze and manipulate the data as needed.  It will be a container with some summary information at the top and a diffgrid below for detailed analysis.
  - Options tab: Provides various settings and options for the project, including data processing options, mapping, reporting, and other export settings.  This tab should always be the last tab in the document.
>Ribbon tabs for a project should include:
  - Data Management: Commands for importing, exporting, and managing raw project data.
  - Output: Commands for generating and exporting reports and labels based on the project data.
  - Analysis: Commands for performing various data analyses on the project data.  This includes generating diff reports, summary statistics, and other relevant analyses that can be exported.
>The project module will store default specs for reports and labels that can be applied to the project data.  This will allow users to quickly generate standard reports and labels without having to define them from scratch each time.  
>Labels are basically a specialized report.  Where the report will populate a large table from entire datasets, the label will use the same spec pipeline, but to populate many instances of the specified table for each row of data in the dataset.

+Module: Spec (SpecEditor)
>The SpecEditor is currently represented by the SpecView in the current implementation.  The Spec module is intended to be a multi-tab editor that allows the users to manage and define table outputs for reports.  We have a fairly robust implementation of the spec class in the EasyAFEngine library that we can leverage for this module.  The module should have its own ribbon tab with commands specific to spec management and table definition.
>The tabs within the SpecEditor should include:
  - Table Definition Tabs: These will be dynamically generated based on the tables defined in the spec.  Each table definition tab will provide a detailed interface for defining the structure and content of a specific table, allowing users to graphically create and manipulate columns, data types, formatting options, and other relevant settings.
>Ribbon tabs for a spec should include:
  - Table Management: Commands for creating, editing, and deleting table definitions.  This includes adding new tables (tabs) to the spec as well as removing existing ones.
  - Column Management: Commands for adding, removing, and configuring columns within table definitions.
>The spec should be able to differentiate whether it is intended for use with a report or a label.  This will affect how the spec is applied to the project data and how the output is generated.

