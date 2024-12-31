# PromptSpark Chat Workflow
An ASP.NET Core and SignalR-based solution for building dynamic, structured chat workflows that incorporate Adaptive Cards and optional AI integration. Includes real-time conversation features, simple concurrency management, and flexible workflow logic, making it easy to guide users through customized multi-step processes with minimal overhead.
A conversational workflow application built with ASP.NET Core, SignalR, and Adaptive Cards. This repository demonstrates how to guide users through structured steps, present interactive UI elements, and respond to freeform conversation using AI or custom logic.

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
1. **Clone** the repository to your local machine.
2. **Configure** the app settings for SignalR and any AI endpoints you wish to integrate.
3. **Run** the application in Visual Studio or through the .NET CLI.
4. **Access** the site locally to begin testing.

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
