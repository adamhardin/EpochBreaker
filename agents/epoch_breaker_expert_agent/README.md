# Epoch Breaker Expert Agent

This agent provides expert game design reviews for Epoch Breaker using the project's competency framework.

## Prerequisites

- Python 3.10+
- Microsoft Agent Framework (pinned)
- Microsoft Foundry project and deployed model

## Setup

1) Create a virtual environment and install dependencies:

```
python -m venv .venv
source .venv/bin/activate
pip install -r requirements.txt
```

2) Configure environment variables:

```
cp .env.example .env
```

Edit .env and set:
- FOUNDRY_PROJECT_ENDPOINT
- FOUNDRY_MODEL_DEPLOYMENT_NAME

3) Run the agent server:

```
python agent_server.py
```

The server starts in HTTP mode using Agent Server. Use the AI Toolkit Agent Inspector to connect.

## Debugging

If you want local debugging and Agent Inspector integration, install:

```
pip install debugpy
pip install agent-dev-cli --pre
```

Then run:

```
python -m debugpy --listen 127.0.0.1:5679 -m agentdev run agent_server.py --verbose --port 8087
```

## Notes

- This agent expects supporting artifacts defined in docs/Reviewer-Competency-Templates.md.
- If results are missing, the agent will report incomplete competency evaluations.
