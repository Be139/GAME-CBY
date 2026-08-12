#!/usr/bin/env python3
"""Small project-local client for the configured MCP for Unity HTTP server."""

from __future__ import annotations

import argparse
import json
import sys
import urllib.error
import urllib.request


ENDPOINT = "http://127.0.0.1:8080/mcp"


def load_json_argument(value: str) -> object:
    """Load inline JSON or @path JSON without fragile shell escaping."""
    if value.startswith("@"):
        with open(value[1:], "r", encoding="utf-8") as handle:
            return json.load(handle)
    return json.loads(value)


def decode_payload(raw: bytes) -> object:
    text = raw.decode("utf-8", errors="replace").strip()
    if not text:
        return {}
    if text.startswith("event:") or "\ndata:" in text:
        payloads: list[object] = []
        for line in text.splitlines():
            if line.startswith("data:"):
                payloads.append(json.loads(line[5:].strip()))
        for payload in reversed(payloads):
            if isinstance(payload, dict) and "id" in payload:
                return payload
        return payloads[-1] if payloads else {}
    return json.loads(text)


class UnityMcpClient:
    def __init__(self) -> None:
        self.session_id = ""
        self.request_id = 0

    def post(self, payload: dict, expect_reply: bool = True) -> object:
        data = json.dumps(payload).encode("utf-8")
        headers = {
            "Content-Type": "application/json",
            "Accept": "application/json, text/event-stream",
        }
        if self.session_id:
            headers["mcp-session-id"] = self.session_id
        request = urllib.request.Request(
            ENDPOINT,
            data=data,
            headers=headers,
            method="POST",
        )
        try:
            with urllib.request.urlopen(request, timeout=120) as response:
                if not self.session_id:
                    self.session_id = response.headers.get("mcp-session-id", "")
                raw = response.read()
        except urllib.error.HTTPError as error:
            body = error.read().decode("utf-8", errors="replace")
            raise RuntimeError(f"HTTP {error.code}: {body}") from error
        return decode_payload(raw) if expect_reply and raw else {}

    def initialize(self) -> object:
        self.request_id += 1
        result = self.post(
            {
                "jsonrpc": "2.0",
                "id": self.request_id,
                "method": "initialize",
                "params": {
                    "protocolVersion": "2025-03-26",
                    "capabilities": {},
                    "clientInfo": {
                        "name": "hearth-ui-verifier",
                        "version": "1.0",
                    },
                },
            }
        )
        self.post(
            {
                "jsonrpc": "2.0",
                "method": "notifications/initialized",
                "params": {},
            },
            expect_reply=False,
        )
        return result

    def request(self, method: str, params: dict | None = None) -> object:
        self.request_id += 1
        return self.post(
            {
                "jsonrpc": "2.0",
                "id": self.request_id,
                "method": method,
                "params": params or {},
            }
        )


def main() -> int:
    parser = argparse.ArgumentParser()
    subparsers = parser.add_subparsers(dest="command", required=True)
    subparsers.add_parser("list")
    schema = subparsers.add_parser("schema")
    schema.add_argument("names", nargs="+")
    resource = subparsers.add_parser("resource")
    resource.add_argument("uri")
    call = subparsers.add_parser("call")
    call.add_argument("name")
    call.add_argument("arguments", nargs="?", default="{}")
    sequence = subparsers.add_parser("sequence")
    sequence.add_argument("calls")
    args = parser.parse_args()

    client = UnityMcpClient()
    initialized = client.initialize()
    if args.command == "list":
        result = client.request("tools/list")
    elif args.command == "schema":
        listed = client.request("tools/list")
        tools = listed.get("result", {}).get("tools", [])
        result = {
            "tools": [tool for tool in tools if tool.get("name") in args.names]
        }
    elif args.command == "resource":
        result = client.request("resources/read", {"uri": args.uri})
    elif args.command == "call":
        result = client.request(
            "tools/call",
            {"name": args.name, "arguments": load_json_argument(args.arguments)},
        )
    else:
        result = []
        for entry in load_json_argument(args.calls):
            result.append(
                client.request(
                    "tools/call",
                    {
                        "name": entry["name"],
                        "arguments": entry.get("arguments", {}),
                    },
                )
            )
    print(
        json.dumps(
            {
                "initialize": initialized,
                "session_id": client.session_id,
                "result": result,
            },
            ensure_ascii=False,
            indent=2,
        )
    )
    return 0


if __name__ == "__main__":
    sys.exit(main())
