import asyncio
import os
from pathlib import Path

from agent_framework.azure import AzureAIClient
from azure.ai.agentserver.agentframework import from_agent_framework
from azure.identity.aio import DefaultAzureCredential
from dotenv import load_dotenv


def _load_instructions() -> str:
    instructions_path = Path(__file__).parent / "INSTRUCTIONS.md"
    return instructions_path.read_text(encoding="utf-8")


def _require_env(name: str) -> str:
    value = os.getenv(name)
    if not value:
        raise ValueError(f"Missing required environment variable: {name}")
    return value


async def main() -> None:
    load_dotenv(override=True)

    project_endpoint = _require_env("FOUNDRY_PROJECT_ENDPOINT")
    model_deployment_name = _require_env("FOUNDRY_MODEL_DEPLOYMENT_NAME")
    instructions = _load_instructions()

    async with DefaultAzureCredential() as credential:
        client = AzureAIClient(
            project_endpoint=project_endpoint,
            model_deployment_name=model_deployment_name,
            credential=credential,
        )

        async with client.create_agent(
            name="EpochBreakerExpertAgent",
            instructions=instructions,
        ) as agent:
            await from_agent_framework(agent).run_async()


if __name__ == "__main__":
    asyncio.run(main())
