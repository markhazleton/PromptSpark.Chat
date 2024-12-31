# PromptSpark Chat Workflow

**A real-time, conversational workflow application built with ASP.NET Core, SignalR, and Adaptive Cards. It demonstrates how to guide users through multi-step processes, present interactive UI elements, and optionally integrate AI-driven responses—all while providing flexible workflow logic, simple concurrency management, and minimal overhead.**

## Table of Contents
- [Overview](#overview)
- [Features](#features)
- [Technologies](#technologies)
- [Getting Started](#getting-started)
- [Usage](#usage)
- [Contributing](#contributing)
- [License](#license)
- [References](#references)

## Overview
PromptSpark Chat Workflow provides a dynamic chat interface where users:
- Enter a name
- Choose from a list of workflows
- Interact with adaptive cards or text responses
- Progress through branching logic driven by workflow nodes

All conversation data is stored server-side for continuity, so users can refresh the page without losing their place.

## Features
- **Adaptive Cards** for interactive input
- **Real-time Communication** with SignalR
- **Workflow Persistence** in memory using a thread-safe store
- **AI Integration** to handle questions not covered by the workflow

## Technologies
- **ASP.NET Core** for the main web application framework
- **SignalR** for real-time client-server communication
- **Adaptive Cards** for rendering structured inputs
- **Concurrent Dictionary** for storing conversation state
- **Optional AI** functionality via chat completion services

## Getting Started

Follow these steps to set up, build, and run the **PromptSpark.Chat** application. If you have any issues or questions, please create a GitHub Issue or submit a Pull Request.

### Prerequisites

- **.NET SDK 7.0+** (or the latest LTS version)  
  You can check your version by running `dotnet --version` in your terminal.

### Clone the Repository

**1.** Navigate to the folder where you want to store the project.  
```bash
cd path/to/directory
```

**2.** Clone the repository:  

```bash
git clone
```

**3.** Change into the cloned directory:  

```bash
cd PromptSpark.Chat
```

### Project Structure Overview

```
PromptSpark.Chat/
├─ PromptSpark.Chat.csproj
├─ Controllers/
├─ Models/
├─ Services/
├─ Views/
│  ├─ Chat/
│  │  └─ chat.cshtml
│  ├─ Shared/
│  └─ Home/
├─ wwwroot/
├─ appsettings.json
├─ LICENSE
└─ README.md
```

- **Controllers/** – Houses your ASP.NET Core MVC controllers.  
- **Services/** – Contains core business logic and workflow/AI integration classes.  
- **Views/** – Razor views for the UI (`chat.cshtml`).  
- **wwwroot/** – Static content, such as CSS and JavaScript files.  
- **appsettings.json** – Configuration details.  
- **LICENSE** – MIT License file.  
- **README.md** – Documentation.

### Configure Workflows

If you want to add a custom workflow, create a JSON file (e.g., `myWorkflow.json`), then make it known to the application’s workflow loading logic:

[PLACEHOLDER FOR JSON EXAMPLE]

```json
{
  "nodes": [
    {
      "answers": [
        {
          "nextNode": "chooseStrength",
          "response": "Yes",
          "system": "Coffee Assistant"
        },
        {
          "nextNode": "end",
          "response": "No",
          "system": "Coffee Assistant"
        }
      ],
      "id": "start",
      "questionType": 0,
      "question": "Welcome! Ready to brew the perfect cup of coffee? Shall we start?"
    }
  ]
}

```


> **Note:** The `QuestionType` values or node structure may vary. Refer to your `Node` model for valid properties.

### Running the Application

1. **Using Visual Studio**  
   - Open the solution in Visual Studio.  
   - Press `F5` or use the “Start Debugging” option.  
   - The application will launch in your browser.

2. **Using the .NET CLI**  
   - Restore and build the project, then run it.  
   [PLACEHOLDER FOR CLI COMMAND BLOCK]

   - Check the output for the local URL (e.g., `https://localhost:5001`).  
   - Open your browser and navigate to that URL.

### Verification & Usage

1. **Open the Chat UI:**  
   Enter your name and select a workflow from the dropdown.

2. **Interact with Adaptive Cards:**  
   You’ll see interactive elements (buttons, text inputs). Submit these and let the system guide you to the next step.

3. **Refresh Persistence:**  
   The conversation is stored server-side. Reloading the page should preserve your progress, provided you have the same conversation ID.

### Example Interaction

[PLACEHOLDER FOR STEP-BY-STEP EXAMPLE OR GIF]

1. **Name Input** – Type a name.  
2. **Workflow Selection** – Choose a workflow file (like `myWorkflow.json`).  
3. **Chat Exchange** – Complete the prompts with button clicks or typed responses.

### Troubleshooting

- **Port Conflicts:** Update your `launchSettings.json` to use different ports if needed.  
- **Missing Dependencies:** Ensure you have the correct .NET SDK installed and run `dotnet restore`.  
- **AI Integration:** Update `IChatCompletionService` settings in `appsettings.json` for any required keys/endpoints.

### Next Steps

- **Deploy**: Package and host on Azure, AWS, or an on-prem server.  
- **Scale**: Use Azure SignalR Service if your user load is high.  
- **Test**: Integrate a testing framework and set up GitHub Actions for CI/CD.  
- **Extend**: Add more complex workflows and branching logic as your application grows.

## Usage
Once deployed, users can:
1. Enter their name.
2. Select a workflow.
3. Follow prompts in the chat window.
4. Submit answers through text inputs or button options.
5. Receive immediate responses and progress to subsequent steps.

## Contributing
Contributions are welcome! Please open issues for feature requests or bug reports. For larger changes, submit a pull request with a clear description of what you’ve implemented.

## License
This project is licensed under the [MIT License](LICENSE). You are free to use, modify, and distribute the code as described in the license.

## References
- [ASP.NET Core Docs](https://learn.microsoft.com/aspnet/core)
- [SignalR Introduction](https://learn.microsoft.com/aspnet/core/signalr/introduction)
- [Adaptive Cards Documentation](https://docs.microsoft.com/adaptive-cards/)
