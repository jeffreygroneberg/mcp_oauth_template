# MCP OAuth Template

A Model Context Protocol (MCP) server template with Azure AD OAuth authentication for secure todo management.

## What is this?

This is a complete example of an MCP server that:
- ✅ Authenticates users with Azure AD OAuth 2.0
- ✅ Provides secure todo management tools
- ✅ Tracks which user created/modified each todo
- ✅ Uses proper configuration management for secrets

## Quick Start

### 1. Prerequisites
- .NET 8.0 SDK
- Azure AD tenant with app registration

### 2. Azure AD Setup
1. Create an app registration in Azure AD
2. Add API scope: `api://{your-app-id}/mcp.tool`
3. Note your Tenant ID and App Client ID

### 3. Configuration
Create `appsettings.Development.json`:
```json
{
  "McpServer": {
    "TenantId": "your-tenant-id-here",
    "AppClientId": "your-app-client-id-here"
  }
}
```

### 4. Run
```bash
dotnet run
```

Server starts at `http://localhost:5115`

## MCP Tools Available

- **CreateTodo** - Create a new todo item
- **GetTodos** - Get all todos or a specific one by ID
- **UpdateTodo** - Update existing todo
- **DeleteTodo** - Delete a todo

All operations track the authenticated user automatically.

## Architecture

- **Azure AD OAuth 2.0** for authentication
- **JWT token validation** with proper claims extraction
- **User context tracking** for audit trails
- **Configuration externalization** for secure deployments
- **CORS enabled** for browser-based MCP clients

Perfect starting point for building secure MCP servers! 🚀
